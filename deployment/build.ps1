param (
    [Parameter(Mandatory = $true, Position = 0)]
    [System.String]
    $Project
)

$publish_dir = "source\$Project\bin\Release\"
$archive_name = "${Project}.msi"

$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "build"
$dotnetArgs = $dotnetArgs + ".\source\$Project\$Project.wixproj"
$dotnetArgs = $dotnetArgs + "-property:Configuration=Release"
$dotnetArgs = $dotnetArgs + "/warnAsError"
$dotnetArgs = $dotnetArgs + "/nologo"

& msbuild $dotnetArgs

Write-Output "::set-output name=artifact-name::$archive_name"
Write-Output "::set-output name=artifact::$publish_dir\$archive_name"
