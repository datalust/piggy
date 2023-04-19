function Clean-Output
{
	if(Test-Path ./artifacts) { rm ./artifacts -Force -Recurse }
}

function Execute-Tests
{
    & dotnet test ./test/Datalust.Piggy.Tests/Datalust.Piggy.Tests.csproj -c Release /p:VersionPrefix=$version
    if($LASTEXITCODE -ne 0) { exit 3 }
}

function Create-ArtifactDir
{
	mkdir ./artifacts
}

function Publish-Gzips($version)
{
	$rids = @("linux-x64", "win-x64", "osx-x64")
	foreach ($rid in $rids) {
		& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -f net6.0 -r $rid /p:VersionPrefix=$version /p:PublishSingleFile=true /p:SelfContained=true /p:PublishTrimmed=true
	    if($LASTEXITCODE -ne 0) { exit 4 }

		# Make sure the archive contains a reasonable root filename
		mv ./src/Datalust.Piggy/bin/Release/net6.0/$rid/publish/ ./src/Datalust.Piggy/bin/Release/net6.0/$rid/piggy-$version-$rid/

        if ($rid -ne "win-x64") {
            & ./build/7-zip/7za.exe a -ttar piggy-$version-$rid.tar ./src/Datalust.Piggy/bin/Release/net6.0/$rid/piggy-$version-$rid/
            if($LASTEXITCODE -ne 0) { exit 5 }

            & ./build/7-zip/7za.exe a -tgzip ./artifacts/piggy-$version-$rid.tar.gz piggy-$version-$rid.tar
            if($LASTEXITCODE -ne 0) { exit 6 }

    		rm piggy-$version-$rid.tar
        } else {
            & ./build/7-zip/7za.exe a -tzip ./artifacts/piggy-$version-$rid.zip ./src/Datalust.Piggy/bin/Release/net6.0/$rid/piggy-$version-$rid/
            if($LASTEXITCODE -ne 0) { exit 7 }
        }

		# Back to the original directory name
		mv ./src/Datalust.Piggy/bin/Release/net6.0/$rid/piggy-$version-$rid/ ./src/Datalust.Piggy/bin/Release/net6.0/$rid/publish/
	}
}

function Publish-Nupkgs($version)
{
	& dotnet pack src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -o $PSScriptRoot/artifacts /p:VersionPrefix=$version /p:OutputType=Library
	if($LASTEXITCODE -ne 0) { exit 9 }
}

Push-Location $PSScriptRoot

$version = @{ $true = $env:APPVEYOR_BUILD_VERSION; $false = "99.99.99" }[$env:APPVEYOR_BUILD_VERSION -ne $NULL];
Write-Output "Building version $version"

Clean-Output
Execute-Tests
Create-ArtifactDir
Publish-Gzips($version)
Publish-Nupkgs($version)

Pop-Location
