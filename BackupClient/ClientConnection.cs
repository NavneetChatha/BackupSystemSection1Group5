using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace BackupClient
{
    /// <summary>
    /// Handles the TCP connection between the client and the server.
    /// </summary>
    public class ClientConnection
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private ClientLogger logger;
        private bool isConnected = false;

        /// <summary>
        /// Initializes a new instance of the ClientConnection class.
        /// </summary>
        /// <param name="logger">The client logger instance.</param>
        public ClientConnection(ClientLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Establishes a TCP connection to the server.
        /// </summary>
        /// <param name="serverIP">The server IP address.</param>
        /// <param name="port">The server port number.</param>
        /// <returns>True if connected successfully, false otherwise.</returns>
        public bool Connect(string serverIP, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(serverIP, port);
                stream = tcpClient.GetStream();
                isConnected = true;
                logger.LogMessage($"Connected to server at {serverIP}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends a text message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if sent successfully, false otherwise.</returns>
        public bool SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                logger.LogPacket("MESSAGE", message, "TX", data.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Send failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends a file to the server as a byte array.
        /// </summary>
        /// <param name="fileName">The name of the file to send.</param>
        /// <param name="fileData">The file data as a byte array.</param>
        /// <returns>True if sent successfully, false otherwise.</returns>
        public bool SendFile(string fileName, byte[] fileData)
        {
            try
            {
                string header = $"UPLOAD_FILE|{fileName}";
                byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                stream.Write(headerBytes, 0, headerBytes.Length);
                stream.Write(fileData, 0, fileData.Length);
                logger.LogPacket("BACKUP", "UPLOAD_FILE", "TX", fileData.Length);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"File send failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Receives a response message from the server.
        /// </summary>
        /// <returns>The response string from the server.</returns>
        public string ReceiveResponse()
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                logger.LogPacket("RESPONSE", response, "RX", bytesRead);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Receive failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Receives a file from the server as a byte array.
        /// </summary>
        /// <returns>The file data as a byte array.</returns>
        public byte[] ReceiveFile()
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024 * 10]; // 10MB buffer
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                byte[] fileData = new byte[bytesRead];
                Array.Copy(buffer, fileData, bytesRead);
                logger.LogPacket("RESTORE", "RECEIVE_FILE", "RX", bytesRead);
                return fileData;
            }
            catch (Exception ex)
            {
                logger.LogError($"File receive failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sends login credentials to the server for authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if authentication was successful, false otherwise.</returns>
        public bool Login(string username, string password)
        {
            try
            {
                string loginMessage = $"LOGIN|{username}|{password}";
                SendMessage(loginMessage);
                string response = ReceiveResponse();
                logger.LogPacket("AUTH", "LOGIN", "TX", loginMessage.Length);
                return response == "AUTH_SUCCESS";
            }
            catch (Exception ex)
            {
                logger.LogError($"Login failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server cleanly.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (isConnected)
                {
                    SendMessage("LOGOUT");
                    stream?.Close();
                    tcpClient?.Close();
                    isConnected = false;
                    logger.LogMessage("Disconnected from server");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Disconnect error: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns whether the client is currently connected to the server.
        /// </summary>
        public bool IsConnected()
        {
            return isConnected;
        }
    }
}