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

Write-Output "artifact-name=$archive_name" >> "$GITHUB_OUTPUT"
Write-Output "artifact=$publish_dir\$archive_name" >> "$GITHUB_OUTPUT"
