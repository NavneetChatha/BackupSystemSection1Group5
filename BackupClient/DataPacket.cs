using System;

namespace BackupClient
{
    public enum PacketType
    {
        AUTH,
        BACKUP,
        RESTORE,
        RESPONSE,
        LOGOUT
    }

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

    public class PacketHeader
    {
        public PacketType PacketType { get; set; }
        public CommandType Command { get; set; }
        public int PayloadSize { get; set; }
        public string Timestamp { get; set; }

        public PacketHeader(PacketType type, CommandType command, int payloadSize)
        {
            PacketType = type;
            Command = command;
            PayloadSize = payloadSize;
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UserInfo(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public class FileMetadata
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }

        public FileMetadata(string fileName, string fileType, long fileSize)
        {
            FileName = fileName;
            FileType = fileType;
            FileSize = fileSize;
        }
    }

    public class DataPacket
    {
        public PacketHeader Header { get; set; }
        public UserInfo User { get; set; }
        public FileMetadata Metadata { get; set; }
        public byte[] FileData { get; set; } // dynamically allocated

        public DataPacket(PacketHeader header, UserInfo user, FileMetadata metadata, byte[] fileData)
        {
            Header = header;
            User = user;
            Metadata = metadata;
            FileData = fileData;
        }
    }
}