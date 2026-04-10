using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackupServer;

namespace BackupSystemTests
{
    [TestClass]
    public class UnitTest1
    {
        // ===== AUTHENTICATION TESTS =====

        [TestMethod]
        public void Test_Authentication_ValidUser_ReturnsTrue()
        {
            Authentication auth = new Authentication();
            bool result = auth.AuthenticateUser("admin", "admin123");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Authentication_InvalidPassword_ReturnsFalse()
        {
            Authentication auth = new Authentication();
            bool result = auth.AuthenticateUser("admin", "wrongpassword");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_Authentication_InvalidUser_ReturnsFalse()
        {
            Authentication auth = new Authentication();
            bool result = auth.AuthenticateUser("unknownuser", "admin123");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_Authentication_EmptyCredentials_ReturnsFalse()
        {
            Authentication auth = new Authentication();
            bool result = auth.AuthenticateUser("", "");
            Assert.IsFalse(result);
        }

        // ===== STATE MACHINE TESTS =====

        [TestMethod]
        public void Test_StateMachine_InitialState_IsIdle()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_StartBackup_TransitionsToReceivingBackup()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.START_BACKUP);
            Assert.AreEqual(ServerState.RECEIVING_BACKUP, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_StoreComplete_TransitionsToStoringData()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.START_BACKUP);
            stateMachine.HandleCommand(CommandType.STORE_COMPLETE);
            Assert.AreEqual(ServerState.STORING_DATA, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_RequestRestore_TransitionsToSendingRestore()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.REQUEST_RESTORE);
            Assert.AreEqual(ServerState.SENDING_RESTORE, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_EnterMaintenance_TransitionsToMaintenance()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.ENTER_MAINTENANCE);
            Assert.AreEqual(ServerState.MAINTENANCE, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_ExitMaintenance_TransitionsToIdle()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.ENTER_MAINTENANCE);
            stateMachine.HandleCommand(CommandType.EXIT_MAINTENANCE);
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
        }

        [TestMethod]
        public void Test_StateMachine_ResetToIdle_ResetsState()
        {
            ServerStateMachine stateMachine = new ServerStateMachine();
            stateMachine.HandleCommand(CommandType.START_BACKUP);
            stateMachine.ResetToIdle();
            Assert.AreEqual(ServerState.IDLE, stateMachine.GetCurrentState());
        }

        // ===== FILE MANAGER TESTS =====

        [TestMethod]
        public void Test_FileManager_SaveAndRetrieveFile_ReturnsCorrectData()
        {
            FileManager fileManager = new FileManager("TestStorage");
            byte[] testData = new byte[] { 1, 2, 3, 4, 5 };
            fileManager.SaveFile("test.bin", testData);
            byte[] retrieved = fileManager.GetFile("test.bin");
            CollectionAssert.AreEqual(testData, retrieved);
        }

        [TestMethod]
        public void Test_FileManager_FileExists_ReturnsTrue()
        {
            FileManager fileManager = new FileManager("TestStorage");
            byte[] testData = new byte[] { 1, 2, 3 };
            fileManager.SaveFile("existstest.bin", testData);
            bool exists = fileManager.FileExists("existstest.bin");
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void Test_FileManager_FileNotExists_ReturnsFalse()
        {
            FileManager fileManager = new FileManager("TestStorage");
            bool exists = fileManager.FileExists("doesnotexist.bin");
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void Test_FileManager_DeleteFile_RemovesFile()
        {
            FileManager fileManager = new FileManager("TestStorage");
            byte[] testData = new byte[] { 1, 2, 3 };
            fileManager.SaveFile("deletetest.bin", testData);
            fileManager.DeleteFile("deletetest.bin");
            bool exists = fileManager.FileExists("deletetest.bin");
            Assert.IsFalse(exists);
        }

        // ===== PACKET TESTS =====

        [TestMethod]
        public void Test_DataPacket_CreatedCorrectly()
        {
            PacketHeader header = new PacketHeader(PacketType.BACKUP, CommandType.START_BACKUP, 100);
            UserInfo user = new UserInfo("admin", "admin123");
            FileMetadata metadata = new FileMetadata("test.jpg", "jpg", 100);
            byte[] fileData = new byte[100];
            DataPacket packet = new DataPacket(header, user, metadata, fileData);

            Assert.AreEqual(PacketType.BACKUP, packet.Header.PacketType);
            Assert.AreEqual(CommandType.START_BACKUP, packet.Header.Command);
            Assert.AreEqual("admin", packet.User.Username);
            Assert.AreEqual("test.jpg", packet.Metadata.FileName);
            Assert.AreEqual(100, packet.FileData.Length);
        }

        [TestMethod]
        public void Test_PacketHeader_TimestampNotEmpty()
        {
            PacketHeader header = new PacketHeader(PacketType.AUTH, CommandType.LOGIN, 0);
            Assert.IsFalse(string.IsNullOrEmpty(header.Timestamp));
        }

        // ===== CLIENT LOGGER TESTS =====

        [TestMethod]
        public void Test_ClientLogger_LogMessage_WritesToFile()
        {
            BackupClient.ClientLogger logger = new BackupClient.ClientLogger("test_client_log.txt");
            logger.LogMessage("Test message");
            bool exists = System.IO.File.Exists("test_client_log.txt");
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void Test_ClientLogger_LogError_WritesToFile()
        {
            BackupClient.ClientLogger logger = new BackupClient.ClientLogger("test_client_error_log.txt");
            logger.LogError("Test error");
            bool exists = System.IO.File.Exists("test_client_error_log.txt");
            Assert.IsTrue(exists);
        }
    }
}