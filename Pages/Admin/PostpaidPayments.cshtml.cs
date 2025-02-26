using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RechargeAppWeb.Pages.Admin
{
    public class DataModel : PageModel
    {
        public List<PostpaidPayment> PostpaidPayments { get; set; } = new();
        public List<RechargePlan> RechargePlans { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();

        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";
        private readonly List<string> _validStatuses = new() { "Failed", "Completed", "Pending", "Success" };

        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Fetch Postpaid Payments
                using (SqlCommand cmd = new SqlCommand("SELECT PaymentID, UserID, MobileNumber, Amount, PaymentStatus, PaymentDate FROM PostpaidPayments", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PostpaidPayments.Add(new PostpaidPayment
                        {
                            PaymentID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            MobileNumber = reader.GetString(2),
                            Amount = reader.GetDecimal(3),
                            PaymentStatus = reader.GetString(4),
                            PaymentDate = reader.GetDateTime(5)
                        });
                    }
                }

                // Fetch Recharge Plans
                using (SqlCommand cmd = new SqlCommand("SELECT PlanID, PlanType, Amount, Validity, Description FROM RechargePlans", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RechargePlans.Add(new RechargePlan
                        {
                            PlanID = reader.GetInt32(0),
                            PlanType = reader.GetString(1),
                            Amount = reader.GetDecimal(2),
                            Validity = reader.GetInt32(3),
                            Description = reader.GetString(4)
                        });
                    }
                }

                // Fetch Transactions
                using (SqlCommand cmd = new SqlCommand("SELECT TransactionID, UserID, MobileNumber, PlanID, PaymentStatus, TransactionDate FROM Transactions", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Transactions.Add(new Transaction
                        {
                            TransactionID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            MobileNumber = reader.GetString(2),
                            PlanID = reader.GetInt32(3),
                            PaymentStatus = reader.GetString(4),
                            TransactionDate = reader.GetDateTime(5)
                        });
                    }
                }
            }
        }

        public IActionResult OnPostDeletePayment(int paymentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM PostpaidPayments WHERE PaymentID = @PaymentID", connection))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting payment: {ex.Message}");
            }
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateStatus(int paymentId, string newStatus)
        {
            if (!_validStatuses.Contains(newStatus))
            {
                return BadRequest("Invalid payment status.");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE PostpaidPayments SET PaymentStatus = @NewStatus WHERE PaymentID = @PaymentID", connection))
                    {
                        cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                        cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error updating status: {ex.Message}");
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDeletePlan(int planId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM RechargePlans WHERE PlanID = @PlanID", connection))
                    {
                        cmd.Parameters.AddWithValue("@PlanID", planId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting plan: {ex.Message}");
            }
            return RedirectToPage();
        }

        public IActionResult OnPostUpdatePlan(int planId, string planType, decimal amount, int validity, string description)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE RechargePlans SET PlanType = @PlanType, Amount = @Amount, Validity = @Validity, Description = @Description WHERE PlanID = @PlanID", connection))
                    {
                        cmd.Parameters.AddWithValue("@PlanType", planType);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Validity", validity);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@PlanID", planId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating plan: {ex.Message}");
            }
            return RedirectToPage();
        }

        public class PostpaidPayment
        {
            public int PaymentID { get; set; }
            public int UserID { get; set; }
            public string MobileNumber { get; set; }
            public decimal Amount { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime PaymentDate { get; set; }
        }

        public class RechargePlan
        {
            public int PlanID { get; set; }
            public string PlanType { get; set; }
            public decimal Amount { get; set; }
            public int Validity { get; set; }
            public string Description { get; set; }
        }

        public class Transaction
        {
            public int TransactionID { get; set; }
            public int UserID { get; set; }
            public string MobileNumber { get; set; }
            public int PlanID { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime TransactionDate { get; set; }
        }
    }
}