using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace AugmentCleaner
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Augment插件残留文件清理工具 v2.0")
            {
                Description = "彻底清理Augment插件在VSCode和Trae中的残留文件，包括配置文件和数据"
            };

            // 定义命令行选项
            var forceOption = new Option<bool>(
                aliases: new[] { "--force", "-f" },
                description: "强制删除，不提示确认");

            var whatIfOption = new Option<bool>(
                aliases: new[] { "--whatif", "-w" },
                description: "仅显示将要删除的内容，不实际删除");

            var skipBackupOption = new Option<bool>(
                aliases: new[] { "--skip-backup", "-s" },
                description: "跳过配置文件备份");

            var cleanLogsOption = new Option<bool>(
                aliases: new[] { "--clean-logs", "-l" },
                description: "清理日志文件（默认跳过）");

            // 添加选项到根命令
            rootCommand.AddOption(forceOption);
            rootCommand.AddOption(whatIfOption);
            rootCommand.AddOption(skipBackupOption);
            rootCommand.AddOption(cleanLogsOption);

            // 设置命令处理器
            rootCommand.SetHandler(async (force, whatIf, skipBackup, cleanLogs) =>
            {
                var cleaner = new AugmentPluginCleaner(force, whatIf, skipBackup, cleanLogs);
                await cleaner.RunAsync();
            }, forceOption, whatIfOption, skipBackupOption, cleanLogsOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}