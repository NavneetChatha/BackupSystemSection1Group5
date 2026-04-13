﻿using System;
using System.IO;

namespace BackupServer
{
    /// <summary>
    /// Handles logging of all server-side packet activity to a file.
    /// </summary>
    public class ServerLogger
    {
        private string logFilePath;
        private readonly object fileLock = new object();

        /// <summary>
        /// Initializes a new instance of the ServerLogger class.
        /// </summary>
        /// <param name="logFilePath">The path to the log file.</param>
        public ServerLogger(string logFilePath = "server_log.txt")
        {
            this.logFilePath = logFilePath;
        }

        /// <summary>
        /// Logs a data packet with direction.
        /// </summary>
        /// <param name="packet">The packet to log.</param>
        /// <param name="direction">The direction of the packet (TX or RX).</param>
        public void LogPacket(DataPacket packet, string direction)
        {
            string message = $"[{GetTimestamp()}] [{direction}] " +
                           $"Type: {packet.Header.PacketType} | " +
                           $"Command: {packet.Header.Command} | " +
                           $"Size: {packet.Header.PayloadSize} bytes | " +
                           $"User: {packet.User?.Username}";
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
                lock (fileLock)
                {
                    File.AppendAllText(logFilePath, message + Environment.NewLine);
                }
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