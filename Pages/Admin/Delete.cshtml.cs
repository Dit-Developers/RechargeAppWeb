using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace RechargeAppWeb.Pages.Admin
{
    public class DeleteModel : PageModel
    {
        private readonly string _connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

        public int UserID { get; set; }
        public string Username { get; set; }

        public void OnGet(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Username FROM Users WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        Username = result.ToString();
                        UserID = userId;
                    }
                    else
                    {
                        Response.Redirect("/Index");
                    }
                }
            }
        }

        public IActionResult OnPostConfirmDelete(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/Admin/Index");
        }
    }
}
