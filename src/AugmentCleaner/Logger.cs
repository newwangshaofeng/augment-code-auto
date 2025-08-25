using System;

namespace AugmentCleaner
{
    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR,
        SUCCESS
    }

    public static class Logger
    {
        public static void WriteColorOutput(string message, ConsoleColor color = ConsoleColor.White)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteLog(string message, LogLevel level = LogLevel.INFO)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] [{level}] {message}";

            ConsoleColor color = level switch
            {
                LogLevel.INFO => ConsoleColor.Green,
                LogLevel.WARN => ConsoleColor.Yellow,
                LogLevel.ERROR => ConsoleColor.Red,
                LogLevel.SUCCESS => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };

            WriteColorOutput(logMessage, color);
        }

        public static void WriteSeparator(string title = "", ConsoleColor color = ConsoleColor.Cyan)
        {
            if (string.IsNullOrEmpty(title))
            {
                WriteColorOutput("========================================", color);
            }
            else
            {
                WriteColorOutput("========================================", color);
                WriteColorOutput($"    {title}", color);
                WriteColorOutput("========================================", color);
            }
        }

        public static void WritePhaseHeader(string phase, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.WriteLine();
            WriteColorOutput($"=== {phase} ===", color);
        }
    }
}