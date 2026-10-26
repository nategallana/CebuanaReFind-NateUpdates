using CebuanaReFind.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CebuanaReFind
{
    public partial class frmConnection : Form
    {
        public frmConnection()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //string connectionString;

            //if (chkWindowsAuth.Checked)
            //{
            //    connectionString =
            //        //$"Server={txtServer.Text};Database={txtDatabase.Text};Trusted_Connection=True;";
            //}
            //else
            //{
            //    connectionString =
            //        //$"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUsername.Text};Password={txtPassword.Text};";
            //}

            //try
            //{
            //    using (SqlConnection conn = new SqlConnection(connectionString))
            //    {
            //        conn.Open();
            //    }

            //    DatabaseHelper.SaveConnection(connectionString);

            //    MessageBox.Show("Connection Saved Successfully!");

            //    this.DialogResult = DialogResult.OK;
            //    this.Close();
            //}
            //catch
            //{
            //    MessageBox.Show("Cannot save. Connection is invalid.",
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //string connectionString;


            //if (chkWindowsAuth.Checked)
            //{
            //    //connectionString =
            //        //$"Server={txtServer.Text};Database={txtDatabase.Text};Trusted_Connection=True;";
            //}
            //else
            //{
            //    //connectionString =
            //        //$"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUsername.Text};Password={txtPassword.Text};";
                
            //}


            //if (string.IsNullOrWhiteSpace(txtServer.Text) ||
            //    string.IsNullOrWhiteSpace(txtDatabase.Text))
            //{
            //    MessageBox.Show("Server and Database are required.");
            //    return;
            //}

            //try
            //{
            //    using (SqlConnection conn = new SqlConnection(connectionString))
            //    {
            //        conn.Open();
            //    }

            //    MessageBox.Show("Connection Successful!", "Success",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Connection Failed!\n\n" + ex.Message,
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void chkWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            //txtUsername.Enabled = !chkWindowsAuth.Checked;
            //txtPassword.Enabled = !chkWindowsAuth.Checked;
        }

        private void frmConnection_Load(object sender, EventArgs e)
        {

            //string conn = DatabaseHelper.LoadConnection();

            //if (!string.IsNullOrEmpty(conn))
            //{
            //    var builder = new SqlConnectionStringBuilder(conn);

            //    txtServer.Text = builder.DataSource;
            //    txtDatabase.Text = builder.InitialCatalog;

            //    if (builder.IntegratedSecurity)
            //    {
            //        chkWindowsAuth.Checked = true;
            //    }
            //    else
            //    {
            //        chkWindowsAuth.Checked = false;
            //        txtUsername.Text = builder.UserID;
            //        txtPassword.Text = builder.Password;
            //    }
            //}
        }

    }
}