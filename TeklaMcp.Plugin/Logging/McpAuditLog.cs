using System;
using System.IO;
using System.Text.Json;

namespace TeklaMcp.Server.Logging
{
    public static class McpAuditLog
    {
        private static readonly string LogDir = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Develoh", "Tekla");
        private static readonly string LogFile = Path.Combine(LogDir, "mcp-audit.log");
        private static readonly object _lock = new object();

        /// <summary>Raised on the calling thread whenever a new entry is written.</summary>
        public static event Action<string>? EntryWritten;

        public static void Write(string toolName, object? args, bool success, string? guid, string? message)
        {
            var entry = new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                tool      = toolName,
                args,
                success,
                guid,
                message
            };

            var line = JsonSerializer.Serialize(entry);

            lock (_lock)
            {
                if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }

            EntryWritten?.Invoke(line);
        }
    }
}
