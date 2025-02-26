using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages
{
    public class CustomerCareModel : PageModel
    {
        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public string Message { get; set; }
        public bool IsUserLoggedIn { get; private set; }
        public int? UserID { get; private set; }

        public void OnGet()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            IsUserLoggedIn = int.TryParse(userIdString, out int userId);
            if (IsUserLoggedIn)
            {
                UserID = userId;
            }
        }

        public void OnPost(string email, string message, string username)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(email))
            {
                Message = "Email and inquiry message cannot be empty.";
                return;
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var cmd = new SqlCommand();
                cmd.Connection = connection;

                if (HttpContext.Session.GetString("UserID") != null)
                {
                    cmd.CommandText = "INSERT INTO CustomerCareForm (UserID, Username, Email, Message, CreatedAt) VALUES (@UserID, @Username, @Email, @Message, GETDATE())";
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@Username", username ?? (object)DBNull.Value);
                }
                else
                {
                    cmd.CommandText = "INSERT INTO CustomerCareForm (Username, Email, Message, CreatedAt) VALUES (@Username, @Email, @Message, GETDATE())";
                    cmd.Parameters.AddWithValue("@Username", username ?? (object)DBNull.Value);
                }

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.ExecuteNonQuery();

                Message = "Inquiry submitted successfully.";
            }
            catch (SqlException ex)
            {
                Message = "Database error: " + ex.Message;
            }
        }
    }
}
