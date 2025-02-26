using Microsoft.Data.SqlClient;

namespace RechargeAppWeb
{
    public class RechargePlans
    {
        public int PlanID { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Validity { get; set; }

        public static RechargePlans GetPlanById(int planID)
        {
            RechargePlans plan = null;
            string connectionString = "Data Source=DIT-INSTITUTE-P\\SQLEXPRESS01;Initial Catalog=rechargeapp;Integrated Security=True;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PlanID, Description, Amount, Validity FROM RechargePlans WHERE PlanID = @PlanID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlanID", planID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            plan = new RechargePlans
                            {
                                PlanID = reader.GetInt32(0),
                                Description = reader.GetString(1),
                                Amount = reader.GetDecimal(2),
                                Validity = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }

            return plan;
        }
    }
}
