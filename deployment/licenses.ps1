$dotnetArgs = @()
$dotnetArgs = $dotnetArgs + "tool"
$dotnetArgs = $dotnetArgs + "install"
$dotnetArgs = $dotnetArgs + "-g"
$dotnetArgs = $dotnetArgs + "dotnet-project-licenses"
$dotnetArgs = $dotnetArgs + "--prerelease"

& dotnet $dotnetArgs

$licenseArgs = @()
$licenseArgs = $licenseArgs + "-i" + ".\Gnomeshade.sln"
$licenseArgs = $licenseArgs + "-o" + "JsonPretty"
$licenseArgs = $licenseArgs + "-override" + ".\source\Gnomeshade.Avalonia.Core\Help\override.json"
$licenseArgs = $licenseArgs + "-t"

& dotnet-project-licenses $licenseArgs | Out-File -FilePath ".\source\Gnomeshade.Avalonia.Core\Help\licenses.json"
