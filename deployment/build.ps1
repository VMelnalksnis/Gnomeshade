param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project
)

Set-PSDebug -Trace 1

$publish_dir = "source\$Project\bin\Release\"
$archive_name = "${Project}.msi"

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "build"
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.wixproj"
$dotnetArgs = $dotnetArgs + "--configuration" + "Release"
$dotnetArgs = $dotnetArgs + "--no-restore"
$dotnetArgs = $dotnetArgs + "--verbosity" + "diagnostic"
$dotnetArgs = $dotnetArgs + "/nologo"

& dotnet $dotnetArgs

"artifact-name=$archive_name" >> $env:GITHUB_OUTPUT
"artifact<<EOF" >> $env:GITHUB_OUTPUT
"$publish_dir\$archive_name" >> $env:GITHUB_OUTPUT
"EOF" >> $env:GITHUB_OUTPUT
