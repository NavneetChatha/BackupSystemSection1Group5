using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackupServer;

namespace BackupSystemTests
{
    [TestClass]
    public class SystemTests
    {
        private static TcpListener testListener;
        private static int testPort = 5001;

        [TestInitialize]
        public void Setup()
        {
            // Start a test server on port 5001 for each test
            testListener = new TcpListener(IPAddress.Any, testPort);
            testListener.Start();
        }

        [TestCleanup]
        public void Cleanup()
        {
            testListener?.Stop();
        }

        // ===== SYSTEM LEVEL TESTS =====

        [TestMethod]
        public void SystemTest_Authentication_ValidLogin_Succeeds()
        {
            Authentication auth = new Authentication();
            ServerStateMachine stateMachine = new ServerStateMachine();
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Verify server starts in IDLE
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());

            // Verify valid login succeeds
            bool result = auth.AuthenticateUser("admin", "admin123");
            Assert.IsTrue(result);

            // Verify state did not change after login (login is not a state transition)
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());

            logger.LogMessage("SystemTest_Authentication_ValidLogin_Succeeds PASSED");
        }

        [TestMethod]
        public void SystemTest_Authentication_InvalidLogin_Fails()
        {
            Authentication auth = new Authentication();
            ServerStateMachine stateMachine = new ServerStateMachine();
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Verify invalid login fails
            bool result = auth.AuthenticateUser("admin", "wrongpassword");
            Assert.IsFalse(result);

            // Verify state did not change
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());

            logger.LogMessage("SystemTest_Authentication_InvalidLogin_Fails PASSED");
        }

        [TestMethod]
        public void SystemTest_BackupFlow_StateMachineTransitions()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Verify initial state
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());

            // Simulate START_BACKUP command
            stateMachine.HandleCommand(CommandType.START_BACKUP);
            Assert.AreEqual(ServerState.RECEIVING_BACKUP, stateMachine.GetCurrentState());
            logger.LogMessage("State transitioned to RECEIVING_BACKUP");

            // Simulate STORE_COMPLETE command
            stateMachine.HandleCommand(CommandType.STORE_COMPLETE);
            Assert.AreEqual(ServerState.STORING_DATA, stateMachine.GetCurrentState());
            logger.LogMessage("State transitioned to STORING_DATA");

            // Reset to IDLE after backup
            stateMachine.ResetToIdle();
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
            logger.LogMessage("State reset to IDLE");

            logger.LogMessage("SystemTest_BackupFlow_StateMachineTransitions PASSED");
        }

        [TestMethod]
        public void SystemTest_RestoreFlow_StateMachineTransitions()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Verify initial state
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());

            // Simulate REQUEST_RESTORE command
            stateMachine.HandleCommand(CommandType.REQUEST_RESTORE);
            Assert.AreEqual(ServerState.SENDING_RESTORE, stateMachine.GetCurrentState());
            logger.LogMessage("State transitioned to SENDING_RESTORE");

            // Reset to IDLE after restore
            stateMachine.ResetToIdle();
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
            logger.LogMessage("State reset to IDLE");

            logger.LogMessage("SystemTest_RestoreFlow_StateMachineTransitions PASSED");
        }

        [TestMethod]
        public void SystemTest_FileTransfer_SaveAndRetrieve()
        {
            FileManager fileManager = new FileManager("SystemTestStorage");
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Create test file data (simulating a large file)
            byte[] testData = new byte[1024 * 1024]; // 1MB
            new Random().NextBytes(testData);

            // Save file
            bool saved = fileManager.SaveFile("system_test.jpg", testData);
            Assert.IsTrue(saved);
            logger.LogMessage("File saved to server storage");

            // Verify file exists
            bool exists = fileManager.FileExists("system_test.jpg");
            Assert.IsTrue(exists);
            logger.LogMessage("File exists in server storage");

            // Retrieve file
            byte[] retrieved = fileManager.GetFile("system_test.jpg");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(testData.Length, retrieved.Length);
            logger.LogMessage($"File retrieved successfully - Size: {retrieved.Length} bytes");

            logger.LogMessage("SystemTest_FileTransfer_SaveAndRetrieve PASSED");
        }

        [TestMethod]
        public void SystemTest_LargeFileTransfer_MinimumOneMB()
        {
            FileManager fileManager = new FileManager("SystemTestStorage");
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Create a file that is at least 1MB (REQ-SYS-070)
            byte[] largeFile = new byte[1024 * 1024 * 2]; // 2MB
            new Random().NextBytes(largeFile);

            bool saved = fileManager.SaveFile("large_test.jpg", largeFile);
            Assert.IsTrue(saved);

            byte[] retrieved = fileManager.GetFile("large_test.jpg");
            Assert.IsNotNull(retrieved);

            // Verify file is at least 1MB
            Assert.IsTrue(retrieved.Length >= 1024 * 1024,
                $"File should be at least 1MB but was {retrieved.Length} bytes");

            logger.LogMessage($"Large file transfer test passed - Size: {retrieved.Length} bytes");
            logger.LogMessage("SystemTest_LargeFileTransfer_MinimumOneMB PASSED");
        }

        [TestMethod]
        public void SystemTest_MaintenanceMode_BlocksCommands()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            ServerLogger logger = new ServerLogger("sys_test_log.txt");

            // Enter maintenance mode
            stateMachine.HandleCommand(CommandType.ENTER_MAINTENANCE);
            Assert.AreEqual(ServerState.MAINTENANCE, stateMachine.GetCurrentState());
            logger.LogMessage("Server entered maintenance mode");

            // Try to start backup while in maintenance (should not transition)
            stateMachine.HandleCommand(CommandType.START_BACKUP);
            Assert.AreEqual(ServerState.MAINTENANCE, stateMachine.GetCurrentState());
            logger.LogMessage("Backup command blocked in maintenance mode");

            // Exit maintenance
            stateMachine.HandleCommand(CommandType.EXIT_MAINTENANCE);
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
            logger.LogMessage("Server exited maintenance mode");

            logger.LogMessage("SystemTest_MaintenanceMode_BlocksCommands PASSED");
        }

        [TestMethod]
        public void SystemTest_Logging_PacketsAreLogged()
        {
            string logFile = "sys_packet_test_log.txt";
            ServerLogger logger = new ServerLogger(logFile);

            // Create and log a packet
            PacketHeader header = new PacketHeader(
                PacketType.BACKUP, CommandType.START_BACKUP, 100);
            UserInfo user = new UserInfo("admin", "admin123");
            FileMetadata metadata = new FileMetadata("test.jpg", "jpg", 100);
            byte[] fileData = new byte[100];
            DataPacket packet = new DataPacket(header, user, metadata, fileData);

            logger.LogPacket(packet, "TX");
            logger.LogPacket(packet, "RX");

            // Verify log file was created and has content
            Assert.IsTrue(File.Exists(logFile));
            string logContent = File.ReadAllText(logFile);
            Assert.IsTrue(logContent.Contains("TX"));
            Assert.IsTrue(logContent.Contains("RX"));

            logger.LogMessage("SystemTest_Logging_PacketsAreLogged PASSED");
        }

        [TestMethod]
        public void SystemTest_DataPacket_StructureIsCorrect()
        {
            // Verify packet structure meets REQ-SYS-020 and REQ-SYS-030
            PacketHeader header = new PacketHeader(
                PacketType.BACKUP, CommandType.START_BACKUP, 512);
            UserInfo user = new UserInfo("user1", "password1");
            FileMetadata metadata = new FileMetadata("backup.jpg", "jpg", 512);
            byte[] fileData = new byte[512]; // dynamically allocated buffer

            DataPacket packet = new DataPacket(header, user, metadata, fileData);

            // Verify all components exist
            Assert.IsNotNull(packet.Header);
            Assert.IsNotNull(packet.User);
            Assert.IsNotNull(packet.Metadata);
            Assert.IsNotNull(packet.FileData);

            // Verify dynamic allocation
            Assert.AreEqual(512, packet.FileData.Length);
            Assert.AreEqual(512, packet.Header.PayloadSize);
            Assert.IsFalse(string.IsNullOrEmpty(packet.Header.Timestamp));
        }
    }
}