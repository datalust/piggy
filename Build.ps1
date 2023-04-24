$ErrorActionPreference = 'Stop'

Write-Output "build: Build started"
Push-Location $PSScriptRoot

Write-Output "build: Loading sdk.version from global.json"
$json = Get-Content 'global.json' | Out-String | ConvertFrom-Json
$requiredSdkVersion = $json.sdk.version
Write-Output "build: Required sdk.version is $requiredSdkVersion"

Write-Output "build: Downloading .NET SDK $requiredSdkVersion"
Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'

Write-Output "build: Installing .NET SDK $requiredSdkVersion"
./dotnet-install.ps1 -Version $requiredSdkVersion

Write-Output "build: Checking installed .NET SDK versions"
$installedSdkVersions = & dotnet --list-sdks
Write-Output "build: Installed .NET SDK versions:"
Write-Output $installedSdkVersions

if($installedSdkVersions -inotmatch $requiredSdkVersion) {
	Write-Output "build: Didn't find .NET SDK $requiredSdkVersion installed"
	Write-Output $installedSdkVersions
	exit 1
}
Write-Output "build: .NET SDK $requiredSdkVersion is installed and available for use"

$artifacts = "artifacts"
if(Test-Path .\$artifacts) {
	Write-Output "build: Cleaning .\$artifacts"
	Remove-Item .\$artifacts -Force -Recurse
}

Write-Output "build: Restoring .NET packages (--no-cache)"
& dotnet restore --no-cache
if($LASTEXITCODE -ne 0) { exit 2 }

Write-Output "build: Testing project"
& dotnet test ./test/Datalust.Piggy.Tests/Datalust.Piggy.Tests.csproj -c Release
if($LASTEXITCODE -ne 0) { exit 3 }

$projectDir = "src/Datalust.Piggy/"
$projectFile = "Datalust.Piggy.csproj"
$project = $projectDir + $projectFile
Write-Output "build: Project .csproj file is located at $project"

Write-Output "build: Getting Version from $project"
$xml = [Xml] (Get-Content $project)
$prefix = [Version] $xml.Project.PropertyGroup.VersionPrefix.ToString()
Write-Output "build: Version prefix is $prefix"

Write-Output "build: Calculating `$branch, `$revision, and `$suffix"
$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$NULL -ne $env:APPVEYOR_REPO_BRANCH];
Write-Output "build: `$branch is $branch"
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$NULL -ne $env:APPVEYOR_BUILD_NUMBER];
Write-Output "build: `$revision is $revision"
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "main" -and $revision -ne "local"]
Write-Output "build: `$suffix is $suffix"

$version= "$prefix-$suffix"
Write-Output "build: Version is $version"

Write-Output "build: Creating artifacts directory at .\$artifacts"
New-Item -ItemType "directory" -Name $artifacts

$rids = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")
foreach ($rid in $rids) {
	Write-Output "build: Building a $rid build of version $version"

	if ($suffix) {
		& dotnet publish $project -c Release -f net7.0 -r $rid /p:PublishSingleFile=true /p:SelfContained=true /p:PublishTrimmed=true --version-suffix=$suffix
	} else {
		& dotnet publish $project -c Release -f net7.0 -r $rid /p:PublishSingleFile=true /p:SelfContained=true /p:PublishTrimmed=true
	}
	if($LASTEXITCODE -ne 0) { exit 4 }

	# Make sure the archive contains a reasonable root filename.
	mv ./src/Datalust.Piggy/bin/Release/net7.0/$rid/publish/ ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/

	if ($rid -ne "win-x64") {
		& ./build/7-zip/7za.exe a -ttar piggy-$version-$rid.tar ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/
		if($LASTEXITCODE -ne 0) { exit 5 }

		& ./build/7-zip/7za.exe a -tgzip ./$artifacts/piggy-$version-$rid.tar.gz piggy-$version-$rid.tar
		if($LASTEXITCODE -ne 0) { exit 6 }

		rm piggy-$version-$rid.tar
	} else {
		& ./build/7-zip/7za.exe a -tzip ./$artifacts/piggy-$version-$rid.zip ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/
		if($LASTEXITCODE -ne 0) { exit 7 }
	}

	# Move back to the original directory name.
	mv ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/ ./src/Datalust.Piggy/bin/Release/net7.0/$rid/publish/
}

Write-Output "build: Packing library into .nupkg"
if ($suffix) {
	& dotnet pack $project -c Release -o $PSScriptRoot/$artifacts /p:OutputType=Library --version-suffix=$suffix
} else {
	& dotnet pack $project -c Release -o $PSScriptRoot/$artifacts /p:OutputType=Library
}
if($LASTEXITCODE -ne 0) { exit 8 }

Pop-Location
