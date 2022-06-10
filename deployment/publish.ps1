param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project,
    [Parameter(Mandatory = $true, Position = 1)]
    [System.String]
    $Runtime,
    [Parameter(Mandatory = $true, Position = 2)]
    [System.String]
    $Version,
    [Parameter(Mandatory = $true, Position = 3)]
    [System.Int32]
    $RunNumber,
    [Parameter(Position = 4)]
    [System.String]
    $Tag
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
