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

Set-PSDebug -Trace 1

$version = Get-Content version

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "publish"
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.csproj"
$dotnetArgs = $dotnetArgs + "--runtime" + $Runtime
$dotnetArgs = $dotnetArgs + "--configuration" + "Release"
$dotnetArgs = $dotnetArgs + "--self-contained"
$dotnetArgs = $dotnetArgs + "--no-restore"
$dotnetArgs = $dotnetArgs + "-p:AssemblyVersion=$version.$RunNumber"
$dotnetArgs = $dotnetArgs + "-p:InformationalVersion=$version$Tag+$Runtime"
$dotnetArgs = $dotnetArgs + "-p:DebuggerSupport=false"
$dotnetArgs = $dotnetArgs + "-p:DebugSymbols=false"
$dotnetArgs = $dotnetArgs + "-p:DebugType=None"
$dotnetArgs = $dotnetArgs + "-p:TrimmerRemoveSymbols=true"
$dotnetArgs = $dotnetArgs + "-p:StripSymbols=true"
$dotnetArgs = $dotnetArgs + "/warnAsError"
$dotnetArgs = $dotnetArgs + "/nologo"

& dotnet $dotnetArgs
