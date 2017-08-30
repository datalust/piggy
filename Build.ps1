function Clean-Output
{
	if(Test-Path ./artifacts) { rm ./artifacts -Force -Recurse }
}

function Restore-Packages
{
	& dotnet restore
}

function Update-WixVersion($version)
{
    $defPattern = "define Version = ""0\.0\.0"""
	$def = "define Version = ""$version"""
    $product = ".\setup\Datalust.Piggy.Setup\Product.wxs"

    (cat $product) | foreach {  
            % {$_ -replace $defPattern, $def }    
        } | sc -Encoding "UTF8" $product
}

function Execute-MSBuild($version)
{
	& msbuild ./piggy.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64 /p:VersionPrefix=$version
    if($LASTEXITCODE -ne 0) { exit 2 }
}

function Execute-Tests
{
    & dotnet test ./test/Datalust.Piggy.Tests/Datalust.Piggy.Tests.csproj -c Release /p:Configuration=Release /p:Platform=x64 /p:VersionPrefix=$version
    if($LASTEXITCODE -ne 0) { exit 3 }
}

function Create-ArtifactDir
{
	mkdir ./artifacts
}

function Publish-Gzips($version)
{
	$rids = @("ubuntu.14.04-x64", "ubuntu.16.04-x64", "rhel.7-x64", "osx.10.12-x64")
	foreach ($rid in $rids) {
		& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -r $rid /p:VersionPrefix=$version
	    if($LASTEXITCODE -ne 0) { exit 4 }
	
		# Make sure the archive contains a reasonable root filename
		mv ./src/Datalust.Piggy/bin/Release/netcoreapp2.0/$rid/publish/ ./src/Datalust.Piggy/bin/Release/netcoreapp2.0/$rid/piggy-$version-$rid/

		& ./build/7-zip/7za.exe a -ttar piggy-$version-$rid.tar ./src/Datalust.Piggy/bin/Release/netcoreapp2.0/$rid/piggy-$version-$rid/
		if($LASTEXITCODE -ne 0) { exit 5 }

		# Back to the original directory name
		mv ./src/Datalust.Piggy/bin/Release/netcoreapp2.0/$rid/piggy-$version-$rid/ ./src/Datalust.Piggy/bin/Release/netcoreapp2.0/$rid/publish/
		
		& ./build/7-zip/7za.exe a -tgzip ./artifacts/piggy-$version-$rid.tar.gz piggy-$version-$rid.tar
		if($LASTEXITCODE -ne 0) { exit 6 }

		rm piggy-$version-$rid.tar
	}
}

function Publish-Msi($version)
{
	& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -r win10-x64 /p:VersionPrefix=$version
	if($LASTEXITCODE -ne 0) { exit 7 }

	& msbuild ./setup/Datalust.Piggy.Setup/Datalust.Piggy.Setup.wixproj /t:Rebuild /p:Configuration=Release /p:Platform=x64 /p:Version=$version
	if($LASTEXITCODE -ne 0) { exit 8 }

	mv ./setup/Datalust.Piggy.Setup/bin/Release/piggy.msi ./artifacts/piggy-$version.msi
}

Push-Location $PSScriptRoot

$version = @{ $true = $env:APPVEYOR_BUILD_VERSION; $false = "99.99.99" }[$env:APPVEYOR_BUILD_VERSION -ne $NULL];
Write-Output "Building version $version"

Clean-Output
Restore-Packages
Update-WixVersion($version)
Execute-MSBuild($version)
Execute-Tests
Create-ArtifactDir
Publish-Gzips($version)
Publish-Msi($version)

Pop-Location
