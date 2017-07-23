function Clean-Output
{
	if(Test-Path ./artifacts) { rm ./artifacts -Force -Recurse }
}

function Restore-Packages
{
	& dotnet restore
}

function Update-AssemblyInfo($version)
{  
    $versionPattern = "[0-9]+(\.([0-9]+|\*)){3}"

    foreach ($file in ls ./src/*/Properties/AssemblyInfo.cs)  
    {     
        (cat $file) | foreach {  
                % {$_ -replace $versionPattern, "$version.0" }             
            } | sc -Encoding "UTF8" $file                                 
    }  
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

function Execute-MSBuild
{
	& msbuild ./piggy.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64
    if($LASTEXITCODE -ne 0) { exit 2 }
}

function Execute-Tests
{
    pushd ./test/Datalust.Piggy.Tests

    & dotnet test -c Release
    if($LASTEXITCODE -ne 0) { exit 3 }

    popd
}

function Create-ArtifactDir
{
	mkdir ./artifacts
}

function Publish-Gzips($version)
{
	$rids = @("ubuntu.14.04-x64", "ubuntu.16.04-x64", "rhel.7-x64", "osx.10.12-x64")
	foreach ($rid in $rids) {
		& dotnet publish src/Datalust.Piggy/Datalust.Piggy.csproj -c Release -r $rid
	    if($LASTEXITCODE -ne 0) { exit 3 }
	
		& ./build/7-zip/7za.exe a -ttar piggy-$version-$rid.tar ./src/Datalust.Piggy/bin/Release/netcoreapp1.1/$rid/publish/*
		if($LASTEXITCODE -ne 0) { exit 3 }
		
		& ./build/7-zip/7za.exe a -tgzip ./artifacts/piggy-$version-$rid.tar.gz piggy-$version-$rid.tar
		if($LASTEXITCODE -ne 0) { exit 3 }

		rm piggy-$version-$rid.tar
	}
}

function Publish-Msi($version)
{
	mv ./setup/Datalust.Piggy.Setup/bin/Release/piggy.msi ./artifacts/piggy-$version.msi
}

Push-Location $PSScriptRoot

$version = @{ $true = $env:APPVEYOR_BUILD_VERSION; $false = "0.0.0" }[$env:APPVEYOR_BUILD_VERSION -ne $NULL];

Clean-Output
Restore-Packages
Update-AssemblyInfo($version)
Update-WixVersion($version)
Execute-MSBuild
Execute-Tests
Create-ArtifactDir
Publish-Gzips($version)
Publish-Msi($version)

Pop-Location
