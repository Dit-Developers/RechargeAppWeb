using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RechargeAppWeb.Pages.User
{
    public class IndexModel : PageModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<RechargePlan> RechargePlans { get; set; } = new();
        public List<PostpaidPayment> PostpaidPayments { get; set; } = new();
        public List<UserService> ActivatedServices { get; set; } = new();

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(Username))
            {
                Response.Redirect("/Login");
                return;
            }

            string connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        U.UserID, 
                        U.Username, 
                        U.Email, 
                        U.Role, 
                        U.Status, 
                        U.CreatedAt,
                        T.TransactionID, 
                        T.MobileNumber, 
                        T.PaymentStatus, 
                        T.TransactionDate,
                        RP.PlanType, 
                        RP.Amount, 
                        RP.Validity, 
                        RP.Description AS PlanDescription,
                        PP.PaymentID, 
                        PP.Amount AS BillAmount, 
                        PP.PaymentStatus AS BillPaymentStatus, 
                        PP.PaymentDate,
                        S.ServiceName,
                        US.Status AS ServiceStatus,
                        US.ActivatedAt
                    FROM Users U
                    LEFT JOIN Transactions T ON U.UserID = T.UserID
                    LEFT JOIN RechargePlans RP ON T.PlanID = RP.PlanID
                    LEFT JOIN PostpaidPayments PP ON U.UserID = PP.UserID
                    LEFT JOIN UserServices US ON U.UserID = US.UserID
                    LEFT JOIN Services S ON US.ServiceID = S.ServiceID
                    WHERE U.Username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Debugging: Check if transactions are being read
                            Debug.WriteLine("Reading data for user: " + Username);

                            if (string.IsNullOrEmpty(Email))
                            {
                                Email = reader["Email"].ToString();
                                Role = reader["Role"].ToString();
                                Status = reader["Status"].ToString();
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                            }

                            // Add transaction data if exists
                            if (!reader.IsDBNull(reader.GetOrdinal("TransactionID")))
                            {
                                var transaction = new Transaction
                                {
                                    TransactionID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                                    MobileNumber = reader.GetString(reader.GetOrdinal("MobileNumber")),
                                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                                    TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
                                };
                                RecentTransactions.Add(transaction);
                                Debug.WriteLine($"Added Transaction: {transaction.TransactionID} - {transaction.MobileNumber}");
                            }
                        }
                    }
                }
            }

            // Debugging: Check transaction count
            Debug.WriteLine($"Total Transactions: {RecentTransactions.Count}");
        }

        public class Transaction
        {
            public int TransactionID { get; set; }
            public string MobileNumber { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime TransactionDate { get; set; }
        }

        public class RechargePlan
        {
            public string PlanType { get; set; }
            public decimal Amount { get; set; }
            public int Validity { get; set; }
            public string Description { get; set; }
        }

        public class PostpaidPayment
        {
            public int PaymentID { get; set; }
            public decimal Amount { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime PaymentDate { get; set; }
        }

        public class UserService
        {
            public string ServiceName { get; set; }
            public string Status { get; set; }
            public DateTime ActivatedAt { get; set; }
        }
    }
}
