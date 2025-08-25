using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AugmentCleaner
{
    public class AugmentPluginCleaner
    {
        private readonly bool _force;
        private readonly bool _whatIf;
        private readonly bool _skipBackup;
        private readonly bool _cleanLogs;
        private readonly string _currentUser;
        private const string AugmentVersion = "0.521.0";

        public AugmentPluginCleaner(bool force, bool whatIf, bool skipBackup, bool cleanLogs)
        {
            _force = force;
            _whatIf = whatIf;
            _skipBackup = skipBackup;
            _cleanLogs = cleanLogs;
            _currentUser = Environment.UserName;
        }

        public async Task RunAsync()
        {
            try
            {
                // 显示标题
                Logger.WriteSeparator("Augment插件残留文件清理工具 v2.0");
                Console.WriteLine();

                // 检查VSCode是否正在运行
                if (!ProcessHelper.IsVSCodeRunning(_force))
                {
                    return;
                }

                // 检查管理员权限
                if (!ProcessHelper.IsRunningAsAdministrator())
                {
                    Logger.WriteLog("建议以管理员权限运行此程序以确保完全访问权限", LogLevel.WARN);
                }

                Logger.WriteLog($"当前用户: {_currentUser}", LogLevel.INFO);
                Logger.WriteLog($"Augment插件版本: {AugmentVersion}", LogLevel.INFO);

                if (_whatIf)
                {
                    Logger.WriteColorOutput("\n=== 模拟模式 - 仅显示将要删除的内容 ===", ConsoleColor.Yellow);
                }

                // 统计信息
                var stats = new CleanupStats();

                // 执行清理阶段
                await CleanExtensionDirectoriesAsync(stats);
                await CleanVSCodeDataDirectoriesAsync(stats);
                
                if (_cleanLogs)
                {
                    await CleanLogFilesAsync(stats);
                }
                else
                {
                    Logger.WritePhaseHeader("跳过日志文件清理", ConsoleColor.Yellow);
                    Logger.WriteLog("使用 --clean-logs 参数可清理日志文件", LogLevel.INFO);
                }

                await CleanConfigFilesAsync(stats);
                await CleanWorkspaceExtensionsAsync(stats);

                // 显示清理结果摘要
                DisplaySummary(stats);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"程序执行过程中发生未处理的错误: {ex.Message}", LogLevel.ERROR);
                Environment.Exit(1);
            }

            // 等待用户按键退出
            Console.WriteLine();
            Logger.WriteColorOutput("按任意键退出...", ConsoleColor.Gray);
            Console.ReadKey(true);
        }

        private async Task CleanExtensionDirectoriesAsync(CleanupStats stats)
        {
            Logger.WritePhaseHeader("第一阶段：清理扩展目录");

            var extensionPaths = new[]
            {
                $@"C:\Users\{_currentUser}\.vscode\extensions\augment.vscode-augment-{AugmentVersion}",
                $@"C:\Users\{_currentUser}\.trae\extensions\augment.vscode-augment-{AugmentVersion}"
            };

            var extensionNames = new[] { "VSCode扩展", "Trae扩展" };

            for (int i = 0; i < extensionPaths.Length; i++)
            {
                Console.WriteLine();
                Logger.WriteColorOutput($"检查 {extensionNames[i]} 目录...", ConsoleColor.White);

                if (Directory.Exists(extensionPaths[i]))
                {
                    stats.TotalFound++;
                    var result = await FileHelper.RemoveDirectorySafelyAsync(
                        extensionPaths[i], extensionNames[i], _whatIf, _force);
                    if (result)
                    {
                        stats.TotalDeleted++;
                    }
                    else
                    {
                        stats.Errors++;
                    }
                }
                else
                {
                    Logger.WriteLog($"{extensionNames[i]} 目录不存在，无需清理", LogLevel.INFO);
                }
            }
        }

        private async Task CleanVSCodeDataDirectoriesAsync(CleanupStats stats)
        {
            Logger.WritePhaseHeader("第二阶段：清理VSCode数据目录");

            var dataPaths = new[]
            {
                $@"C:\Users\{_currentUser}\AppData\Roaming\Code\User\globalStorage",
                $@"C:\Users\{_currentUser}\AppData\Roaming\Code\User\workspaceStorage",
                $@"C:\Users\{_currentUser}\AppData\Roaming\Code\CachedExtensions"
            };

            var dataPatterns = new[] { "augment.*", "*augment*", "augment.*" };
            var dataNames = new[] { "VSCode全局存储", "VSCode工作区存储", "VSCode缓存扩展" };

            for (int i = 0; i < dataPaths.Length; i++)
            {
                Console.WriteLine();
                Logger.WriteColorOutput($"检查 {dataNames[i]}...", ConsoleColor.White);

                if (Directory.Exists(dataPaths[i]))
                {
                    var matchingItems = Directory.GetDirectories(dataPaths[i], dataPatterns[i]);
                    foreach (var item in matchingItems)
                    {
                        stats.TotalFound++;
                        var result = await FileHelper.RemoveDirectorySafelyAsync(
                            item, $"{dataNames[i]} - {Path.GetFileName(item)}", _whatIf, _force);
                        if (result)
                        {
                            stats.TotalDeleted++;
                        }
                        else
                        {
                            stats.Errors++;
                        }
                    }

                    if (matchingItems.Length == 0)
                    {
                        Logger.WriteLog($"{dataNames[i]} 中未发现Augment相关项目", LogLevel.INFO);
                    }
                }
                else
                {
                    Logger.WriteLog($"{dataNames[i]} 路径不存在: {dataPaths[i]}", LogLevel.INFO);
                }
            }
        }

        private async Task CleanLogFilesAsync(CleanupStats stats)
        {
            Logger.WritePhaseHeader("第三阶段：清理日志文件");

            var logPath = $@"C:\Users\{_currentUser}\AppData\Roaming\Code\logs";
            Console.WriteLine();
            Logger.WriteColorOutput("检查VSCode日志文件...", ConsoleColor.White);

            if (Directory.Exists(logPath))
            {
                var logFiles = Directory.GetFiles(logPath, "*augment*", SearchOption.AllDirectories);
                foreach (var logFile in logFiles)
                {
                    stats.TotalFound++;
                    var result = await FileHelper.RemoveFileAsync(logFile, "日志文件", _whatIf);
                    if (result)
                    {
                        stats.TotalDeleted++;
                    }
                    else
                    {
                        stats.Errors++;
                    }
                }

                if (logFiles.Length == 0)
                {
                    Logger.WriteLog("VSCode日志文件中未发现Augment相关项目", LogLevel.INFO);
                }
            }
            else
            {
                Logger.WriteLog($"VSCode日志路径不存在: {logPath}", LogLevel.INFO);
            }
        }

        private async Task CleanConfigFilesAsync(CleanupStats stats)
        {
            Logger.WritePhaseHeader("第四阶段：清理配置文件");

            var configFiles = new[]
            {
                $@"C:\Users\{_currentUser}\AppData\Roaming\Code\User\settings.json",
                $@"C:\Users\{_currentUser}\AppData\Roaming\Code\User\keybindings.json"
            };

            var configTypes = new[] { "settings", "keybindings" };

            for (int i = 0; i < configFiles.Length; i++)
            {
                Console.WriteLine();
                Logger.WriteColorOutput($"检查 {configTypes[i]} 配置文件...", ConsoleColor.White);

                var result = await FileHelper.CleanConfigFileAsync(
                    configFiles[i], configTypes[i], _whatIf, _skipBackup);
                if (result)
                {
                    stats.ConfigModified++;
                }
            }
        }

        private async Task CleanWorkspaceExtensionsAsync(CleanupStats stats)
        {
            Logger.WritePhaseHeader("第五阶段：检查工作区扩展推荐");

            var workspaceFolders = Directory.GetDirectories(".", ".vscode", SearchOption.TopDirectoryOnly);
            foreach (var wsFolder in workspaceFolders)
            {
                var extensionsJsonPath = Path.Combine(wsFolder, "extensions.json");
                if (File.Exists(extensionsJsonPath))
                {
                    Logger.WriteColorOutput($"检查工作区扩展推荐: {extensionsJsonPath}", ConsoleColor.White);
                    var result = await FileHelper.CleanConfigFileAsync(
                        extensionsJsonPath, "extensions", _whatIf, _skipBackup);
                    if (result)
                    {
                        stats.ConfigModified++;
                    }
                }
            }
        }

        private void DisplaySummary(CleanupStats stats)
        {
            Console.WriteLine();
            Logger.WriteSeparator("清理结果摘要");

            Logger.WriteLog($"发现的项目数量: {stats.TotalFound}", LogLevel.INFO);
            Logger.WriteLog($"修改的配置文件数量: {stats.ConfigModified}", LogLevel.INFO);

            if (_whatIf)
            {
                Logger.WriteLog($"模拟删除的项目数量: {stats.TotalDeleted}", LogLevel.WARN);
            }
            else
            {
                Logger.WriteLog($"成功删除的项目数量: {stats.TotalDeleted}", LogLevel.SUCCESS);
            }

            if (stats.Errors > 0)
            {
                Logger.WriteLog($"处理失败的项目数量: {stats.Errors}", LogLevel.ERROR);
            }

            // 最终状态报告
            Console.WriteLine();
            if (_whatIf)
            {
                Logger.WriteColorOutput("模拟运行完成！", ConsoleColor.Yellow);
                Logger.WriteColorOutput("使用以下参数执行实际清理：", ConsoleColor.Yellow);
                Logger.WriteColorOutput("  --force          : 跳过确认提示", ConsoleColor.Gray);
                Logger.WriteColorOutput("  --clean-logs     : 同时清理日志文件", ConsoleColor.Gray);
                Logger.WriteColorOutput("  --skip-backup    : 跳过配置文件备份", ConsoleColor.Gray);
            }
            else if (stats.TotalDeleted > 0 || stats.ConfigModified > 0)
            {
                Logger.WriteColorOutput("清理完成！Augment插件残留文件已被成功清理。", ConsoleColor.Green);
                if (stats.ConfigModified > 0 && !_skipBackup)
                {
                    Logger.WriteColorOutput("配置文件已备份，如需恢复请查看 .backup_* 文件", ConsoleColor.Green);
                }
            }
            else
            {
                Logger.WriteColorOutput("未发现需要清理的Augment插件残留文件。", ConsoleColor.Green);
            }
        }
    }

    public class CleanupStats
    {
        public int TotalFound { get; set; }
        public int TotalDeleted { get; set; }
        public int ConfigModified { get; set; }
        public int Errors { get; set; }
    }
}