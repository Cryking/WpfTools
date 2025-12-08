# PowerShell脚本：快速编译WPF工具箱应用
Write-Host "开始编译WPF工具箱应用..." -ForegroundColor Green

# 检查.NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "检测到.NET SDK版本: $dotnetVersion" -ForegroundColor Cyan
} catch {
    Write-Host "错误: 未检测到.NET SDK，请先安装.NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# 恢复NuGet包
Write-Host "恢复NuGet包..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "恢复包失败！" -ForegroundColor Red
    exit 1
}

# 编译项目
Write-Host "编译项目..." -ForegroundColor Yellow
dotnet build WpfTools/WpfTools.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}

# 检查输出文件
$outputPath = "WpfTools/bin/Release/net8.0-windows/WpfTools.exe"
if (Test-Path $outputPath) {
    $fileSize = (Get-Item $outputPath).Length / 1MB
    Write-Host "编译成功！" -ForegroundColor Green
    Write-Host "输出文件: $outputPath" -ForegroundColor Cyan
    Write-Host "文件大小: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Host "警告: 未找到输出文件" -ForegroundColor Yellow
}

Write-Host "编译完成！" -ForegroundColor Green
Read-Host "按任意键继续..."