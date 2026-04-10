using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackupClient
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a username and password.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ClientLogger logger = new ClientLogger();
            ClientConnection connection = new ClientConnection(logger);

            bool connected = connection.Connect("127.0.0.1", 5000);

            if (!connected)
            {
                MessageBox.Show("Could not connect to server. Make sure the server is running.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool loggedIn = connection.Login(username, password);

            if (loggedIn)
            {
                logger.LogMessage($"User {username} logged in successfully");
                MainDashboard dashboard = new MainDashboard(connection, logger);
                dashboard.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                connection.Disconnect();
            }
        }
    }
}