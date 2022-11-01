param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project
)

$publish_dir = "source\$Project\bin\Release\"
$archive_name = "${Project}.msi"

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.wixproj"
$dotnetArgs = $dotnetArgs + "-property:Configuration=Release"
$dotnetArgs = $dotnetArgs + "/nologo"

& msbuild $dotnetArgs

"artifact-name=$archive_name" >> $env:GITHUB_OUTPUT
"artifact<<EOF" >> $env:GITHUB_OUTPUT
"$publish_dir\$archive_name" >> $env:GITHUB_OUTPUT
"EOF" >> $env:GITHUB_OUTPUT
