using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace RechargeAppWeb.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public string Username { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulRecharges { get; set; }
        public int FailedTransactions { get; set; }
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<RechargePlan> RechargePlans { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Feedback> Feedbacks { get; set; } = new();
        public List<PostpaidPayment> PostpaidPayments { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<UserService> UserServices { get; set; } = new();
        public List<CustomerCare> CustomerCareContacts { get; set; } = new();

        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(Username))
            {
                Response.Redirect("/Login");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Get User Role
                string userRole = "";
                using (SqlCommand cmd = new SqlCommand("SELECT Role FROM Users WHERE Username = @Username", connection))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);
                    object result = cmd.ExecuteScalar();
                    userRole = result?.ToString() ?? "";
                }

                if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("../User/Index");
                    return;
                }

                // Get Dashboard Metrics
                using (SqlCommand cmd = new SqlCommand("SELECT SUM(RP.Amount) FROM Transactions T JOIN RechargePlans RP ON T.PlanID = RP.PlanID WHERE T.PaymentStatus = 'Completed'", connection))
                {
                    TotalBalance = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
                }
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Transactions", connection))
                {
                    TotalTransactions = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Transactions WHERE PaymentStatus = 'Completed'", connection))
                {
                    SuccessfulRecharges = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Transactions WHERE PaymentStatus = 'Failed'", connection))
                {
                    FailedTransactions = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Get Users List
                using (SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, Role, Status FROM Users", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Users.Add(new User
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2),
                                Role = reader.GetString(3),
                                Status = reader.GetString(4)
                            });
                        }
                    }
                }

            }
        }
        public IActionResult OnPostLogout()
        {
            // Clear the session data
            HttpContext.Session.Clear();

            // Redirect to login page after logging out
            return RedirectToPage("../Login");
        }
        public IActionResult OnPostToggleStatus(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string currentStatus = "";

                using (SqlCommand cmd = new SqlCommand("SELECT Status FROM Users WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    object result = cmd.ExecuteScalar();
                    currentStatus = result?.ToString() ?? "Inactive";
                }

                string newStatus = currentStatus == "Active" ? "Inactive" : "Active";

                using (SqlCommand cmd = new SqlCommand("UPDATE Users SET Status = @Status WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage();
        }
        public class Transaction
        {
            public int TransactionID { get; set; }
            public string MobileNumber { get; set; }
            public int PlanID { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime TransactionDate { get; set; }
        }

        public class RechargePlan
        {
            public int PlanID { get; set; }
            public string PlanType { get; set; }
            public decimal Amount { get; set; }
            public int Validity { get; set; }
            public string Description { get; set; }
        }

        public class User
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string Status { get; set; }
        }

        public class Feedback
        {
            public int FeedbackID { get; set; }
            public int? UserID { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAt { get; set; }
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

        public class Service
        {
            public int ServiceID { get; set; }
            public string ServiceName { get; set; }
        }

        public class UserService
        {
            public int UserServiceID { get; set; }
            public int UserID { get; set; }
            public int ServiceID { get; set; }
            public string Status { get; set; }
            public DateTime ActivatedAt { get; set; }
        }

        public class CustomerCare
        {
            public int ContactID { get; set; }
            public string ContactNumber { get; set; }
            public string Email { get; set; }
        }
    }
}
