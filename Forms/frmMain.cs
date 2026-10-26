using CebuanaReFind.Forms;
using CebuanaReFind.Helpers;
using System;
using System.Windows.Forms;

namespace CebuanaReFind
{
    public partial class frmMain : Form
    {
        private string connectionString;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //LoadConnection();

            //// Attach context menu
            //notifyIcon1.ContextMenuStrip = cmsRefind;
            //notifyIcon1.Visible = true;

            //// Hide form
            //this.ShowInTaskbar = false;
            //this.WindowState = FormWindowState.Minimized;
            //this.Hide();

            LoadConnection();

            notifyIcon1.ContextMenuStrip = cmsRefind;
            notifyIcon1.Visible = true;

            // Hide AFTER everything is ready
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.BeginInvoke(new Action(() => this.Hide()));
        }

        // =========================
        // LOAD CONNECTION
        // =========================
        private void LoadConnection()
        {
            connectionString = DatabaseHelper.LoadConnection();

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show(
                    "No database connection found. Please configure first.",
                    "Connection Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                OpenConfiguration();
            }
        }

        // =========================
        // OPEN CONFIGURATION
        // =========================
        private void OpenConfiguration()
        {
            using (frmConfiguration config = new frmConfiguration())
            {
                if (config.ShowDialog() == DialogResult.OK)
                {
                    // Reload connection after saving
                    connectionString = DatabaseHelper.LoadConnection();
                }
                else
                {
                    // If user cancels → exit app
                    notifyIcon1.Visible = false;
                    Application.Exit();
                }
            }
        }

        // =========================
        // OPEN ACCOUNT
        // =========================
        private void OpenAccount()
        {
            using (frmAccount config = new frmAccount())
            {
                if (config.ShowDialog() == DialogResult.OK)
                {
                    // Reload connection after saving
                    connectionString = DatabaseHelper.LoadConnection();
                }
                else
                {
                    // If user cancels → exit app
                    notifyIcon1.Visible = false;
                    Application.Exit();
                }
            }
        }

        // =========================
        // TRAY MENU - CONFIGURATION
        // =========================

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenConfiguration();
        }

        // =========================
        // TRAY MENU - EXIT
        // =========================
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        // =========================
        // DOUBLE CLICK TRAY
        // =========================
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //this.Show();
            //this.ShowInTaskbar = true;
            //this.WindowState = FormWindowState.Normal;
        }

        // =========================
        // PREVENT CLOSE (MINIMIZE TO TRAY)
        // =========================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        private void accountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenAccount();
        }


        private void OpenReport()
        {
            using (frmReport config = new frmReport())
            {
                if (config.ShowDialog() == DialogResult.OK)
                {
                    // Reload connection after saving
                    connectionString = DatabaseHelper.LoadConnection();
                }
                else
                {
                    // If user cancels → exit app
                    notifyIcon1.Visible = false;
                    Application.Exit();
                }
            }
        }

        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenReport();
        }
    }
}