using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages.Admin
{
    public class EditUserModel : PageModel
    {
        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        [BindProperty]
        public int UserID { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Role { get; set; }

        [BindProperty]
        public string Status { get; set; }

        public List<string> Roles { get; } = new List<string> { "User", "Admin" };
        public List<string> Statuses { get; } = new List<string> { "Active", "Inactive" };

        public void OnGet(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Username, Email, Role, Status FROM Users WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserID = userId;
                            Username = reader["Username"].ToString();
                            Email = reader["Email"].ToString();
                            Role = reader["Role"].ToString();
                            Status = reader["Status"].ToString();
                        }
                        else
                        {
                            Response.Redirect("/Admin/Index");
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Users SET Username = @Username, Email = @Email, Role = @Role, Status = @Status WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Role", Role);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Index");
        }
    }
}
