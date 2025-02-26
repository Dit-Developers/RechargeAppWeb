using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages.User
{
    public class MyAccountModel : PageModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }

        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public void OnGet()
        {
            // Get the logged-in username from session
            string sessionUsername = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(sessionUsername))
            {
                Response.Redirect("/Login");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Username, Email FROM Users WHERE Username = @Username", connection))
                {
                    cmd.Parameters.AddWithValue("@Username", sessionUsername);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Username = reader.GetString(0);
                            Email = reader.GetString(1);
                        }
                    }
                }
            }
        }

        public void OnPost()
        {
            string sessionUsername = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(sessionUsername))
            {
                Response.Redirect("/Login");
                return;
            }

            string newUsername = Request.Form["Username"];
            string newEmail = Request.Form["Email"];
            string newPassword = Request.Form["NewPassword"];
            string confirmPassword = Request.Form["ConfirmPassword"];

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Validate if the username or email already exists (except for current user)
                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE (Username = @NewUsername OR Email = @NewEmail) AND Username != @CurrentUsername", connection))
                {
                    checkCmd.Parameters.AddWithValue("@NewUsername", newUsername);
                    checkCmd.Parameters.AddWithValue("@NewEmail", newEmail);
                    checkCmd.Parameters.AddWithValue("@CurrentUsername", sessionUsername);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        Message = "Username or Email already exists!";
                        return;
                    }
                }

                // Update query
                string updateQuery;
                if (!string.IsNullOrEmpty(newPassword))
                {
                    if (newPassword != confirmPassword)
                    {
                        Message = "Passwords do not match!";
                        return;
                    }
                    updateQuery = "UPDATE Users SET Username = @NewUsername, Email = @NewEmail, Password = @NewPassword, UpdatedAt = GETDATE() WHERE Username = @CurrentUsername";
                }
                else
                {
                    updateQuery = "UPDATE Users SET Username = @NewUsername, Email = @NewEmail, UpdatedAt = GETDATE() WHERE Username = @CurrentUsername";
                }

                using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@NewUsername", newUsername);
                    updateCmd.Parameters.AddWithValue("@NewEmail", newEmail);
                    updateCmd.Parameters.AddWithValue("@CurrentUsername", sessionUsername);

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    }

                    updateCmd.ExecuteNonQuery();
                }

                // Update session username
                HttpContext.Session.SetString("Username", newUsername);

                Message = "Account updated successfully!";
            }
        }
    }
}
