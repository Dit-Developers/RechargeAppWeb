using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages.User
{
    public class OnlineRechargeModel : PageModel
    {
        [BindProperty]
        public string MobileNumber { get; set; }

        [BindProperty]
        public int PlanID { get; set; }

        public List<RechargePlans> RechargePlans { get; set; } = new();
        public string Message { get; set; }

        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public void OnGet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT PlanID, Description, Amount, Validity FROM RechargePlans";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RechargePlans.Add(new RechargePlans
                                {
                                    PlanID = reader.GetInt32(0),
                                    Description = reader.GetString(1),
                                    Amount = reader.GetDecimal(2),
                                    Validity = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = "Error fetching recharge plans: " + ex.Message;
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet(); // Reload plans
                return Page();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string insertQuery = "INSERT INTO Transactions (UserID, MobileNumber, PlanID, PaymentStatus, TransactionDate) VALUES (@UserID, @MobileNumber, @PlanID, 'Pending', GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", 2); // Replace with actual logged-in user ID
                        cmd.Parameters.AddWithValue("@MobileNumber", MobileNumber);
                        cmd.Parameters.AddWithValue("@PlanID", PlanID);

                        cmd.ExecuteNonQuery();
                    }
                }
                Message = "Recharge request submitted successfully!";
            }
            catch (Exception ex)
            {
                Message = "Error processing recharge: " + ex.Message;
            }

            OnGet(); // Reload plans
            return Page();
        }
    }

    public class RechargePlans
    {
        public int PlanID { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Validity { get; set; }
    }
}
