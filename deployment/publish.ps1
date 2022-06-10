param (
    [Parameter(Mandatory = $true)]
    [System.String]
    $Project,
    [Parameter(Mandatory = $true)]
    [System.String]
    $Runtime,
    [Parameter(Mandatory = $true)]
    [System.String]
    $Version,
    [System.String]
    $Tag,
    [Parameter(Mandatory = $true)]
    [System.Int32]
    $RunNumber
)

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "publish"
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.csproj"
$dotnetArgs = $dotnetArgs + "--runtime" + $Runtime
$dotnetArgs = $dotnetArgs + "--configuration" + "Release"
$dotnetArgs = $dotnetArgs + "--self-contained"
$dotnetArgs = $dotnetArgs + "-p:PublishSingleFile=true"
$dotnetArgs = $dotnetArgs + "-p:AssemblyVersion=$Version.$RunNumber"
$dotnetArgs = $dotnetArgs + "-p:InformationalVersion=$Version$Tag+$Runtime"
$dotnetArgs = $dotnetArgs + "/warnAsError"
$dotnetArgs = $dotnetArgs + "/nologo"

& dotnet $dotnetArgs

Push-Location source\$Project\bin\Release\net6.0\$Runtime\publish
& 7z a -mx9 -r -w "${Project}_$Runtime.zip"
Pop-Location
