using System;
using System.IO;

namespace BackupClient
{
    /// <summary>
    /// Handles logging of all client side packet activity to a file.
    /// </summary>
    public class ClientLogger
    {
        private string logFilePath;

        /// <summary>
        /// Initializes a new instance of the ClientLogger class.
        /// </summary>
        /// <param name="logFilePath">The path to the log file.</param>
        public ClientLogger(string logFilePath = "client_log.txt")
        {
            this.logFilePath = logFilePath;
        }

        /// <summary>
        /// Logs a data packet with direction.
        /// </summary>
        /// <param name="packetType">The type of packet.</param>
        /// <param name="command">The command in the packet.</param>
        /// <param name="direction">The direction of the packet (TX or RX).</param>
        /// <param name="size">The size of the packet in bytes.</param>
        public void LogPacket(string packetType, string command, string direction, int size)
        {
            string message = $"[{GetTimestamp()}] [{direction}] " +
                           $"Type: {packetType} | " +
                           $"Command: {command} | " +
                           $"Size: {size} bytes";
            WriteToFile(message);
        }

        /// <summary>
        /// Logs a general message to the log file.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogMessage(string message)
        {
            WriteToFile($"[{GetTimestamp()}] {message}");
        }

        /// <summary>
        /// Logs an error message to the log file.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public void LogError(string message)
        {
            WriteToFile($"[{GetTimestamp()}] [ERROR] {message}");
        }

        /// <summary>
        /// Writes a message to the log file and console.
        /// </summary>
        /// <param name="message">The message to write.</param>
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

        /// <summary>
        /// Returns the current timestamp as a formatted string.
        /// </summary>
        private string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}