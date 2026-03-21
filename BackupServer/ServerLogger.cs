using System;
using System.IO;

namespace BackupServer
{
    public class ServerLogger
    {
        private string logFilePath;

        public ServerLogger(string logFilePath = "server_log.txt")
        {
            this.logFilePath = logFilePath;
        }

        public void LogPacket(DataPacket packet, string direction)
        {
            string message = $"[{GetTimestamp()}] [{direction}] " +
                           $"Type: {packet.Header.PacketType} | " +
                           $"Command: {packet.Header.Command} | " +
                           $"Size: {packet.Header.PayloadSize} bytes | " +
                           $"User: {packet.User?.Username}";
            WriteToFile(message);
        }

        public void LogMessage(string message)
        {
            WriteToFile($"[{GetTimestamp()}] {message}");
        }

        public void LogError(string message)
        {
            WriteToFile($"[{GetTimestamp()}] [ERROR] {message}");
        }

        private void WriteToFile(string message)
        {
            try
            {
                Console.WriteLine(message);
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}