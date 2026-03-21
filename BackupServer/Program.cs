using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
                    // Read incoming message
                    byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] parts = message.Split('|');

                    if (parts.Length < 2) continue;

                    string command = parts[0];
                    string username = parts.Length > 1 ? parts[1] : "";
                    string password = parts.Length > 2 ? parts[2] : "";

                    Console.WriteLine($"Command received: {command}");

                    // Handle authentication first
                    if (command == "LOGIN")
                    {
                        if (auth.AuthenticateUser(username, password))
                        {
                            isAuthenticated = true;
                            connectedUsername = username;
                            SendResponse(stream, "AUTH_SUCCESS");
                            logger.LogMessage($"User {username} authenticated successfully");
                        }
                        else
                        {
                            SendResponse(stream, "AUTH_FAILED");
                            logger.LogError($"Authentication failed for user: {username}");
                        }
                        continue;
                    }

                    // Reject unauthenticated commands
                    if (!isAuthenticated)
                    {
                        SendResponse(stream, "NOT_AUTHENTICATED");
                        continue;
                    }

                    // Handle authenticated commands
                    switch (command)
                    {
                        case "START_BACKUP":
                            stateMachine.HandleCommand(CommandType.START_BACKUP);
                            SendResponse(stream, $"STATE:{stateMachine.GetCurrentState()}");
                            logger.LogMessage($"Backup started by {connectedUsername}");
                            break;

                        case "UPLOAD_FILE":
                            string fileName = parts.Length > 1 ? parts[1] : "backup.dat";
                            byte[] fileData = new byte[bytesRead];
                            Array.Copy(buffer, fileData, bytesRead);
                            fileManager.SaveFile(fileName, fileData);
                            stateMachine.HandleCommand(CommandType.STORE_COMPLETE);
                            SendResponse(stream, "FILE_SAVED");
                            logger.LogMessage($"File {fileName} saved for user {connectedUsername}");
                            break;

                        case "REQUEST_RESTORE":
                            stateMachine.HandleCommand(CommandType.REQUEST_RESTORE);
                            string restoreFile = parts.Length > 1 ? parts[1] : "restore.jpg";
                            byte[] restoreData = fileManager.GetFile(restoreFile);
                            if (restoreData != null)
                            {
                                stream.Write(restoreData, 0, restoreData.Length);
                                logger.LogMessage($"File {restoreFile} sent to {connectedUsername}");
                            }
                            else
                            {
                                SendResponse(stream, "FILE_NOT_FOUND");
                            }
                            stateMachine.ResetToIdle();
                            break;

                        case "ENTER_MAINTENANCE":
                            stateMachine.HandleCommand(CommandType.ENTER_MAINTENANCE);
                            SendResponse(stream, $"STATE:{stateMachine.GetCurrentState()}");
                            logger.LogMessage("Server entered maintenance mode");
                            break;

                        case "EXIT_MAINTENANCE":
                            stateMachine.HandleCommand(CommandType.EXIT_MAINTENANCE);
                            SendResponse(stream, $"STATE:{stateMachine.GetCurrentState()}");
                            logger.LogMessage("Server exited maintenance mode");
                            break;

                        case "LOGOUT":
                            SendResponse(stream, "LOGOUT_SUCCESS");
                            logger.LogMessage($"User {connectedUsername} logged out");
                            client.Close();
                            break;

                        default:
                            SendResponse(stream, "UNKNOWN_COMMAND");
                            break;
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
        /// Sends a response message to the client.
        /// </summary>
        /// <param name="stream">The network stream to send on.</param>
        /// <param name="message">The message to send.</param>
        static void SendResponse(NetworkStream stream, string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            stream.Write(response, 0, response.Length);
            Console.WriteLine($"Response sent: {message}");
        }
    }
}