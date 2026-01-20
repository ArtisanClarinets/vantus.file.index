# Build-Installer.ps1
param (
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0",
    [string]$CertificatePath = "",
    [string]$CertificatePassword = ""
)

$ErrorActionPreference = "Stop"
$scriptPath = $PSScriptRoot
$rootPath = Resolve-Path (Join-Path $scriptPath "..")

Write-Host "Vantus File Indexer - Enterprise Installer Build" -ForegroundColor Cyan
Write-Host "Root Path: $rootPath" -ForegroundColor Gray

# 1. Check Requirements
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) { Throw "dotnet SDK is required." }
if (-not (Get-Command "wix" -ErrorAction SilentlyContinue)) { 
    Write-Warning "WiX Toolset not found. Attempting to install..."
    dotnet tool install --global wix
}

# Ensure extensions
Write-Host "Ensuring WiX extensions..."
wix extension add WixToolset.UI.wixext --global

# 2. Publish Vantus.App
$publishDir = Join-Path $rootPath "publish"
Write-Host "Cleaning publish directory: $publishDir"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir | Out-Null

Write-Host "Publishing Vantus.App ($Configuration)..."
dotnet publish (Join-Path $rootPath "Vantus.App\Vantus.App.csproj") -c $Configuration -r win-x64 --self-contained -o $publishDir -p:Version=$Version

Write-Host "Publishing Vantus.Engine ($Configuration)..."
$enginePublishDir = Join-Path $publishDir "Engine"
dotnet publish (Join-Path $rootPath "Vantus.Engine\Vantus.Engine.csproj") -c $Configuration -r win-x64 --self-contained -o $enginePublishDir -p:Version=$Version

# 3. Generate Components.wxs
Write-Host "Generating Components.wxs..."
$componentsFile = Join-Path $scriptPath "Components.wxs"

$sb = new-object System.Text.StringBuilder
$sb.AppendLine('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">') | Out-Null
$sb.AppendLine('  <Fragment>') | Out-Null
$sb.AppendLine('    <ComponentGroup Id="AppComponents" Directory="INSTALLFOLDER">') | Out-Null

# Harvest files (App)
$files = Get-ChildItem $publishDir -File
foreach ($file in $files) {
    $id = "cmp_" + [System.Guid]::NewGuid().ToString("N")
    # Source path relative to build base (root)
    $source = "publish\" + $file.Name
    $sb.AppendLine("      <Component Id=""$id"" Guid=""*"">") | Out-Null
    $sb.AppendLine("        <File Id=""file_$id"" Source=""$source"" KeyPath=""yes"" />") | Out-Null
    $sb.AppendLine("      </Component>") | Out-Null
}

$sb.AppendLine('    </ComponentGroup>') | Out-Null

$sb.AppendLine('    <ComponentGroup Id="EngineComponents" Directory="ENGINEFOLDER">') | Out-Null
# Harvest files (Engine)
$engineFiles = Get-ChildItem $enginePublishDir -File
foreach ($file in $engineFiles) {
    $id = "cmp_eng_" + [System.Guid]::NewGuid().ToString("N")
    $source = "publish\Engine\" + $file.Name
    $sb.AppendLine("      <Component Id=""$id"" Guid=""*"">") | Out-Null
    $sb.AppendLine("        <File Id=""file_$id"" Source=""$source"" KeyPath=""yes"" />") | Out-Null
    $sb.AppendLine("      </Component>") | Out-Null
}
$sb.AppendLine('    </ComponentGroup>') | Out-Null

$sb.AppendLine('  </Fragment>') | Out-Null
$sb.AppendLine('</Wix>') | Out-Null

$sb.ToString() | Out-File $componentsFile -Encoding UTF8

# 4. Build MSI
$msiName = "VantusSetup-$Version.msi"
$msiPath = Join-Path $rootPath $msiName
Write-Host "Building MSI: $msiPath"

wix build -o $msiPath -b $rootPath `
    (Join-Path $scriptPath "Package.wxs") `
    (Join-Path $scriptPath "Components.wxs") `
    (Join-Path $scriptPath "Shortcuts.wxs") `
    -ext WixToolset.UI.wixext

# 5. Sign MSI (Enterprise Requirement)
if (-not [string]::IsNullOrEmpty($CertificatePath)) {
    Write-Host "Signing MSI..."
    # Placeholder for signtool or wix sign
    # & "signtool.exe" sign /f $CertificatePath /p $CertificatePassword /t http://timestamp.digicert.com $msiPath
    Write-Host "Signing completed (simulated)."
} else {
    Write-Warning "No certificate provided. MSI is unsigned (not ready for production deployment)."
}

Write-Host "Build Complete! Installer available at: $msiPath" -ForegroundColor Green
