# 编译 Release 版 exe
$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "渔人的直感\渔人的直感.csproj"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

$msbuild = $null
if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
}

if (-not $msbuild) {
    $candidates = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    $msbuild = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $msbuild) {
    Write-Error "未找到 MSBuild。请安装 Visual Studio（含“.NET 桌面开发”工作负载）后重试。"
}

Write-Host "使用 MSBuild: $msbuild"
& $msbuild $project /p:Configuration=Release /p:Platform=AnyCPU /restore /v:minimal

$exe = Join-Path $PSScriptRoot "渔人的直感\bin\Release\渔人的直感.exe"
if (Test-Path $exe) {
    $info = Get-Item $exe
    Write-Host ""
    Write-Host "编译成功:"
    Write-Host $info.FullName
    Write-Host ("大小: {0:N0} 字节" -f $info.Length)
    Write-Host ("时间: {0}" -f $info.LastWriteTime)
} else {
    Write-Error "编译失败，未找到输出文件。"
}
