using System;
using System.IO;

namespace BackupServer
{
    /// <summary>
    /// Handles storing and retrieving files on the server.
    /// </summary>
    public class FileManager
    {
        private string storageDirectory;

        /// <summary>
        /// Initializes a new instance of the FileManager class.
        /// </summary>
        /// <param name="storageDirectory">The directory where files will be stored.</param>
        public FileManager(string storageDirectory = "ServerStorage")
        {
            this.storageDirectory = storageDirectory;
            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }
        }

        /// <summary>
        /// Saves a file to the server storage directory.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="fileData">The file data as a byte array.</param>
        /// <returns>True if saved successfully, false otherwise.</returns>
        public bool SaveFile(string fileName, byte[] fileData)
        {
            try
            {
                string filePath = Path.Combine(storageDirectory, fileName);
                File.WriteAllBytes(filePath, fileData);
                Console.WriteLine($"File saved: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a file from the server storage directory.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve.</param>
        /// <returns>The file data as a byte array, or null if not found.</returns>
        public byte[] GetFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(storageDirectory, fileName);
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"File retrieved: {fileName}");
                    return File.ReadAllBytes(filePath);
                }
                Console.WriteLine($"File not found: {fileName}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a file exists in the server storage directory.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public bool FileExists(string fileName)
        {
            return File.Exists(Path.Combine(storageDirectory, fileName));
        }

        /// <summary>
        /// Deletes a file from the server storage directory.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public bool DeleteFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(storageDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"File deleted: {fileName}");
                    return true;
                }
                Console.WriteLine($"File not found: {fileName}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete file: {ex.Message}");
                return false;
            }
        }
    }
}