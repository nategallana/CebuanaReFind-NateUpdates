using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace CebuanaReFind.Helpers
{
    public static class AccountHelper
    {
        private static string GetConnection()
        {
            return DatabaseHelper.LoadConnection();
        }

        public static DataTable GetStores()
        {

            string query = @"
                SELECT
                    Location,
                    Description
                FROM Location
                WHERE IsActive = 'T'
                ORDER BY Location";

            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }



        // =========================
        // GET DATA (WITH SEARCH)
        // =========================
        public static DataTable GetAccounts(string keyword = "")
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(GetConnection()))
            {
                string query = @"
            SELECT
                AccountID,
                AccountType,
                AccountNo,
                Description
            FROM Account
            WHERE
                @kw = ''
                OR AccountType LIKE '%' + @kw + '%'
                OR AccountNo LIKE '%' + @kw + '%'
                OR Description LIKE '%' + @kw + '%'
            ORDER BY AccountID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@kw",
                        SqlDbType.NVarChar,
                        255).Value = keyword ?? "";

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }

        // =========================
        // INSERT
        // =========================
        public static void Insert(string accNo, string accType, string desc)
        {
            using (SqlConnection con = new SqlConnection(GetConnection()))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(1) FROM Account WHERE AccountNo = @AccountNo";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@AccountNo", SqlDbType.VarChar).Value = accNo;

                    int exists = (int)checkCmd.ExecuteScalar();
                    if (exists > 0)
                        throw new Exception("Account already exists.");
                }

                string query = @"INSERT INTO Account (AccountNo, AccountType, Description)
                         VALUES (@AccountNo, @AccountType, @Description)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@AccountNo", SqlDbType.VarChar).Value = accNo;
                    cmd.Parameters.Add("@AccountType", SqlDbType.VarChar).Value = accType;
                    cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = desc;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =========================
        // UPDATE
        // =========================
        public static void Update(string accNo, string accType, string desc)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetConnection()))
                {
                    con.Open();

                    string query = @"UPDATE Account 
                                 SET AccountType = @AccountType,
                                     Description = @Description
                                 WHERE AccountNo = @AccountNo";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@AccountNo", accNo);
                    cmd.Parameters.AddWithValue("@AccountType", accType);
                    cmd.Parameters.AddWithValue("@Description", desc);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Account successfully updated.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error updating account.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================
        // DELETE
        // =========================
        public static void Delete(int accountId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetConnection()))
                {
                    con.Open();

                    string query = @"
                DELETE FROM Account
                WHERE AccountID = @AccountID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@AccountID",
                            SqlDbType.Int).Value = accountId;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Account successfully deleted.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error deleting account.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        public static DataTable GetAccountById(int accountId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnection()))
                {
                    string query = @"
                SELECT 
                    AccountId,
                    AccountNo,
                    AccountType,
                    Description
                FROM Account
                WHERE AccountId = @AccountId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add(
                            "@AccountId",
                            SqlDbType.Int
                        ).Value = accountId;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error retrieving account by ID.",
                    ex);
            }

            return dt;
        }




        public static void EnsureAccountTable()
        {
            string query = @"
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM sys.objects 
                        WHERE object_id = OBJECT_ID(N'[dbo].[Account]') 
                        AND type = 'U'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[Account](
                            [AccountNo] NVARCHAR(50) NOT NULL PRIMARY KEY,
                            [AccountType] NVARCHAR(50) NULL,
                            [Description] NVARCHAR(255) NULL,
                            [CreatedDate] DATETIME DEFAULT GETDATE()
                        )
                    END";

            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static void EnsureAccountTypeTable()
        {
            string query = @"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.objects 
                    WHERE object_id = OBJECT_ID(N'[dbo].[AccountType]') 
                    AND type = 'U'
                )
                BEGIN
                    CREATE TABLE [dbo].[AccountType](
                        [AccountType] NVARCHAR(50) NOT NULL PRIMARY KEY
                    )
                END";

            using (SqlConnection conn = new SqlConnection(GetConnection()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static bool AccountTypeExists(string name)
        {
            using (SqlConnection conn = new SqlConnection(GetConnection()))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM AccountType WHERE AccountType = @Name";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static void AddAccountType(string name)
        {
            using (SqlConnection conn = new SqlConnection(GetConnection()))
            {
                conn.Open();

                string query = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM AccountType WHERE AccountType = @Name
                    )
                    BEGIN
                        INSERT INTO AccountType (AccountType)
                        VALUES (@Name)
                    END";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = name;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static DataTable GetReport(string reportType, DateTime dateFrom, DateTime dateTo, string store)
        {
            return GetReport(reportType, dateFrom, dateTo, store, "", true);
        }

        public static DataTable GetReport(string reportType, DateTime? dateFrom, DateTime? dateTo, string store, string keyword = "", bool applyDate = true)
        {
            DataTable dt = new DataTable();

            // Validate input early
            if (string.IsNullOrWhiteSpace(reportType))
                throw new Exception("Invalid report type: empty value.");

            // Normalize to avoid mismatch issues
            string normalizedType = reportType.Trim().ToLower();

            string query = "";

            switch (normalizedType)
            {
                case "sales transaction report":
                    query = GetSalesReportQuery();
                    break;

                case "sold items report":
                    query = GetSoldItemsReportQuery();
                    break;

                default:
                    throw new Exception($"Invalid report type: [{reportType}]");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(GetConnection()))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 120;

                    DateTime fromVal = dateFrom.HasValue ? dateFrom.Value.Date : DateTime.Today;
                    DateTime toVal = dateTo.HasValue ? dateTo.Value.Date : DateTime.Today;

                    cmd.Parameters.Add("@DateFrom", SqlDbType.DateTime).Value = fromVal;
                    cmd.Parameters.Add("@DateTo", SqlDbType.DateTime).Value = toVal;
                    cmd.Parameters.AddWithValue("@Store", store ?? "");
                    cmd.Parameters.Add("@ApplyDate", SqlDbType.Bit).Value = applyDate ? 1 : 0;
                    cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 255).Value = keyword ?? "";

                    DateTime? parsedSearchDate = null;
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string kwTrimmed = keyword.Trim();
                        DateTime tempDt;
                        if (DateTime.TryParseExact(kwTrimmed, new[] { "MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy", "M-d-yyyy", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "yyyy/MM/dd" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tempDt)
                            || DateTime.TryParse(kwTrimmed, out tempDt))
                        {
                            parsedSearchDate = tempDt.Date;
                        }
                    }
                    cmd.Parameters.Add("@SearchDate", SqlDbType.Date).Value = parsedSearchDate.HasValue ? (object)parsedSearchDate.Value : DBNull.Value;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating report.\n\n" + ex.Message, ex);
            }

            return dt;
        }


        public static string GetSoldItemsReportQuery()
        {
            return @"
            SELECT 
		CASE 
			WHEN Pos.DocNo LIKE '%-SR%' THEN
				LEFT(Pos.DocNo, CHARINDEX('-', Pos.DocNo))
				+ 'RT'
				+ REPLACE(
					SUBSTRING(
						Pos.DocNo, 
						CHARINDEX('-', Pos.DocNo) + 1, 
						LEN(Pos.DocNo)
					),
					'SR',
					''
				)

			WHEN Pos.DocNo LIKE '%-SO%' THEN
				LEFT(Pos.DocNo, CHARINDEX('-', Pos.DocNo))
				+ 'SO'
				+ REPLACE(
					SUBSTRING(
						Pos.DocNo, 
						CHARINDEX('-', Pos.DocNo) + 1, 
						LEN(Pos.DocNo)
					),
					'SO',
					''
				)

			ELSE
				LEFT(Pos.DocNo, CHARINDEX('-', Pos.DocNo))
                
				+ SUBSTRING(
					Pos.DocNo, 
					CHARINDEX('-', Pos.DocNo) + 1, 
					LEN(Pos.DocNo)
				)
		END AS [Header Text],

        Pos.DocNo AS [Receipt No.],
        FORMAT(MAX(Pos.CreatedTime), 'MM/dd/yyyy') AS PostingDate, 
        FORMAT(MAX(Pos.TaxInvoiceDate), 'MM/dd/yyyy') AS DocumentDate, 
        'AUTOCOUNT POS' AS [Header Text],
        '3' AS [Version for Print],
        PosDtl.ItemCode AS [Material],
        '' AS [Material Description],
        'JJ03' AS [Plant],
        '' AS [Storage Location],
        '' AS [Batch Number],
        CASE
			WHEN Pos.DocNo LIKE '%-SR%' THEN 'Z56'
			WHEN Pos.DocNo LIKE '%-SI%' THEN 'Z55'
		END AS [Movement Type],
        '' AS [Stock Type],
        '' AS [Special Stock Indicator],
        SUM(PosDtl.Qty) AS [Qty in Unit of Entry],
        MAX(PosDtl.UOM) AS [Unit of Entry],
        MAX(Pos.DocNo) AS [Item Text],
        MAX(PosTerminal.Outlet) AS [Profit Center],
        '' AS [GL Account],
        MAX(Debtor.AccNo) AS [Goods Recipient],

        -- Excel-SAFE TIMESTAMP WITH LEADING '
        '''' + REPLACE(
                REPLACE(
REPLACE(CONVERT(VARCHAR(19), MAX(Pos.CreatedTime), 120), '-', ''),
                ':', ''),
            ' ', '') AS [timestamp]

    FROM Pos
        INNER JOIN PosDtl ON Pos.DocKey = PosDtl.DocKey
        LEFT JOIN Debtor ON Pos.DebtorCode = Debtor.AccNo
        LEFT JOIN PosTerminal ON Pos.TerminalID = PosTerminal.TerminalID

    -- ? DATE FILTER HERE
    WHERE Pos.CreatedTime >= @DateFrom
      AND Pos.CreatedTime < DATEADD(DAY, 1, @DateTo)
      AND (
			Pos.SalesOrder = 'F'
			OR (
				Pos.SalesOrder = 'T'
				AND NULLIF(LTRIM(RTRIM(Pos.DocNo2)), '') IS NOT NULL
			)
		)
      AND PosDtl.ItemCode <> ''
    GROUP BY Pos.DocNo, PosDtl.ItemCode
    ORDER BY Pos.DocNo ASC;
                ";
        }



        private static string GetSalesReportQuery()
        {
            return @"
            WITH InvoiceData AS (
                SELECT 
                    Pos.DocNo AS SINo,
                    MAX(Pos.DocNo2) AS DocNo2,
                    MAX(Debtor.AccNo) AS DebtorAccNo,
                    MAX(Pos.CreatedTime) AS [Posting Date], 
                    MAX(Pos.TaxInvoiceDate) AS [Document Date], 
                    MAX(Debtor.CompanyName) AS CompanyName,
		            MAX(PosDtl.ReturnFromDocNo) AS ReturnFromDocNo,

                    SUM(CAST(PosDtl.UnitPrice / 1.12 AS DECIMAL(18,3))) AS Sales,

                    SUM(CAST(OrigDtl.UnitPrice / 1.12 AS DECIMAL(18,3))) AS ReturnSales,

                    SUM(
                        CAST(
                            CASE 
                                -- RETURN TRANSACTION  
                                WHEN PosDtl.ReturnFromDocNo IS NOT NULL 
                                    THEN
                                        CASE
                                            -- Only use computed discount if value contains %
                                            WHEN ISNULL(OrigDtl.Discount, '') LIKE '%[%]%'
                                                THEN 
                                                    FLOOR(
                                                        (
                                                            (OrigDtl.UnitPrice / 1.12) *
                                                            (
                                                                CAST(
                                                                    REPLACE(OrigDtl.Discount, '%', '') 
                                                                    AS DECIMAL(18,3)
                                                                ) / 100.0
                                                            )
                                                        ) * 1000
                                                    ) / 1000

                                            -- Otherwise use TotalDiscount
                                            ELSE ISNULL(OrigDtl.TotalDiscount, 0)
                                        END

                                -- NORMAL SALES
                                ELSE
                                    CASE
                                        -- Only use computed discount if value contains %
                                        WHEN ISNULL(PosDtl.Discount, '') LIKE '%[%]%'
                                            THEN 
                                                FLOOR(
                                                    (
                                                        (PosDtl.UnitPrice / 1.12) *
                                                        (
                                                            CAST(
                                                                REPLACE(PosDtl.Discount, '%', '') 
                                                                AS DECIMAL(18,3)
                                                            ) / 100.0
                                                        )
                                                    ) * 1000
                                                ) / 1000

                                        -- Otherwise use TotalDiscount
                                        ELSE ABS(ISNULL(PosDtl.TotalDiscount, 0))
                                    END
                            END
                            AS DECIMAL(18,3))
                    ) AS SalesDiscount,

                    MAX(
                        CASE 	
                            WHEN PosDtl.ItemCode = '00001' 
                            THEN 1
                            ELSE 0
                        END 
                    ) AS IsExtension,

                    MAX(
                        CASE 	
                            WHEN PosDtl.ItemCode = '00002' 
                            THEN 1
                            ELSE 0
                        END 
                    ) AS IsReservation,

                    MAX(
                        CASE 	
                            WHEN PosDtl.ItemCode = '00003' 
                            THEN 1
                            ELSE 0
                        END 
                    ) AS IsForfeited,

                    MAX(
                        CASE 	
                            WHEN PosDtl.ItemCode = '00005' 
                            THEN 1
                            ELSE 0
                        END 
                    ) AS IsPrepayment,

                    MAX(
                        CASE
                            WHEN PosDtl.ReturnFromDocNo IS NOT NULL
                                THEN 1
                            ELSE 0
                        END
                    ) AS IsReturn,

		            MAX(PosDtl.Remarks) AS Remarks,

                    -- =========================================================
                    -- DP DISCOUNT CHECK
                    -- Normal transaction  : PosDtl
                    -- Return transaction  : OrigDtl
                    -- =========================================================
                    MAX(
                        CASE
                            WHEN PosDtl.ReturnFromDocNo IS NOT NULL
                                 AND OrigDtl.PH_BIRDiscountType = 'DP'
                                THEN 1

                            WHEN PosDtl.ReturnFromDocNo IS NULL
                                 AND PosDtl.PH_BIRDiscountType = 'DP'
                                THEN 1

                            ELSE 0
                        END
                    ) AS IsDPDiscount,

                    SUM(CAST(PosDtl.Tax AS DECIMAL(18,3))) AS OutputTax,

                    STRING_AGG(PosDtl.ItemCode, CHAR(13) + CHAR(10)) AS SKU,

                    MAX(
                        CASE
                            WHEN ISNULL(Pos.PH_ZeroRatedAmt, 0) > 0
                                THEN 'OX' 	
                            WHEN Pos.TaxCode <> 'SR' 
                                THEN 'OX'
                            ELSE 'O1'
                        END 
                    ) AS TaxCode,

                    MAX(PosTerminal.StockLocation) AS StockLocation,

                    MAX('JJ207' + (PosTerminal.StockLocation)) AS ProfitCenter,

                    STRING_AGG(Item.UDF_Karat, CHAR(13) + CHAR(10)) AS [Karat],

                    STRING_AGG(Item.UDF_Weight, CHAR(13) + CHAR(10)) AS [Grams],

                    STRING_AGG(Item.ItemGroup, CHAR(13) + CHAR(10)) AS [Product Tag],

                    MAX(PosTerminal.Description) AS [Plant],

                    STRING_AGG(Item.UDF_Color, CHAR(13) + CHAR(10)) AS [Jewelry Color Tag],

                    STRING_AGG(Item.ItemType, CHAR(13) + CHAR(10)) AS [Jewelry Type]

                FROM Pos

                INNER JOIN PosDtl 
                    ON Pos.DocKey = PosDtl.DocKey

                LEFT JOIN Pos OrigPos 
                    ON PosDtl.ReturnFromDocNo = OrigPos.DocNo

                LEFT JOIN PosDtl OrigDtl 
                    ON OrigPos.DocKey = OrigDtl.DocKey 
                    AND OrigDtl.ItemCode = PosDtl.ItemCode

                LEFT JOIN Debtor 
                    ON Pos.DebtorCode = Debtor.AccNo

                LEFT JOIN PosTerminal 
                    ON Pos.TerminalID = PosTerminal.TerminalID

                LEFT JOIN Location 
                    ON PosTerminal.Outlet = Location.Location

                LEFT JOIN Item 
                    ON PosDtl.ItemCode = Item.ItemCode

                WHERE 
                    (@ApplyDate = 0 OR (Pos.CreatedTime >= @DateFrom AND Pos.CreatedTime < DATEADD(DAY,1,@DateTo)))
                    AND (
                        @Store IS NULL 
                        OR @Store = ''
                        OR Location.Location = @Store
                    )
                    AND (
                        @Keyword IS NULL
                        OR @Keyword = ''
                        OR (@SearchDate IS NOT NULL AND (
                            CAST(Pos.CreatedTime AS DATE) = @SearchDate 
                            OR CAST(Pos.TaxInvoiceDate AS DATE) = @SearchDate
                        ))
                        OR Pos.DocNo LIKE '%' + @Keyword + '%'
                        OR Pos.DocNo2 LIKE '%' + @Keyword + '%'
                        OR PosDtl.ItemCode LIKE '%' + @Keyword + '%'
                        OR PosTerminal.StockLocation LIKE '%' + @Keyword + '%'
                        OR PosTerminal.Outlet LIKE '%' + @Keyword + '%'
                        OR Debtor.AccNo LIKE '%' + @Keyword + '%'
                        OR Debtor.CompanyName LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.CreatedTime, 101) LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.TaxInvoiceDate, 101) LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.CreatedTime, 103) LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.TaxInvoiceDate, 103) LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.CreatedTime, 120) LIKE '%' + @Keyword + '%'
                        OR CONVERT(VARCHAR(10), Pos.TaxInvoiceDate, 120) LIKE '%' + @Keyword + '%'
                    )

                GROUP BY Pos.DocNo
            ),

            PaymentData AS (
                SELECT 
                    Pos.DocNo AS SINo,
                    Pos.DocNo2,
                    Pos.Total2,
                    PosPayment.PaymentType,
                    PosPayment.DocNo AS PaymentDocNo,
                    PosPayment.CCType,
                    PosPayment.DCType,
                    PosPayment.CustomPaymentType,
                    CAST(PosPayment.PaymentAmt AS DECIMAL(18,2)) AS PaymentAmt,
                    Pos.CreatedTime,
                    Pos.TaxInvoiceDate,
                    Debtor.CompanyName,
                    PosTerminal.Outlet AS ProfitCenter,
                    ROW_NUMBER() OVER (
                        PARTITION BY Pos.DocNo 
                        ORDER BY ISNULL(PosPayment.DocNo, ''), PosPayment.PaymentType, PosPayment.PaymentAmt
                    ) AS PaymentSeq,
                    SUM(CAST(PosPayment.PaymentAmt AS DECIMAL(18,2))) OVER (
                        PARTITION BY Pos.DocNo
                    ) AS DocTotalPaymentAmt

                FROM Pos

                INNER JOIN PosPayment 
                    ON Pos.DocKey = PosPayment.DocKey

                LEFT JOIN Debtor 
                    ON Pos.DebtorCode = Debtor.AccNo

                LEFT JOIN PosTerminal 
                    ON Pos.TerminalID = PosTerminal.TerminalID

                LEFT JOIN Location 
                    ON PosTerminal.Outlet = Location.Location

                WHERE 
                    (@ApplyDate = 0 OR (Pos.CreatedTime >= @DateFrom AND Pos.CreatedTime < DATEADD(DAY,1,@DateTo)))
                    AND (
                        @Store IS NULL 
                        OR @Store = ''
                        OR Location.Location = @Store
                    )
                    AND (
                        @Keyword IS NULL
                        OR @Keyword = ''
                        OR Pos.DocNo IN (SELECT SINo FROM InvoiceData)
                    )
            ),

            PaymentTotal AS (
                SELECT 
                    SINo,
                    MAX(DocNo2) AS DocNo2,
                    SUM(PaymentAmt) AS TotalAmt,
                    MAX(Total2) AS TotalAmt2,
                    MAX(CreatedTime) AS CreatedTime,
                    MAX(TaxInvoiceDate) AS TaxInvoiceDate

                FROM PaymentData

                GROUP BY SINo
            )

            SELECT 
                t.[Company Code],
                FORMAT(t.[Posting Date],'MM/dd/yyyy') AS [Posting Date],
                FORMAT(t.[Document Date],'MM/dd/yyyy') AS [Document Date],
                t.DocumentType AS [Document Type],
                t.Currency,
                t.[Header Text],
                t.Reference,

                CASE 
                    WHEN A.Description IS NOT NULL 
                        THEN A.AccountType 
                    ELSE '' 
                END AS [Account Type],

                -- =========================================================
                -- DP DISCOUNT ACCOUNT OVERRIDE
                -- If Customer + / Customer - and DP discount:
                -- Account = 11062473
                -- Otherwise retain existing Account
                -- =========================================================
                CASE
                    WHEN t.IsDPDiscount = 1
                         AND A.AccountType = 'Customer'
                        THEN '11062473'

                    WHEN A.Description IS NOT NULL 
                        THEN A.AccountNo

                    ELSE ''
                END AS [Account],

                t.[SGL Indicator],
                t.[Header Text] AS [Item Text],
                t.Amount,
                t.TaxCode,
                '' AS [Cost Center],
                t.ProfitCenter AS [Profit Center],
                '' AS [Quantity],
                '' AS [Base UOM],
                '' AS [Payment Terms],
                '' AS [Baseline Date],
                '' AS [Product],
                t.Plant,
                '' AS [Sales Organization],
                '' AS [Distribution Channel],
                '' AS [Division],
                '' AS [Jewelry Color Tag],
                '' AS [Jewelry Type],

                CASE 
                    WHEN t.[WTX Code] = ''
                        THEN ''
                    WHEN A.AccountType = 'Customer'
                         AND t.[WTX Code] <> A.AccountNo
                        THEN '00'
                    ELSE ''
                END AS [WTX Code]

                --t.GLDescription

            FROM (

                -- =========================
                -- ORIGINAL TRANSACTIONS
                -- =========================
                SELECT 
                    'JJDB' AS [Company Code],
                    I.[Posting Date],
                    I.[Document Date],

                    CASE
                        WHEN I.IsForfeited = 1 THEN 'Refund'
                        WHEN I.IsReturn = 1 THEN 'Refund'
                        ELSE 'Invoice'
                    END AS DocumentType,

                    'PHP' AS Currency,

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    CASE
			            -- CANCELLED TRANSACTION
			            -- Do not use DocNo2
			            WHEN EXISTS (
				            SELECT 1
				            FROM Pos
				            WHERE Pos.DocNo = I.SINo
				              AND Pos.Cancelled = 'T'
			            )
				            THEN I.Remarks

			            -- RETURN TRANSACTION
			            -- Use ReturnFromDocNo instead of DocNo2
			            WHEN NULLIF(LTRIM(RTRIM(I.ReturnFromDocNo)), '') IS NOT NULL
				            THEN I.ReturnFromDocNo

			            -- NORMAL TRANSACTION
			            WHEN NULLIF(LTRIM(RTRIM(P.DocNo2)), '') IS NULL
				            THEN I.Remarks

			            WHEN NULLIF(LTRIM(RTRIM(I.Remarks)), '') IS NULL
				            THEN P.DocNo2

			            ELSE
				            P.DocNo2 + ' - ' + I.Remarks
		            END AS [SGL Indicator],
                    '' AS [Item Text],

                    CASE
                        WHEN I.IsForfeited = 1
                            THEN CAST(P.PaymentAmt AS DECIMAL(18,2))

                        WHEN I.IsReservation = 1
                             AND NULLIF(LTRIM(RTRIM(I.DocNo2)), '') IS NULL
                            THEN CAST(P.PaymentAmt * -1 AS DECIMAL(18,2))

                        WHEN I.IsReservation = 1
                            THEN CAST(ROUND(P.PaymentAmt * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2))

                        WHEN I.IsPrepayment = 1
                             AND NULLIF(LTRIM(RTRIM(I.DocNo2)), '') IS NULL
                            THEN CAST(P.PaymentAmt * -1 AS DECIMAL(18,2))

                        WHEN I.IsPrepayment = 1
                            THEN CAST(ROUND(P.PaymentAmt * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2))

                        WHEN I.IsReturn = 1
                            THEN CAST(ROUND(I.ReturnSales * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) AS DECIMAL(18,2))

                        ELSE 
                            CAST(ROUND(I.Sales * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2)) 
                    END AS Amount,

                    CASE 
                        WHEN I.IsForfeited = 1
                            THEN 'OX'

                        WHEN (I.IsReservation = 1 OR I.IsPrepayment = 1)
                             AND NULLIF(LTRIM(RTRIM(I.DocNo2)), '') IS NULL
                            THEN 'OX'

                        ELSE I.TaxCode
                    END AS [TaxCode],

                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    CASE 
                        WHEN I.IsExtension = 1
                            THEN 'OTHER INCOME'

                        WHEN I.IsForfeited = 1
                            THEN 'DEPOSIT FROM CUSTOMERS CURRENT 1'

                        WHEN (I.IsReservation = 1 OR I.IsPrepayment = 1)
                             AND NULLIF(LTRIM(RTRIM(I.DocNo2)), '') IS NULL
                            THEN 'DEPOSIT FROM CUSTOMERS CURRENT'

                        WHEN [Product Tag] LIKE '%Services%' 
                            THEN 'Service Fee Income'

                        ELSE 'Sales'
                    END AS GLDescription,

                    1 AS SortOrder,
                    P.PaymentSeq AS BlockOrder

                FROM InvoiceData I
                INNER JOIN PaymentData P 
                    ON I.SINo = P.SINo


                UNION ALL


                SELECT 
                    'JJDB',
                    I.[Posting Date],
                    I.[Document Date],

                    CASE
                        WHEN I.IsReturn = 1 THEN 'Refund'
                        ELSE 'Invoice'
                    END,

                    'PHP',

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CASE
                        WHEN I.IsReturn = 1
                            THEN CAST(ROUND(I.SalesDiscount * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2))
                        ELSE
                            CAST(ROUND(I.SalesDiscount * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) AS DECIMAL(18,2))
                    END AS Amount,

                    I.TaxCode,
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Sales Discount',

                    2,
                    P.PaymentSeq

                FROM InvoiceData I
                INNER JOIN PaymentData P
                    ON I.SINo = P.SINo
                WHERE I.SalesDiscount <> 0


                UNION ALL


                SELECT 
                    'JJDB',
                    I.[Posting Date],
                    I.[Document Date],

                    CASE
                        WHEN I.IsForfeited = 1 THEN 'Refund'
                        WHEN I.IsReturn = 1 THEN 'Refund'
                        ELSE 'Invoice'
                    END,

                    'PHP',

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CASE
                        WHEN I.IsForfeited = 1
                            THEN CAST(
                                (P.PaymentAmt - (P.PaymentAmt/1.12)) * -1 
                                AS DECIMAL(18,2)
                            )

                        WHEN I.IsReturn = 1
                            THEN CAST(ROUND(I.OutputTax * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2))

                        ELSE
                            CAST(ROUND(I.OutputTax * (CASE WHEN P.DocTotalPaymentAmt > 0 THEN P.PaymentAmt / P.DocTotalPaymentAmt ELSE 1.0 END), 2) * -1 AS DECIMAL(18,2))
                    END AS Amount,

                    I.TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Output Tax',

                    3,
                    P.PaymentSeq

                FROM InvoiceData I
                INNER JOIN PaymentData P 
                    ON I.SINo = P.SINo

                WHERE 
                    (I.IsReservation = 0 AND I.IsPrepayment = 0) 
                    OR I.DocNo2 IS NOT NULL


                UNION ALL


                -- =========================================================
                -- SO PARTIAL PAYMENTS / CUSTOMER DEPOSIT
                -- =========================================================

                SELECT 
                    'JJDB' AS [Company Code],
                    I.[Posting Date],
                    I.[Document Date],

                    'Invoice' AS DocumentType,

                    'PHP' AS Currency,

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        ELSE
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                
                            + SUBSTRING(
                                I.SINo, 
                                CHARINDEX('-', I.SINo) + 1, 
                                LEN(I.SINo)
                            )
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(
                        (
                            SELECT SUM(PP.PaymentAmt)
                            FROM Pos SO
                            INNER JOIN PosPayment PP
                                ON SO.DocKey = PP.DocKey
                            WHERE 
                                SO.DocNo2 = I.DocNo2
                                AND PP.DocNo LIKE I.DocNo2 + '-%'
                                AND PP.DocNo NOT LIKE I.SINo + '-%'
                        )
                        AS DECIMAL(18,2)
                    ) AS Amount,

                    'OX' AS TaxCode,
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    I.DebtorAccNo AS [WTX Code],

                    I.IsDPDiscount,

                    'DEPOSIT FROM CUSTOMERS CURRENT' AS GLDescription,

                    4 AS SortOrder,
                    1 AS BlockOrder

                FROM InvoiceData I

                WHERE 
                    NULLIF(LTRIM(RTRIM(I.DocNo2)), '') IS NOT NULL

                    AND EXISTS (
                        SELECT 1
                        FROM Pos SO
                        INNER JOIN PosPayment PP
                            ON SO.DocKey = PP.DocKey
                        WHERE 
                            SO.DocNo2 = I.DocNo2
                            AND PP.DocNo LIKE I.DocNo2 + '-%'
                            AND PP.DocNo NOT LIKE I.SINo + '-%'
                    )


                UNION ALL


                -- =========================================================
                -- FINAL SI PAYMENT / NORMAL SI PAYMENT
                -- =========================================================

                SELECT 
                    'JJDB' AS [Company Code],
                    P.CreatedTime,
                    P.TaxInvoiceDate,

                    CASE
                        WHEN I.IsForfeited = 1 THEN 'Refund'
                        WHEN I.IsReturn = 1 THEN 'Credit Memo'
                        ELSE 'Payment'
                    END AS DocumentType,

                    'PHP' AS Currency,

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CASE
                        WHEN I.IsForfeited = 1
                            THEN CAST(
                                (P.PaymentAmt / 1.12) * -1
                                AS DECIMAL(18,2)
                            )

                        WHEN I.IsReturn = 1
                            THEN CAST(
                                P.PaymentAmt
                                AS DECIMAL(18,2)
                            )

                        ELSE CAST(
                            P.PaymentAmt
                            AS DECIMAL(18,2)
                        )
                    END AS Amount,

                    CASE 
                        WHEN P.CustomPaymentType = 'Other Income'
                            THEN 'O1'
                        ELSE ''
                    END AS TaxCode,

                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    I.DebtorAccNo AS [WTX Code],

                    I.IsDPDiscount,

                    CASE
                        WHEN P.PaymentType = 1
                            THEN 'Cash' + ' - ' + I.StockLocation

                        WHEN P.PaymentType = 4
                            THEN 'Cheque'

                        ELSE COALESCE(
                            NULLIF(P.CustomPaymentType, ''),
                            CASE
                                WHEN P.CCType IS NOT NULL
                                     AND P.CCType <> ''
                                    THEN 'Credit - ' + P.CCType

                                WHEN P.DCType IS NOT NULL
                                     AND P.DCType <> ''
                                    THEN 'Debit - ' + P.DCType

                                ELSE 'Payment Type '
                                     + CAST(P.PaymentType AS VARCHAR(10))
                            END
                        )
                    END AS GLDescription,

                    4 AS SortOrder,
                    P.PaymentSeq AS BlockOrder

                FROM PaymentData P

                INNER JOIN InvoiceData I
                    ON P.SINo = I.SINo


                UNION ALL


                -- =========================================================
                -- 5. CUSTOMER +
                -- =========================================================
                SELECT 
                    'JJDB',
                    P.CreatedTime,
                    P.TaxInvoiceDate,

                    CASE
                        WHEN I.IsReturn = 1 THEN 'Refund'
                        ELSE 'Invoice'
                    END AS DocumentType,

                    'PHP',

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(P.PaymentAmt AS DECIMAL(18,2)) AS Amount,

                    '' AS TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    CASE 
                        WHEN I.CompanyName IS NULL 
                             OR LTRIM(RTRIM(I.CompanyName)) = '' 
                            THEN 'Customer Name'
                        ELSE I.CompanyName
                    END,

                    5,
                    P.PaymentSeq

                FROM PaymentData P
                JOIN InvoiceData I 
                    ON I.SINo = P.SINo

                WHERE I.IsForfeited = 0


                UNION ALL


                -- =========================================================
                -- 6. CUSTOMER -
                -- =========================================================
                SELECT 
                    'JJDB',
                    P.CreatedTime,
                    P.TaxInvoiceDate,

                    CASE
                        WHEN I.IsReturn = 1 THEN 'Credit Memo'
                        ELSE 'Payment'
                    END AS DocumentType,

                    'PHP',

                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        WHEN I.SINo LIKE 'SO-%' THEN
                            I.SINo

                        ELSE
                            CASE 
                                WHEN CHARINDEX('-', I.SINo) > 0 THEN
                                    LEFT(I.SINo, CHARINDEX('-', I.SINo))
                                    + SUBSTRING(
                                        I.SINo, 
                                        CHARINDEX('-', I.SINo) + 1, 
                                        LEN(I.SINo)
                                    )
                                ELSE I.SINo
                            END
                    END AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(P.PaymentAmt * -1 AS DECIMAL(18,2)) AS Amount,

                    '' AS TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    CASE 
                        WHEN I.CompanyName IS NULL 
                             OR LTRIM(RTRIM(I.CompanyName)) = '' 
                            THEN 'Customer Name'
                        ELSE I.CompanyName
                    END,

                    6,
                    P.PaymentSeq

                FROM PaymentData P
                JOIN InvoiceData I 
                    ON I.SINo = P.SINo

                WHERE I.IsForfeited = 0


                -- =========================
                -- CANCELLED TRANSACTIONS
                -- =========================

                UNION ALL


                SELECT 
                    'JJDB',
                    I.[Posting Date],
                    I.[Document Date],
                    'Refund',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
        
                    CASE 
                        WHEN I.SINo LIKE '%-SR%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'RT'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SR',
                                ''
                            )

                        WHEN I.SINo LIKE '%-SO%' THEN
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                            + 'SO'
                            + REPLACE(
                                SUBSTRING(
                                    I.SINo, 
                                    CHARINDEX('-', I.SINo) + 1, 
                                    LEN(I.SINo)
                                ),
                                'SO',
                                ''
                            )

                        ELSE
                            LEFT(I.SINo, CHARINDEX('-', I.SINo))
                
                            + SUBSTRING(
                                I.SINo, 
                                CHARINDEX('-', I.SINo) + 1, 
                                LEN(I.SINo)
                            )
                    END AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(I.Sales AS DECIMAL(18,2)),

                    I.TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Sales - Cancelled',

                    1,
                    2

                FROM InvoiceData I

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = I.SINo
                      AND Pos.Cancelled = 'T'
                )


                UNION ALL


                SELECT 
                    'JJDB',
                    I.[Posting Date],
                    I.[Document Date],
                    'Refund',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(I.SalesDiscount * -1 AS DECIMAL(18,2)),

                    I.TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Sales Discount',

                    2,
                    2

                FROM InvoiceData I

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = I.SINo
                      AND Pos.Cancelled = 'T'
                )

                AND I.SalesDiscount <> 0


                UNION ALL


                SELECT 
                    'JJDB',
                    I.[Posting Date],
                    I.[Document Date],
                    'Refund',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(I.OutputTax AS DECIMAL(18,2)),

                    I.TaxCode,  
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Output Tax',

                    3,
                    2

                FROM InvoiceData I

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = I.SINo
                      AND Pos.Cancelled = 'T'
                )


                UNION ALL


                SELECT 
                    'JJDB',
                    P.CreatedTime,
                    P.TaxInvoiceDate,
                    'Credit Memo',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(P.PaymentAmt * -1 AS DECIMAL(18,2)),

                    '' AS TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    'Cash',

                    4,
                    2

                FROM PaymentData P

                INNER JOIN InvoiceData I 
                    ON P.SINo = I.SINo

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = P.SINo
                      AND Pos.Cancelled = 'T'
                )


                UNION ALL


                SELECT 
                    'JJDB',
                    PT.CreatedTime,
                    PT.TaxInvoiceDate,
                    'Credit Memo',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(PT.TotalAmt AS DECIMAL(18,2)),

                    '' AS TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    CASE 
                        WHEN I.CompanyName IS NULL 
                             OR LTRIM(RTRIM(I.CompanyName)) = '' 
                            THEN 'Customer Name'
                        ELSE I.CompanyName
                    END,

                    5,
                    2

                FROM PaymentTotal PT

                JOIN InvoiceData I 
                    ON I.SINo = PT.SINo

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = PT.SINo
                      AND Pos.Cancelled = 'T'
                )


                UNION ALL


                SELECT 
                    'JJDB',
                    PT.CreatedTime,
                    PT.TaxInvoiceDate,
                    'Refund',
                    'PHP',

                    LEFT(I.DocNo2, CHARINDEX('-', I.DocNo2))
                    + 'SV'
                    + SUBSTRING(
                        I.DocNo2,
                        CHARINDEX('-', I.DocNo2) + 3,
                        LEN(I.DocNo2)
                    ) AS [Header Text],

                    '' AS Reference,
                    '' AS [SGL Indicator],
                    '' AS [Item Text],

                    CAST(PT.TotalAmt * -1 AS DECIMAL(18,2)),

                    '' AS TaxCode, 
                    '' AS [Cost Center],
                    I.ProfitCenter,
                    '' AS [Quantity],
                    '' AS [Base UOM],
                    '' AS [Payment Terms],
                    '' AS [Baseline Date],
                    '' AS [Product],
                    I.Plant,
                    '' AS [Sales Organization],
                    '' AS [Distribution Channel],
                    '' AS [Division],
                    '' AS [Jewelry Color Tag],
                    '' AS [Jewelry Type],
                    '' AS [WTX Code],

                    I.IsDPDiscount,

                    CASE 
                        WHEN I.CompanyName IS NULL 
                             OR LTRIM(RTRIM(I.CompanyName)) = '' 
                            THEN 'Customer Name'
                        ELSE I.CompanyName
                    END,

                    6,
                    2

                FROM PaymentTotal PT

                JOIN InvoiceData I 
                    ON I.SINo = PT.SINo

                WHERE EXISTS (
                    SELECT 1
                    FROM Pos
                    WHERE Pos.DocNo = PT.SINo
                      AND Pos.Cancelled = 'T'
                )

            ) t

            LEFT JOIN Account A
                ON UPPER(A.Description) =
                   UPPER(
                       CASE 
                           WHEN t.GLDescription = 'Sales - Cancelled' 
                               THEN 'Sales'
                           ELSE t.GLDescription
                       END
                   )

            ORDER BY 
                t.[Posting Date],
                t.[Header Text],
                t.BlockOrder,
                t.SortOrder;
            ";
        }



        public static void ExportToExcel(DataTable dt, string path)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Report");
                wb.SaveAs(path);
            }
        }


    }
}
