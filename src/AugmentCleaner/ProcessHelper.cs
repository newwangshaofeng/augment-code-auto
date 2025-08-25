using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

namespace AugmentCleaner
{
    public static class ProcessHelper
    {
        public static bool IsVSCodeRunning(bool force)
        {
            try
            {
                var vscodeProcesses = Process.GetProcessesByName("Code");
                if (vscodeProcesses.Length > 0)
                {
                    Logger.WriteLog("检测到VSCode正在运行，建议关闭VSCode后再执行清理操作", LogLevel.WARN);
                    if (!force)
                    {
                        Console.Write("是否继续执行清理? (y/N): ");
                        var confirmation = Console.ReadLine();
                        if (!string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(confirmation?.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.WriteLog("用户选择退出清理操作", LogLevel.WARN);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"检查VSCode进程时发生错误: {ex.Message}", LogLevel.WARN);
                return true; // 继续执行，不因为检查失败而中断
            }
        }

        public static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}