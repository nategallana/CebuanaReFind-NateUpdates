using System;
using System.Data;
using System.Data.SqlClient;
using CebuanaReFind.Helpers;

namespace CebuanaReFind.Data
{
    public class AccountRepository
    {
        private string GetConnection()
        {
            return DatabaseHelper.LoadConnection();
        }

        public void Insert(string accountType, string accountNo, string description)
        {
            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand("AccountMapping_Insert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AccountType", accountType);
                cmd.Parameters.AddWithValue("@AccountNo", accountNo);
                cmd.Parameters.AddWithValue("@Description", description);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(int id, string accountType, string accountNo, string description)
        {
            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand("AccountMapping_Update", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@AccountType", accountType);
                cmd.Parameters.AddWithValue("@AccountNo", accountNo);
                cmd.Parameters.AddWithValue("@Description", description);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetAll()
        {
            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand("AccountMapping_GetAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}