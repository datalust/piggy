Write-Output "build: Build started"
Push-Location $PSScriptRoot

Write-Output "build: Loading sdk.version from global.json"
$json = Get-Content 'global.json' | Out-String | ConvertFrom-Json

$sdkVersion = $json.sdk.version
Write-Output "build: Downloading .NET SDK $sdkVersion"
Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'

Write-Output "build: Installing .NET SDK $sdkVersion"
./dotnet-install.ps1 -Version $sdkVersion

if(Test-Path .\artifacts) {
	Write-Output "build: Cleaning .\artifacts"
	Remove-Item .\artifacts -Force -Recurse
}

Write-Output "build: Restoring .NET packages (--no-cache)"
& dotnet restore --no-cache
if($LASTEXITCODE -ne 0) { exit 1 }

Write-Output "build: Calculating `$version"
$version = @{ $true = $env:APPVEYOR_BUILD_VERSION; $false = "99.99.99" }[$NULL -ne $env:APPVEYOR_BUILD_VERSION];
Write-Output "build: Building version $version"

Write-Output "build: Calculating `$branch, `$revision, and `$suffix"
$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$NULL -ne $env:APPVEYOR_REPO_BRANCH];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$NULL -ne $env:APPVEYOR_BUILD_NUMBER];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "main" -and $revision -ne "local"]
Write-Output "build: Version suffix is $suffix"

Write-Output "build: Testing project"
& dotnet test ./test/Datalust.Piggy.Tests/Datalust.Piggy.Tests.csproj -c Release /p:VersionPrefix=$version
if($LASTEXITCODE -ne 0) { exit 2 }

Write-Output "build: Creating artifacts directory"
New-Item -ItemType "directory" -Name "artifacts"

$rids = @("win-x64", "linux-x64", "osx-x64")
foreach ($rid in $rids) {
	Write-Output "build: Building a $rid build"

	if ($suffix) {
		& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -f net7.0 -r $rid /p:VersionPrefix=$version /p:PublishSingleFile=true /p:SelfContained=true /p:PublishTrimmed=true --version-suffix=$suffix
	} else {
		& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -f net7.0 -r $rid /p:VersionPrefix=$version /p:PublishSingleFile=true /p:SelfContained=true /p:PublishTrimmed=true
	}
	if($LASTEXITCODE -ne 0) { exit 3 }

	# Make sure the archive contains a reasonable root filename
	mv ./src/Datalust.Piggy/bin/Release/net7.0/$rid/publish/ ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/

	if ($rid -ne "win-x64") {
		& ./build/7-zip/7za.exe a -ttar piggy-$version-$rid.tar ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/
		if($LASTEXITCODE -ne 0) { exit 4 }

		& ./build/7-zip/7za.exe a -tgzip ./artifacts/piggy-$version-$rid.tar.gz piggy-$version-$rid.tar
		if($LASTEXITCODE -ne 0) { exit 5 }

		rm piggy-$version-$rid.tar
	} else {
		& ./build/7-zip/7za.exe a -tzip ./artifacts/piggy-$version-$rid.zip ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/
		if($LASTEXITCODE -ne 0) { exit 6 }
	}

	# Back to the original directory name
	mv ./src/Datalust.Piggy/bin/Release/net7.0/$rid/piggy-$version-$rid/ ./src/Datalust.Piggy/bin/Release/net7.0/$rid/publish/
}

& dotnet pack src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -o $PSScriptRoot/artifacts /p:VersionPrefix=$version /p:OutputType=Library
if($LASTEXITCODE -ne 0) { exit 7 }

Pop-Location
