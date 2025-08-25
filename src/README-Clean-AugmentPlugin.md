# Augment插件残留文件清理工具 v2.0

## 概述

这是一个增强版的PowerShell脚本，用于彻底清理Augment插件在Windows系统中的所有残留文件，包括扩展目录、配置文件、数据存储和日志文件。

## 功能特性

### 🎯 **全面清理**
- **扩展目录**：VSCode和Trae扩展安装目录
- **配置文件**：settings.json、keybindings.json、extensions.json中的Augment相关配置
- **数据存储**：全局存储、工作区存储、缓存扩展
- **日志文件**：VSCode日志目录中的Augment相关日志（可选）

### 🛡️ **安全机制**
- **VSCode运行检测**：自动检测VSCode是否正在运行并提醒
- **配置文件备份**：修改配置文件前自动创建备份
- **模拟模式**：支持预览将要删除的内容
- **用户确认**：删除前可选择性提示确认

### 📊 **详细报告**
- **分阶段清理**：按类型分阶段执行清理操作
- **彩色输出**：不同类型信息使用不同颜色显示
- **统计信息**：显示发现、删除、修改的项目数量
- **错误处理**：完整的异常捕获和错误报告

## 清理范围

### 1. 扩展目录
- `C:\Users\[用户名]\.vscode\extensions\augment.vscode-augment-0.521.0`
- `C:\Users\[用户名]\.trae\extensions\augment.vscode-augment-0.521.0`

### 2. VSCode数据目录
- `C:\Users\[用户名]\AppData\Roaming\Code\User\globalStorage\augment.*`
- `C:\Users\[用户名]\AppData\Roaming\Code\User\workspaceStorage\*augment*`
- `C:\Users\[用户名]\AppData\Roaming\Code\CachedExtensions\augment.*`

### 3. 日志文件（可选）
- `C:\Users\[用户名]\AppData\Roaming\Code\logs\*augment*`

### 4. 配置文件清理
- **settings.json**：移除所有augment相关的设置项
- **keybindings.json**：移除所有augment相关的快捷键绑定
- **extensions.json**：移除工作区中的augment扩展推荐

## 使用方法

### 基本语法
```powershell
.\Clean-AugmentPlugin.ps1 [参数]
```

### 参数说明
- `-Force`：强制删除，跳过所有确认提示
- `-WhatIf`：模拟模式，仅显示将要删除的内容，不实际执行
- `-CleanLogs`：同时清理日志文件（默认跳过）
- `-SkipBackup`：跳过配置文件备份

### 使用示例

#### 1. 推荐使用流程
```powershell
# 第一步：模拟运行，预览将要清理的内容
.\Clean-AugmentPlugin.ps1 -WhatIf

# 第二步：确认无误后执行实际清理
.\Clean-AugmentPlugin.ps1
```

#### 2. 其他使用方式
```powershell
# 强制清理，不提示确认
.\Clean-AugmentPlugin.ps1 -Force

# 包含日志文件的完整清理
.\Clean-AugmentPlugin.ps1 -CleanLogs

# 跳过备份的快速清理
.\Clean-AugmentPlugin.ps1 -Force -SkipBackup -CleanLogs

# 仅模拟完整清理过程
.\Clean-AugmentPlugin.ps1 -WhatIf -CleanLogs
```

## 清理过程

脚本按以下6个阶段执行清理：

### 第一阶段：清理扩展目录
- 删除VSCode和Trae中的Augment扩展文件夹

### 第二阶段：清理VSCode数据目录
- 清理全局存储、工作区存储、缓存扩展中的Augment相关数据

### 第三阶段：清理日志文件（可选）
- 删除VSCode日志目录中的Augment相关日志文件

### 第四阶段：清理配置文件
- 从用户配置文件中移除Augment相关设置和快捷键

### 第五阶段：检查工作区扩展推荐
- 清理当前目录及子目录中.vscode/extensions.json的Augment推荐

### 第六阶段：额外检查
- 查找其他可能遗漏的Augment相关文件

## 安全注意事项

### ⚠️ **重要提醒**
1. **关闭VSCode**：建议在运行脚本前关闭所有VSCode实例
2. **备份重要数据**：脚本会自动备份配置文件，但建议手动备份重要的工作区数据
3. **管理员权限**：建议以管理员权限运行以确保完全访问权限

### 🔒 **安全机制**
- 自动检测VSCode运行状态并提醒
- 配置文件修改前自动创建时间戳备份
- 只删除明确匹配Augment模式的文件和配置
- 提供详细的操作日志和错误信息

## 备份和恢复

### 配置文件备份
脚本会自动为修改的配置文件创建备份：
- 格式：`原文件名.backup_YYYYMMDD_HHMMSS`
- 位置：与原文件相同目录

### 恢复方法
如需恢复配置文件：
```powershell
# 示例：恢复settings.json
Copy-Item "settings.json.backup_20241225_143022" "settings.json" -Force
```

## 故障排除

### 常见问题

**Q: 脚本提示权限不足**
A: 以管理员身份运行PowerShell，然后执行脚本

**Q: VSCode仍显示Augment相关设置**
A: 重启VSCode，设置应该会消失

**Q: 工作区存储删除后数据丢失**
A: 检查是否有备份文件，工作区存储通常包含扩展的临时数据

**Q: 脚本执行被阻止**
A: 运行 `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

### 日志分析
脚本提供详细的彩色日志输出：
- 🟢 绿色：成功操作和信息
- 🟡 黄色：警告和模拟操作
- 🔴 红色：错误信息
- 🔵 青色：阶段标题和成功完成
- ⚪ 白色：一般信息

## 版本历史

### v2.0 (当前版本)
- ✅ 新增VSCode配置文件清理功能
- ✅ 新增数据存储和缓存清理
- ✅ 新增日志文件清理（可选）
- ✅ 新增配置文件备份机制
- ✅ 新增VSCode运行状态检测
- ✅ 新增分阶段清理流程
- ✅ 改进错误处理和用户体验

### v1.0
- ✅ 基础扩展目录清理功能
- ✅ 用户确认和模拟模式
- ✅ 基本错误处理

## 技术支持

如遇到问题或需要帮助，请检查：
1. PowerShell执行策略设置
2. 用户权限（建议管理员权限）
3. VSCode是否完全关闭
4. 备份文件是否正确创建

---

**注意**：此工具专门针对Augment插件版本0.521.0设计，其他版本可能需要调整版本号参数。
