using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AugmentCleaner
{
    public static class FileHelper
    {
        public static async Task<bool> RemoveDirectorySafelyAsync(string path, string description, bool whatIf, bool force)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Logger.WriteLog($"{description} 目录不存在: {path}", LogLevel.INFO);
                    return false;
                }

                Logger.WriteLog($"发现目录: {path}", LogLevel.INFO);

                // 获取目录信息
                var itemCount = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories).Length;
                Logger.WriteLog($"目录包含 {itemCount} 个项目", LogLevel.INFO);

                if (whatIf)
                {
                    Logger.WriteLog($"[模拟模式] 将删除: {path}", LogLevel.WARN);
                    return true;
                }

                // 用户确认
                if (!force)
                {
                    Logger.WriteColorOutput($"\n即将删除 {description} 目录:", ConsoleColor.Yellow);
                    Logger.WriteColorOutput($"路径: {path}", ConsoleColor.Yellow);
                    Logger.WriteColorOutput($"包含项目数: {itemCount}", ConsoleColor.Yellow);

                    Console.Write("\n确认删除吗? (y/N): ");
                    var confirmation = Console.ReadLine();
                    if (!string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(confirmation?.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.WriteLog("用户取消删除操作", LogLevel.WARN);
                        return false;
                    }
                }

                // 删除目录
                Logger.WriteLog($"正在删除目录: {path}", LogLevel.INFO);

                // 移除只读属性
                await RemoveReadOnlyAttributesAsync(path);

                Directory.Delete(path, true);
                Logger.WriteLog($"成功删除 {description} 目录", LogLevel.SUCCESS);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"删除 {description} 目录时发生错误: {ex.Message}", LogLevel.ERROR);
                return false;
            }
        }

        private static async Task RemoveReadOnlyAttributesAsync(string path)
        {
            await Task.Run(() =>
            {
                try
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var attributes = File.GetAttributes(file);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLog($"移除只读属性时发生错误: {ex.Message}", LogLevel.WARN);
                }
            });
        }

        public static async Task<bool> CleanConfigFileAsync(string filePath, string configType, bool whatIf, bool skipBackup)
        {
            if (!File.Exists(filePath))
            {
                Logger.WriteLog($"{configType} 配置文件不存在: {filePath}", LogLevel.INFO);
                return false;
            }

            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var originalContent = content;
                var modified = false;

                // 查找并移除Augment相关配置
                var augmentPatterns = new[]
                {
                    @"""augment[^""]*"":\s*[^,}]+,?",
                    @"""Augment[^""]*"":\s*[^,}]+,?",
                    @"""command"":\s*""augment[^""]*""[^}]*},?",
                    @"""augment[^""]*"""
                };

                foreach (var pattern in augmentPatterns)
                {
                    var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        Logger.WriteLog($"移除配置项: {match.Value}", LogLevel.INFO);
                        content = content.Replace(match.Value, "");
                        modified = true;
                    }
                }

                if (modified)
                {
                    // 清理多余的逗号和空行
                    content = Regex.Replace(content, @",\s*,", ",");
                    content = Regex.Replace(content, @",\s*}", "}");
                    content = Regex.Replace(content, @",\s*]", "]");
                    content = Regex.Replace(content, @"\n\s*\n+", "\n");

                    if (whatIf)
                    {
                        Logger.WriteLog($"[模拟模式] 将修改 {configType} 配置文件", LogLevel.WARN);
                        return true;
                    }

                    // 创建备份
                    if (!skipBackup)
                    {
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var backupPath = $"{filePath}.backup_{timestamp}";
                        await File.WriteAllTextAsync(backupPath, originalContent);
                        Logger.WriteLog($"已创建配置文件备份: {backupPath}", LogLevel.INFO);
                    }

                    // 写入修改后的内容
                    await File.WriteAllTextAsync(filePath, content);
                    Logger.WriteLog($"成功清理 {configType} 配置文件", LogLevel.SUCCESS);
                    return true;
                }
                else
                {
                    Logger.WriteLog($"{configType} 配置文件中未发现Augment相关条目", LogLevel.INFO);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"处理 {configType} 配置文件时发生错误: {ex.Message}", LogLevel.ERROR);
                return false;
            }
        }

        public static async Task<bool> RemoveFileAsync(string filePath, string description, bool whatIf)
        {
            try
            {
                if (whatIf)
                {
                    Logger.WriteLog($"[模拟模式] 将删除日志文件: {filePath}", LogLevel.WARN);
                }
                else
                {
                    await Task.Run(() => File.Delete(filePath));
                    Logger.WriteLog($"成功删除日志文件: {Path.GetFileName(filePath)}", LogLevel.SUCCESS);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"删除日志文件失败: {ex.Message}", LogLevel.ERROR);
                return false;
            }
        }
    }
}