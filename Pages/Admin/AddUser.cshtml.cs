using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages.Admin
{
    public class AddUserModel : PageModel
    {
        public string Message { get; set; }
        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public List<string> Roles { get; } = new List<string> { "User", "Admin" };
        public List<string> Statuses { get; } = new List<string> { "Active", "Inactive" };

        public void OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                Response.Redirect("/Login");
        }

        public void OnPost(string username, string email, string password, string role, string status)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Message = "All fields are required.";
                return;
            }

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                string query = "INSERT INTO Users (Username, Email, Password, Role, Status, CreatedAt, UpdatedAt) VALUES (@Username, @Email, @Password, @Role, @Status, GETDATE(), GETDATE())";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.ExecuteNonQuery();

                Message = "User added successfully.";
            }
            catch (SqlException ex)
            {
                Message = "Database error: " + ex.Message;
            }
        }
    }
}
