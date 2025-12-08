# 多功能工具箱应用

这是一个基于WPF框架开发的多功能工具箱应用，提供JSON格式化和图片/Base64转换功能。

## 功能特性

### JSON格式化工具
- ✨ **美化格式**: 将压缩的JSON字符串转换为易读的缩进格式
- 🗜️ **压缩格式**: 移除JSON中的空白字符，生成紧凑格式
- ✅ **验证JSON**: 检查JSON字符串的语法正确性
- 📋 **一键复制**: 快速复制处理结果到剪贴板
- 📁 **拖拽支持**: 支持拖拽JSON文件到输入框

### 图片/Base64转换工具
- 🖼️ **图片选择**: 支持选择本地图片文件
- 📋 **粘贴图片**: 直接从剪贴板粘贴图片
- 🔄 **双向转换**: 图片转Base64、Base64转图片
- 💾 **文件保存**: 支持将Base64转换为图片文件并保存
- 🎨 **多格式支持**: 支持JPG、PNG、GIF、BMP等常见图片格式

## 技术栈
- **语言**: C#
- **框架**: WPF (.NET 8.0)
- **UI设计**: 简约现代风格，清爽视觉效果
- **架构**: 模块化设计，易于扩展

## 编译和运行

### 方法1: 使用批处理文件
1. 双击运行 `build.bat` 文件
2. 等待编译完成
3. 运行生成的exe文件

### 方法2: 使用命令行
```bash
# 恢复NuGet包
dotnet restore

# 编译项目
dotnet build --configuration Release

# 运行应用
dotnet run --project WpfTools/WpfTools.csproj
```

### 方法3: 发布为独立exe文件
```bash
# 发布为单文件独立exe
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# exe文件位置: WpfTools\bin\Release\net8.0-windows\win-x64\publish\WpfTools.exe
```

## 文件结构
```
WpfTools/
├── MainWindow.xaml          # 主窗口界面定义
├── MainWindow.xaml.cs       # 主窗口逻辑代码
├── Tools/                   # 工具类目录
│   ├── JsonFormatter.cs     # JSON格式化工具
│   ├── ImageBase64Converter.cs  # 图片/Base64转换工具
│   └── ToolHelper.cs        # 辅助工具类
├── App.xaml                 # 应用程序资源
├── App.xaml.cs              # 应用程序启动逻辑
└── WpfTools.csproj          # 项目文件
```

## 使用说明

### JSON格式化
1. 切换到"JSON格式化工具"标签页
2. 在输入框中粘贴或输入JSON字符串
3. 选择相应的操作按钮：
   - **美化格式**: 将JSON格式化为易读格式
   - **压缩格式**: 移除空白字符生成紧凑格式
   - **验证JSON**: 检查JSON语法正确性
   - **复制结果**: 将处理结果复制到剪贴板

### 图片/Base64转换
1. 切换到"图片/Base64转换"标签页
2. 选择图片：
   - 点击"选择图片"按钮选择本地文件
   - 点击"粘贴图片"按钮从剪贴板粘贴
3. 执行转换：
   - **图片转Base64**: 将图片转换为Base64编码
   - **Base64转图片**: 将Base64字符串转换为图片
   - **复制Base64**: 复制Base64字符串到剪贴板

## 系统要求
- Windows 10/11
- .NET 8.0 Runtime (如果使用非独立发布版本)

## 开发说明
项目采用模块化设计，核心功能分别封装在对应的工具类中：
- `JsonFormatter`: 处理JSON相关操作
- `ImageBase64Converter`: 处理图片和Base64转换
- `ToolHelper`: 提供通用辅助方法

这种设计便于后续添加新的工具功能，只需在Tools目录下添加新的工具类，并在主界面中集成即可。

## 许可证
本项目仅供学习和个人使用。