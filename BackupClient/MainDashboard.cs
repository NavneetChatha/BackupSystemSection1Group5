using System;
using System.IO;
using System.Windows.Forms;

namespace BackupClient
{
    /// <summary>
    /// Main dashboard form for the Backup System client application.
    /// Allows users to backup files, restore files, and view logs.
    /// </summary>
    public partial class MainDashboard : Form
    {
        private ClientConnection connection;
        private ClientLogger logger;

        /// <summary>
        /// Initializes the MainDashboard with an active connection and logger.
        /// </summary>
        /// <param name="connection">The active client connection.</param>
        /// <param name="logger">The client logger instance.</param>
        public MainDashboard(ClientConnection connection, ClientLogger logger)
        {
            InitializeComponent();
            this.connection = connection;
            this.logger = logger;
        }

        /// <summary>
        /// Handles the Backup File button click.
        /// Opens a file picker and sends the selected file to the server.
        /// </summary>
        private void btnBackup_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a file to backup";
            fileDialog.Filter = "All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = fileDialog.FileName;
                string fileName = Path.GetFileName(filePath);
                byte[] fileData = File.ReadAllBytes(filePath);

                progressBar.Visible = true;
                progressBar.Value = 50;

                connection.SendMessage("START_BACKUP");
                string stateResponse = connection.ReceiveResponse();
                lblState.Text = $"Server State: {stateResponse.Replace("STATE:", "")}";

                bool sent = connection.SendFile(fileName, fileData);

                if (sent)
                {
                    string response = connection.ReceiveResponse();
                    progressBar.Value = 100;
                    lblState.Text = "Server State: IDLE";
                    MessageBox.Show($"File '{fileName}' backed up successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    logger.LogMessage($"File {fileName} backed up successfully");
                }
                else
                {
                    MessageBox.Show("Backup failed.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logger.LogError("Backup failed");
                }

                progressBar.Visible = false;
                progressBar.Value = 0;
            }
        }

        /// <summary>
        /// Handles the Restore File button click.
        /// Requests a file restore from the server and saves it locally.
        /// </summary>
        private void btnRestore_Click(object sender, EventArgs e)
        {
            string fileName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the filename to restore:", "Restore File", "restore.jpg");

            if (string.IsNullOrEmpty(fileName)) return;

            progressBar.Visible = true;
            progressBar.Value = 50;

            connection.SendMessage($"REQUEST_RESTORE|{fileName}");
            byte[] fileData = connection.ReceiveFile();

            if (fileData != null && fileData.Length > 0)
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.FileName = fileName;
                saveDialog.Filter = "All Files (*.*)|*.*";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(saveDialog.FileName, fileData);
                    progressBar.Value = 100;
                    MessageBox.Show($"File restored successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    logger.LogMessage($"File {fileName} restored successfully");
                }
            }
            else
            {
                MessageBox.Show("File not found on server.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.LogError($"Restore failed for file: {fileName}");
            }

            progressBar.Visible = false;
            progressBar.Value = 0;
        }

        /// <summary>
        /// Handles the View Logs button click.
        /// Opens the client log file in Notepad.
        /// </summary>
        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            string logFile = "client_log.txt";
            if (File.Exists(logFile))
            {
                System.Diagnostics.Process.Start("notepad.exe", logFile);
            }
            else
            {
                MessageBox.Show("No log file found yet.", "Logs",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Handles the Logout button click.
        /// Disconnects from the server and returns to the login screen.
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            connection.Disconnect();
            logger.LogMessage("User logged out");
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }
    }
}