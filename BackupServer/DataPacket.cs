using System;

namespace BackupServer
{
    /// <summary>
    /// Defines the type of packet being sent between client and server.
    /// </summary>
    public enum PacketType
    {
        AUTH,
        BACKUP,
        RESTORE,
        RESPONSE,
        LOGOUT
    }

    /// <summary>
    /// Defines the command type contained within a packet.
    /// </summary>
    public enum CommandType
    {
        LOGIN,
        START_BACKUP,
        STORE_COMPLETE,
        REQUEST_RESTORE,
        ENTER_MAINTENANCE,
        EXIT_MAINTENANCE,
        NONE
    }

    /// <summary>
    /// Represents the header of a data packet.
    /// </summary>
    public class PacketHeader
    {
        public PacketType PacketType { get; set; }
        public CommandType Command { get; set; }
        public int PayloadSize { get; set; }
        public string Timestamp { get; set; }

        /// <summary>
        /// Initializes a new instance of the PacketHeader class.
        /// </summary>
        /// <param name="type">The type of packet.</param>
        /// <param name="command">The command for this packet.</param>
        /// <param name="payloadSize">The size of the payload in bytes.</param>
        public PacketHeader(PacketType type, CommandType command, int payloadSize)
        {
            PacketType = type;
            Command = command;
            PayloadSize = payloadSize;
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// Represents user credentials used for authentication.
    /// </summary>
    public class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the UserInfo class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public UserInfo(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    /// <summary>
    /// Represents metadata about a file being transferred.
    /// </summary>
    public class FileMetadata
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the FileMetadata class.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileType">The file type or extension.</param>
        /// <param name="fileSize">The file size in bytes.</param>
        public FileMetadata(string fileName, string fileType, long fileSize)
        {
            FileName = fileName;
            FileType = fileType;
            FileSize = fileSize;
        }
    }

    /// <summary>
    /// Represents a structured data packet used for all client-server communication.
    /// Contains a header, user info, file metadata, and a dynamically allocated file buffer.
    /// </summary>
    public class DataPacket
    {
        public PacketHeader Header { get; set; }
        public UserInfo User { get; set; }
        public FileMetadata Metadata { get; set; }
        public byte[] FileData { get; set; }

        /// <summary>
        /// Initializes a new instance of the DataPacket class.
        /// </summary>
        /// <param name="header">The packet header.</param>
        /// <param name="user">The user information.</param>
        /// <param name="metadata">The file metadata.</param>
        /// <param name="fileData">The dynamically allocated file data buffer.</param>
        public DataPacket(PacketHeader header, UserInfo user, FileMetadata metadata, byte[] fileData)
        {
            Header = header;
            User = user;
            Metadata = metadata;
            FileData = fileData;
        }
    }
}