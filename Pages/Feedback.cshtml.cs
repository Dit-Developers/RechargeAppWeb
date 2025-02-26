using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages
{
    public class FeedbackModel : PageModel
    {
        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public string Message { get; set; }
        public bool IsUserLoggedIn { get; private set; }
        public int? UserID { get; private set; }

        public void OnGet()
        {
            // Check if user is logged in
            var userIdString = HttpContext.Session.GetString("UserID");
            IsUserLoggedIn = int.TryParse(userIdString, out int userId);

            if (IsUserLoggedIn)
            {
                UserID = userId;
            }
        }

        public void OnPost(string message, string username)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Message = "Feedback cannot be empty.";
                return;
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query;
                using var cmd = new SqlCommand();
                cmd.Connection = connection;

                if (HttpContext.Session.GetString("UserID") != null)
                {
                    // Logged-in user feedback
                    cmd.CommandText = "INSERT INTO Feedback (UserID, Message, CreatedAt) VALUES (@UserID, @Message, GETDATE())";
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                }
                else
                {
                    // Guest feedback (UserID will be NULL)
                    cmd.CommandText = "INSERT INTO Feedback (Message, CreatedAt) VALUES (@Message, GETDATE())";
                }

                cmd.Parameters.AddWithValue("@Message", message);
                cmd.ExecuteNonQuery();

                Message = "Feedback submitted successfully.";
            }
            catch (SqlException ex)
            {
                Message = "Database error: " + ex.Message;
            }
        }
    }
}
