param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project,
    [Parameter(Mandatory = $true, Position = 1)]
    [System.String]
    $Runtime,
    [Parameter(Mandatory = $true, Position = 3)]
    [System.Int32]
    $RunNumber,
    [Parameter(Position = 4)]
    [System.String]
    $Tag
)

$version = Get-Content version
$publish_dir = "source\$Project\bin\Release\net6.0\$Runtime\publish"
$archive_name = "${Project}_$Runtime.zip"

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "publish"
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.csproj"
$dotnetArgs = $dotnetArgs + "--runtime" + $Runtime
$dotnetArgs = $dotnetArgs + "--configuration" + "Release"
$dotnetArgs = $dotnetArgs + "--self-contained"
$dotnetArgs = $dotnetArgs + "--no-restore"
$dotnetArgs = $dotnetArgs + "-p:PublishSingleFile=true"
$dotnetArgs = $dotnetArgs + "-p:AssemblyVersion=$version.$RunNumber"
$dotnetArgs = $dotnetArgs + "-p:InformationalVersion=$version$Tag+$Runtime"
$dotnetArgs = $dotnetArgs + "/warnAsError"
$dotnetArgs = $dotnetArgs + "/nologo"

& dotnet $dotnetArgs

Push-Location $publish_dir
& 7z a -mx9 -r -w $archive_name
Pop-Location

$env:GITHUB_OUTPUT += "`nartifact-name=$archive_name"
$env:GITHUB_OUTPUT += "`nartifact=$publish_dir\$archive_name"
