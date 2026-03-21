using System;
using System.Collections.Generic;

namespace BackupServer
{
    /// <summary>
    /// Handles user authentication for the server.
    /// Uses hardcoded credentials for mock login purposes.
    /// </summary>
    public class Authentication
    {
        private Dictionary<string, string> validUsers = new Dictionary<string, string>()
        {
            { "admin", "admin123" },
            { "user1", "password1" },
            { "user2", "password2" }
        };

        /// <summary>
        /// Authenticates a user by checking credentials against stored users.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to authenticate.</param>
        /// <returns>True if authentication is successful, false otherwise.</returns>
        public bool AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Authentication failed: empty username or password.");
                return false;
            }

            if (validUsers.ContainsKey(username) && validUsers[username] == password)
            {
                Console.WriteLine($"Authentication successful for user: {username}");
                return true;
            }

            Console.WriteLine($"Authentication failed for user: {username}");
            return false;
        }

        /// <summary>
        /// Adds a new user to the valid users list.
        /// </summary>
        /// <param name="username">The username to add.</param>
        /// <param name="password">The password for the new user.</param>
        public void AddUser(string username, string password)
        {
            if (!validUsers.ContainsKey(username))
            {
                validUsers[username] = password;
                Console.WriteLine($"User {username} added successfully.");
            }
            else
            {
                Console.WriteLine($"User {username} already exists.");
            }
        }

        /// <summary>
        /// Removes a user from the valid users list.
        /// </summary>
        /// <param name="username">The username to remove.</param>
        public void RemoveUser(string username)
        {
            if (validUsers.ContainsKey(username))
            {
                validUsers.Remove(username);
                Console.WriteLine($"User {username} removed successfully.");
            }
            else
            {
                Console.WriteLine($"User {username} not found.");
            }
        }
    }
}