using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CebuanaReFind.Helpers;

namespace CebuanaReFind.Forms
{
    public partial class frmReport : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);

        public frmReport()
        {
            InitializeComponent();
        }

        private void frmReport_Load(object sender, EventArgs e)
        {
            cmbReportType.Items.Clear();
            cmbReportType.Items.Add("Sales Transaction Report");
            cmbReportType.Items.Add("Sold Items Report");

            cmbReportType.SelectedIndex = 0;

            dtpFrom.Value = DateTime.Today;
            dtpTo.Value = DateTime.Today;

            LoadStores();

            SendMessage(txtSearch.Handle, 0x1501, 0, "Search report (press Enter)...");

            this.WindowState = FormWindowState.Maximized;
        }

        private void LoadStores()
        {
            try
            {
                DataTable dt = AccountHelper.GetStores();

                DataRow allRow = dt.NewRow();
                allRow["Location"] = "";
                allRow["Description"] = "All Stores";

                dt.Rows.InsertAt(allRow, 0);

                cmbStore.DataSource = dt;
                cmbStore.DisplayMember = "Description";
                cmbStore.ValueMember = "Location";

                cmbStore.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stores: " + ex.Message);
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            if (cmbReportType.SelectedIndex == -1)
                return;

            string reportType = cmbReportType.SelectedItem.ToString().Trim();

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;

            // Get selected Location
            string store = cmbStore.SelectedValue?.ToString() ?? "";

            if (from > to)
            {
                MessageBox.Show("Invalid date range.");
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                DataTable dt = AccountHelper.GetReport(
                    reportType,
                    from,
                    to,
                    store,
                    txtSearch.Text.Trim(),
                    applyDate: true
                );

                dgvAccounts.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchReport();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SearchReport();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvAccounts.DataSource is DataTable dt)
            {
                string keyword = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrEmpty(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    List<string> filterExpressions = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.DataType == typeof(string))
                        {
                            filterExpressions.Add($"[{col.ColumnName}] LIKE '%{keyword}%'");
                        }
                    }

                    if (filterExpressions.Count > 0)
                    {
                        dt.DefaultView.RowFilter = string.Join(" OR ", filterExpressions);
                    }
                }
            }
        }

        private void SearchReport()
        {
            string keyword = txtSearch.Text.Trim();

            // If keyword is empty, view by date range
            if (string.IsNullOrWhiteSpace(keyword))
            {
                btnView_Click(this, EventArgs.Empty);
                return;
            }

            if (cmbReportType.SelectedIndex == -1)
                return;

            string reportType = cmbReportType.SelectedItem.ToString().Trim();
            string store = cmbStore.SelectedValue?.ToString() ?? "";

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Search across database even if date is not configured
                DataTable dt = AccountHelper.GetReport(
                    reportType,
                    null,
                    null,
                    store,
                    keyword,
                    applyDate: false
                );

                dgvAccounts.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show(
                        $"No records found for \"{keyword}\".",
                        "Search Report",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.DataSource == null)
                return;

            DataTable dtSource = dgvAccounts.DataSource as DataTable;
            if (dtSource == null)
                return;

            DataTable dt = dtSource.DefaultView.ToTable();

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;

            string reportName = cmbReportType.SelectedItem.ToString();
            string searchKw = txtSearch.Text.Trim();

            string fileName;

            if (!string.IsNullOrEmpty(searchKw))
            {
                fileName = $"{reportName}_Search_{searchKw}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            }
            else
            {
                switch (reportName)
                {
                    case "Sales Transaction Report":
                        fileName = $"Sales Transaction Report_{from:yyyyMMdd}_to_{to:yyyyMMdd}.csv";
                        break;

                    case "Sold Items Report":
                        fileName = $"Sold Items Report_{from:yyyyMMdd}_to_{to:yyyyMMdd}.csv";
                        break;

                    default:
                        fileName = $"Report_{from:yyyyMMdd}_to_{to:yyyyMMdd}.csv";
                        break;
                }
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files|*.csv";
                sfd.FileName = fileName;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // Headers
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sw.Write(dt.Columns[i].ColumnName);

                            if (i < dt.Columns.Count - 1)
                                sw.Write(",");
                        }

                        sw.WriteLine();

                        // Rows
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string value = row[i]?.ToString()?.Replace("\"", "\"\"");

                                // Handle commas and quotes properly
                                if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                                {
                                    value = $"\"{value}\"";
                                }

                                sw.Write(value);

                                if (i < dt.Columns.Count - 1)
                                    sw.Write(",");
                            }

                            sw.WriteLine();
                        }
                    }

                    MessageBox.Show("CSV exported successfully.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
