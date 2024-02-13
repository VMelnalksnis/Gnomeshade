param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project,
    [Parameter(Mandatory = $true, Position = 1)]
    [System.String]
    $Runtime,
    [Parameter(Mandatory = $true, Position = 2)]
    [System.Int32]
    $RunNumber,
    [Parameter(Position = 3)]
    [System.String]
    $Tag
)

$version = Get-Content version

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "publish"
$dotnetArgs = $dotnetArgs + ".\tests\$Project\$Project.csproj"
$dotnetArgs = $dotnetArgs + "--runtime" + $Runtime
$dotnetArgs = $dotnetArgs + "--configuration" + "Release"
$dotnetArgs = $dotnetArgs + "--self-contained"
$dotnetArgs = $dotnetArgs + "-p:AssemblyVersion=$version.$RunNumber"
$dotnetArgs = $dotnetArgs + "-p:InformationalVersion=$version$Tag+$Runtime"
$dotnetArgs = $dotnetArgs + "-p:PublishSingleFile=false"
$dotnetArgs = $dotnetArgs + "/warnAsError"
$dotnetArgs = $dotnetArgs + "/nologo"

& dotnet $dotnetArgs

$testArgs = @()
$testArgs = $testArgs + "test"
$testArgs = $testArgs + ".\tests\$Project\bin\Release\net8.0\$Runtime\$Project.dll"

& dotnet $testArgs
