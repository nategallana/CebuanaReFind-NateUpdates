using CebuanaReFind.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace CebuanaReFind.Forms
{
    public partial class frmConfiguration : Form
    {
        public string AccountMode { get; set; } // ADD / EDIT
        public string AccountType { get; set; }
        public string AccountNo { get; set; }
        public int? AccountID { get; set; }
        public string Description { get; set; }



        public frmConfiguration()
        {
            InitializeComponent();

            //Tab Configuration
            GradientStyle.ApplyGradient(
                btnSave,
                Color.DarkGreen,
                Color.ForestGreen,
                Color.White,
                Color.LightSeaGreen);

            GradientStyle.ApplyGradient(
                btnTest,
                Color.DarkBlue,
                Color.RoyalBlue,
                Color.White,
                Color.Transparent);

            GradientStyle.ApplyGradient(
                btnBrowse,
                Color.DarkBlue,
                Color.RoyalBlue,
                Color.White,
                Color.Transparent);

            GradientStyle.ApplyGradient(
                panel1,
                Color.DarkBlue,
                Color.RoyalBlue);

            GradientStyle.ApplyGradient(
                panel2,
                Color.DarkGray,
                Color.LightGray);




            //Tab Account
            GradientStyle.ApplyGradient(
                tabAccounts,
                Color.DarkGray,
                Color.LightGray);

            GradientStyle.ApplyGradient(
                btnAccSave,
                Color.DarkGreen,
                Color.ForestGreen,
                Color.White,
                Color.Transparent);

            GradientStyle.ApplyGradient(
                btnAccCancel,
                Color.Maroon,
                Color.Firebrick,
                Color.White,
                Color.Transparent);



        }
        private void frmConfiguration_Load(object sender, EventArgs e)
        {
            LoadSavedConnection();

            txtPassword.UseSystemPasswordChar = true;

            chkWindowsAuth_CheckedChanged(null, null);

            LoadAccountTypes();

        }



        private void ClearFields()
        {
            txtAccountNo.Clear();
            txtAccType.Clear();
            txtDescription.Clear();
            cmbAccType.SelectedIndex = -1;

            txtAccountNo.ReadOnly = false;
        }


        public void OpenAccountTab()
        {
            tabConfiguration.SelectedTab = tabAccounts;

            //tabAccounts.Controls.Clear();

            //frmAccount frm = new frmAccount();
            //frm.TopLevel = false;
            //frm.FormBorderStyle = FormBorderStyle.None;
            //frm.Dock = DockStyle.Fill;

            //tabAccounts.Controls.Add(frm);
            //frm.Show();
        }

        //public void EditAccount(string accNo, string accType, string description)
        //{
        //    // Open the tab
        //    tabConfiguration.SelectedTab = tabAccounts;

        //    // Set values
        //    txtAccountNo.Text = accNo;
        //    txtAccType.Text = accType;
        //    txtDescription.Text = description;

        //    cmbAccType.SelectedValue = accType.Trim();

        //    txtAccountNo.ReadOnly = true; // prevent changing PK
        //    isEditMode = true;
        //}


        

        // =========================================
        // BUILD CONNECTION STRING
        // =========================================
        private string BuildConnectionString()
        {
            if (chkWindowsAuth.Checked)
            {
                return $"Server={txtServer.Text};Database={txtDatabase.Text};Trusted_Connection=True;";
            }
            else
            {
                return $"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUsername.Text};Password={txtPassword.Text};";
            }
        }



        // =========================================
        // VALIDATE INPUT FIELDS
        // =========================================
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Server is required.");
                txtServer.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Database is required.");
                txtDatabase.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Location is required.");
                return false;
            }

            if (!chkWindowsAuth.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Username is required.");
                    txtUsername.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Password is required.");
                    txtPassword.Focus();
                    return false;
                }
            }

            return true;
        }





        // =========================================
        // LOAD SAVED CONNECTION
        // =========================================
        private void LoadSavedConnection()
        {
            string conn = DatabaseHelper.LoadConnection();
            string savedPath = DatabaseHelper.GetSavedPath();

            txtLocation.Text = savedPath;

            if (!string.IsNullOrEmpty(conn))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conn);

                txtServer.Text = builder.DataSource;
                txtDatabase.Text = builder.InitialCatalog;

                if (builder.IntegratedSecurity)
                {
                    chkWindowsAuth.Checked = true;
                }
                else
                {
                    chkWindowsAuth.Checked = false;
                    txtUsername.Text = builder.UserID;
                    txtPassword.Text = builder.Password;
                }
            }
        }
        


        public void InitAccount(string mode, int accountId = 0)
        {
            AccountMode = mode;

            tabConfiguration.SelectedTab = tabAccounts;

            if (mode == "ADD")
            {
                AccountID = null;

                ClearAccountFields();
            }
            else if (mode == "EDIT")
            {
                AccountID = accountId;
                AccountMode = "EDIT";

                LoadAccount(accountId);

                txtAccountNo.Enabled = false;
            }
        }

        private void ClearAccountFields()
        {
            txtAccountNo.Clear();
            txtAccType.Clear();
            txtDescription.Clear();
            cmbAccType.SelectedIndex = -1;

            txtAccountNo.Enabled = true;
        }

        private void LoadAccount(int accountId)
        {
            try
            {
                DataTable dt = AccountHelper.GetAccountById(accountId);

                if (dt.Rows.Count > 0)
                {
                    txtAccountNo.Text =
                        dt.Rows[0]["AccountNo"].ToString();

                    txtDescription.Text =
                        dt.Rows[0]["Description"].ToString();

                    txtAccType.Text =
                        dt.Rows[0]["AccountType"].ToString();

                    cmbAccType.SelectedValue =
                        dt.Rows[0]["AccountType"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading account.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void LoadAccountTypes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.LoadConnection()))
                {
                    conn.Open();

                    string query = "SELECT AccountType FROM AccountType";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbAccType.DataSource = dt;
                    cmbAccType.DisplayMember = "AccountType";
                    cmbAccType.ValueMember = "AccountType";

                    cmbAccType.SelectedIndex = -1; // default blank



                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account types: " + ex.Message);
            }
        }


        // =========================================
        // TEST CONNECTION
        // =========================================
        private void btnTest_Click(object sender, EventArgs e)
        {

            if (!ValidateFields())
                return;

            string connectionString = BuildConnectionString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }

                MessageBox.Show("Connection Successful!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed!\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        // =========================================
        // SAVE CONNECTION
        // =========================================
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
                return;

            string connectionString = BuildConnectionString();

            try
            {
                // Test connection first
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }

                // Use DatabaseHelper (IMPORTANT CHANGE)
                DatabaseHelper.SaveConnection(txtLocation.Text, connectionString);

                MessageBox.Show("Connection Saved Successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save connection.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        // =========================================
        // BROWSE SAVE LOCATION
        // =========================================
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder where CebuanaConnection.txt will be saved";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtLocation.Text = dialog.SelectedPath;
            }
        }




        // =========================================
        // SHOW PASSWORD
        // =========================================
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }




        // =========================================
        // CLOSE BUTTON
        // =========================================
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }




        // =========================================
        // EXIT APPLICATION
        // =========================================
        //public void CloseProgram()
        //{
        //    DialogResult result = MessageBox.Show(
        //        "Are you sure you want to exit?",
        //        "Exit Application",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);

        //    if (result == DialogResult.Yes)
        //    {
        //        Application.Exit();
        //    }
        //}




        private void frmConfiguration_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If saved via button, allow closing
            if (this.DialogResult == DialogResult.OK)
                return;

            // Check if a valid connection already exists
            string existingConn = DatabaseHelper.LoadConnection();

            // If connection exists → just close (no prompt needed)
            if (!string.IsNullOrEmpty(existingConn))
                return;

            // No connection → warn user
            DialogResult result = MessageBox.Show(
                "No connection is configured. Do you want to exit the application?",
                "Exit Application",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }



        // =========================================
        // WINDOWS AUTH TOGGLE
        // =========================================
        private void chkWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            txtUsername.Enabled = !chkWindowsAuth.Checked;
            txtPassword.Enabled = !chkWindowsAuth.Checked;
        }

        private void btnAccSave_Click(object sender, EventArgs e)
        {
            string accNo = txtAccountNo.Text.Trim();
            string description = txtDescription.Text.Trim();
            string accType = cmbAccType.SelectedValue?.ToString();

            // =========================
            // VALIDATION
            // =========================
            if (string.IsNullOrEmpty(accNo) ||
                cmbAccType.SelectedIndex == -1 ||
                string.IsNullOrEmpty(description))
            {
                MessageBox.Show(
                    "Please fill in all fields.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                using (SqlConnection conn =
                    new SqlConnection(DatabaseHelper.LoadConnection()))
                {
                    conn.Open();

                    SqlCommand cmd;

                    if (AccountMode == "EDIT" && AccountID.HasValue)
                    {
                        // =========================
                        // UPDATE
                        // =========================
                        string updateQuery = @"
                            UPDATE Account
                            SET AccountNo = @AccountNo,
                                AccountType = @AccountType,
                                Description = @Description
                            WHERE AccountID = @AccountID";

                        cmd = new SqlCommand(updateQuery, conn);

                        cmd.Parameters.Add("@AccountID", SqlDbType.Int)
                                      .Value = AccountID.Value;
                    }
                    else
                    {
                        // =========================
                        // INSERT
                        // =========================
                        string insertQuery = @"
                            INSERT INTO Account
                                (AccountNo, AccountType, Description)
                            VALUES
                                (@AccountNo, @AccountType, @Description)";

                        cmd = new SqlCommand(insertQuery, conn);
                    }

                    // =========================
                    // PARAMETERS
                    // =========================
                    cmd.Parameters.Add("@AccountNo", SqlDbType.VarChar, 50)
                                  .Value = accNo;

                    cmd.Parameters.Add("@AccountType", SqlDbType.VarChar, 100)
                                  .Value = accType;

                    cmd.Parameters.Add("@Description", SqlDbType.VarChar, 255)
                                  .Value = description;

                    cmd.ExecuteNonQuery();

                    MessageBox.Show(
                        "Record saved successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // =========================
                    // RESET
                    // =========================
                    ClearAccountFields();

                    AccountID = null;
                    AccountNo = null;


                    // =========================
                    // REFRESH ACCOUNT FORM
                    // =========================
                    frmAccount accForm = null;

                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is frmAccount existing)
                        {
                            accForm = existing;
                            break;
                        }
                    }

                    if (accForm == null)
                    {
                        accForm = new frmAccount();
                        accForm.Show();
                    }

                    accForm.RefreshData();
                    accForm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error saving record: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnAccTypeAdd_Click(object sender, EventArgs e)
        {
            string input = cmbAccType.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            input = input.ToUpper();

            if (!AccountHelper.AccountTypeExists(input))
            {
                DialogResult result = MessageBox.Show(
                    "Account Type not found. Do you want to add it?",
                    "Add New Account Type",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    AccountHelper.AddAccountType(input);

                    LoadAccountTypes();

                    // ✅ CLEAR AFTER ADD
                    cmbAccType.SelectedIndex = -1;
                    cmbAccType.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Account Type already exists.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Optional: select existing item properly
                cmbAccType.Text = input;
            }


        }

        private void btnAccCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAccTypeDel_Click(object sender, EventArgs e)
        {

        }
    }
}