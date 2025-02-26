using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages
{
    public class LoginModel : PageModel
    {
        public string ErrorMessage { get; set; }
        private const int MaxFailedAttempts = 4; // Maximum allowed failed attempts

        public void OnGet() { }

        public void OnPost()
        {
            string userInput = Request.Form["username"]; // Can be username or email
            string password = Request.Form["password"];

            if (string.IsNullOrEmpty(userInput) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            string connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserID, Username, Password, Role, Status FROM Users WHERE Username = @userInput OR Email = @userInput";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userInput", userInput);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["UserID"]);
                            string username = reader["Username"].ToString();
                            string storedPassword = reader["Password"].ToString();
                            string role = reader["Role"].ToString();
                            string status = reader["Status"].ToString();

                            // Check if the account is already inactive
                            if (status.ToLower() == "inactive")
                            {
                                ErrorMessage = "Your account is inactive. Please contact our support team.";
                                return;
                            }

                            // Verify password
                            if (VerifyPassword(password, storedPassword))
                            {
                                // Save user info to session
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("Role", role);
                                HttpContext.Session.Remove("FailedAttempts"); // Reset failed attempts

                                // Redirect based on role
                                Response.Redirect(role == "Admin" ? "/Admin/Index" : "/User/Index");
                            }
                            else
                            {
                                reader.Close();

                                int failedAttempts = HttpContext.Session.GetInt32("FailedAttempts") ?? 0;
                                failedAttempts++;

                                if (failedAttempts > MaxFailedAttempts)
                                {
                                    InactivateUser(connection, userId, role); 
                                    ErrorMessage = "Your account has been locked due to too many failed login attempts. Please contact support.";
                                }
                                else
                                {
                                    HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);
                                    ErrorMessage = $"Incorrect username or password. You have {MaxFailedAttempts - failedAttempts + 1} attempts left.";
                                }
                            }
                        }
                        else
                        {
                            ErrorMessage = "User not found.";
                        }
                    }
                }
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }

        private void InactivateUser(SqlConnection connection, int userId, string role)
        {
            if (role == "Admin")
            {
                return; 
            }

            string inactivateQuery = "UPDATE Users SET Status = 'Inactive', UpdatedAt = GETDATE() WHERE UserID = @UserID AND Status = 'Active';";
            using (SqlCommand cmd = new SqlCommand(inactivateQuery, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
