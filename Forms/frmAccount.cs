using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CebuanaReFind.Helpers;

namespace CebuanaReFind.Forms
{
    public partial class frmAccount : Form
    {

        private int? SelectedAccountID = null;

        public frmAccount()
        {
            InitializeComponent();

            GradientStyle.ApplyGradient(
                panel1,
                Color.DarkBlue,
                Color.RoyalBlue);

            GradientStyle.ApplyGradient(
                panel4,
                Color.DimGray,
                Color.LightGray);
        }

        // =========================================
        // SEARCH
        // =========================================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                LoadData(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error during search.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // LOAD DATA
        // =========================================
        private void LoadData(string keyword = "")
        {
            try
            {
                dgvAccounts.DataSource = AccountHelper.GetAccounts(keyword);

                // AccountID is for internal use only
                if (dgvAccounts.Columns.Contains("AccountID"))
                {
                    dgvAccounts.Columns["AccountID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unexpected error while loading data.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // DELETE
        // =========================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvAccounts.CurrentRow == null)
                {
                    MessageBox.Show("Please select a record to delete.");
                    return;
                }

                if (dgvAccounts.CurrentRow.Cells["AccountID"].Value == null)
                {
                    MessageBox.Show("Invalid account selection.");
                    return;
                }

                int accountId = Convert.ToInt32(
                    dgvAccounts.CurrentRow.Cells["AccountID"].Value);

                string accountNo =
                    dgvAccounts.CurrentRow.Cells["AccountNo"].Value?.ToString();

                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete account '{accountNo}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    AccountHelper.Delete(accountId);

                    SelectedAccountID = null;

                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error deleting record.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // FORM LOAD
        // =========================================
        private void frmAccount_Load(object sender, EventArgs e)
        {
            try
            {
                string conn = DatabaseHelper.LoadConnection();

                if (string.IsNullOrEmpty(conn))
                {
                    MessageBox.Show(
                        "No database connection configured.");

                    this.Close();
                    return;
                }

                AccountHelper.EnsureAccountTable();
                AccountHelper.EnsureAccountTypeTable();

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error initializing form.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // NEW ACCOUNT
        // =========================================
        private void btnNew_Click(object sender, EventArgs e)
        {

            SelectedAccountID = null;

            frmConfiguration frm = new frmConfiguration();

            frm.Show();
            frm.OpenAccountTab();
        }

        // =========================================
        // CLOSE
        // =========================================
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // =========================================
        // CELL CLICK
        // =========================================
        private void dgvAccounts_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            try
            {
                if (dgvAccounts.Rows[e.RowIndex]
                    .Cells["AccountID"].Value != null)
                {
                    SelectedAccountID = Convert.ToInt32(
                        dgvAccounts.Rows[e.RowIndex]
                            .Cells["AccountID"].Value);
                }
            }
            catch
            {
                SelectedAccountID = null;
            }
        }

        // =========================================
        // DOUBLE CLICK
        // =========================================
        private void dgvAccounts_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0)
                    return;

                if (dgvAccounts.Rows[e.RowIndex]
                    .Cells["AccountID"].Value == null)
                    return;

                int accountId = Convert.ToInt32(
                    dgvAccounts.Rows[e.RowIndex]
                        .Cells["AccountID"].Value);

                SelectedAccountID = accountId;


                OpenConfigurationForEdit(accountId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error opening account.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // EDIT BUTTON
        // =========================================
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.CurrentRow == null)
            {
                MessageBox.Show(
                    "Please select a record first.");

                return;
            }

            if (dgvAccounts.CurrentRow.Cells["AccountID"].Value == null)
            {
                MessageBox.Show(
                    "Invalid account selection.");

                return;
            }

            int accountId = Convert.ToInt32(
                dgvAccounts.CurrentRow
                    .Cells["AccountID"].Value);

            SelectedAccountID = accountId;


            OpenConfigurationForEdit(accountId);
        }

        // =========================================
        // OPEN CONFIGURATION FOR EDIT
        // =========================================
        private void OpenConfigurationForEdit(int accountId)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is frmConfiguration configForm)
                {
                    configForm.Show();
                    configForm.BringToFront();

                    configForm.InitAccount(
                        "EDIT",
                        accountId);

                    return;
                }
            }

            frmConfiguration frm = new frmConfiguration();

            frm.Show();

            frm.InitAccount(
                "EDIT",
                accountId);
        }

        // =========================================
        // REFRESH BUTTON
        // =========================================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Clear();

                SelectedAccountID = null;

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error refreshing data.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================
        // REFRESH METHOD
        // =========================================
        public void RefreshData()
        {
            try
            {
                txtSearch.Clear();

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error refreshing data.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
