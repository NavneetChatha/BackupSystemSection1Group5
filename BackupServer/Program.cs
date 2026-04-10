using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace BackupServer
{
    /// <summary>
    /// Main entry point for the Backup Server application.
    /// Handles incoming client connections and coordinates all server components.
    /// </summary>
    class Program
    {
        private static ServerStateMachine stateMachine = new ServerStateMachine();
        private static Authentication auth = new Authentication();
        private static FileManager fileManager = new FileManager();
        private static ServerLogger logger = new ServerLogger();
        private static TcpListener listener;
        private static bool isRunning = true;

        /// <summary>
        /// Main method that starts the server and listens for client connections.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("=== Backup Server Starting ===");
            Console.WriteLine($"Current State: {stateMachine.GetCurrentState()}");

            try
            {
                listener = new TcpListener(IPAddress.Any, 5000);
                listener.Start();
                Console.WriteLine("Server listening on port 5000...");
                logger.LogMessage("Server started on port 5000");

                while (isRunning)
                {
                    Console.WriteLine("Waiting for client connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected!");
                    logger.LogMessage("Client connected");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Server error: {ex.Message}");
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles communication with a connected client.
        /// </summary>
        /// <param name="client">The connected TcpClient.</param>
        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            bool isAuthenticated = false;
            string connectedUsername = "";

            try
            {
                while (client.Connected)
                {
                    // Read command header first
                    string command = ReadMessage(stream);
                    if (command == null) break;

                    Console.WriteLine($"Command received: {command}");

                    // Handle authentication
                    if (command.StartsWith("LOGIN"))
                    {
                        string[] parts = command.Split('|');
                        string username = parts.Length > 1 ? parts[1] : "";
                        string password = parts.Length > 2 ? parts[2] : "";

                        if (auth.AuthenticateUser(username, password))
                        {
                            isAuthenticated = true;
                            connectedUsername = username;
                            SendMessage(stream, "AUTH_SUCCESS");
                            logger.LogMessage($"User {username} authenticated successfully");
                        }
                        else
                        {
                            SendMessage(stream, "AUTH_FAILED");
                            logger.LogError($"Authentication failed for user: {username}");
                        }
                        continue;
                    }

                    // Reject unauthenticated commands
                    if (!isAuthenticated)
                    {
                        SendMessage(stream, "NOT_AUTHENTICATED");
                        continue;
                    }

                    // Handle authenticated commands
                    if (command.StartsWith("START_BACKUP"))
                    {
                        stateMachine.HandleCommand(CommandType.START_BACKUP);
                        SendMessage(stream, $"STATE:{stateMachine.GetCurrentState()}");
                        logger.LogMessage($"Backup started by {connectedUsername}");

                        // Now receive the file
                        string fileHeader = ReadMessage(stream);
                        if (fileHeader != null && fileHeader.StartsWith("FILE|"))
                        {
                            string[] headerParts = fileHeader.Split('|');
                            string fileName = headerParts[1];
                            int fileSize = int.Parse(headerParts[2]);

                            // Read file data
                            byte[] fileData = ReadBytes(stream, fileSize);
                            if (fileData != null)
                            {
                                fileManager.SaveFile(fileName, fileData);
                                stateMachine.HandleCommand(CommandType.STORE_COMPLETE);
                                stateMachine.ResetToIdle();
                                SendMessage(stream, "FILE_SAVED");
                                logger.LogMessage($"File {fileName} saved for {connectedUsername}");
                            }
                            else
                            {
                                SendMessage(stream, "FILE_ERROR");
                                stateMachine.ResetToIdle();
                            }
                        }
                    }
                    else if (command.StartsWith("REQUEST_RESTORE"))
                    {
                        string[] parts = command.Split('|');
                        string fileName = parts.Length > 1 ? parts[1] : "restore.jpg";

                        stateMachine.HandleCommand(CommandType.REQUEST_RESTORE);

                        byte[] fileData = fileManager.GetFile(fileName);
                        if (fileData != null)
                        {
                            // Send file size first then file data
                            SendMessage(stream, $"FILE|{fileName}|{fileData.Length}");
                            Thread.Sleep(100);
                            stream.Write(fileData, 0, fileData.Length);
                            logger.LogMessage($"File {fileName} sent to {connectedUsername}");
                        }
                        else
                        {
                            SendMessage(stream, "FILE_NOT_FOUND");
                        }
                        stateMachine.ResetToIdle();
                    }
                    else if (command.StartsWith("ENTER_MAINTENANCE"))
                    {
                        stateMachine.HandleCommand(CommandType.ENTER_MAINTENANCE);
                        SendMessage(stream, $"STATE:{stateMachine.GetCurrentState()}");
                        logger.LogMessage("Server entered maintenance mode");
                    }
                    else if (command.StartsWith("EXIT_MAINTENANCE"))
                    {
                        stateMachine.HandleCommand(CommandType.EXIT_MAINTENANCE);
                        SendMessage(stream, $"STATE:{stateMachine.GetCurrentState()}");
                        logger.LogMessage("Server exited maintenance mode");
                    }
                    else if (command.StartsWith("LOGOUT"))
                    {
                        SendMessage(stream, "LOGOUT_SUCCESS");
                        logger.LogMessage($"User {connectedUsername} logged out");
                        break;
                    }
                    else
                    {
                        SendMessage(stream, "UNKNOWN_COMMAND");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Client handling error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected");
                logger.LogMessage("Client disconnected");
            }
        }

        /// <summary>
        /// Reads a text message from the network stream.
        /// </summary>
        /// <param name="stream">The network stream to read from.</param>
        /// <returns>The message string or null if connection closed.</returns>
        static string ReadMessage(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) return null;
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a specific number of bytes from the network stream.
        /// </summary>
        /// <param name="stream">The network stream to read from.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>The byte array or null if failed.</returns>
        static byte[] ReadBytes(NetworkStream stream, int size)
        {
            try
            {
                byte[] data = new byte[size];
                int totalRead = 0;
                while (totalRead < size)
                {
                    int bytesRead = stream.Read(data, totalRead, size - totalRead);
                    if (bytesRead == 0) break;
                    totalRead += bytesRead;
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a text message to the client.
        /// </summary>
        /// <param name="stream">The network stream to send on.</param>
        /// <param name="message">The message to send.</param>
        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            stream.Write(response, 0, response.Length);
            Console.WriteLine($"Response sent: {message}");
        }
    }
}