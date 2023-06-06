using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace EHS_2023.Controllers
{
    public class ValidateController : Controller
    {
        private readonly IConfiguration Configuration;

        public ValidateController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public SqlConnectionStringBuilder myConnection = new();

        private string CreateConnectionString()
        {
            myConnection.DataSource = Configuration["DatabaseOptions:ConnectionStrings:DataSource"];
            myConnection.UserID = Configuration["DatabaseOptions:ConnectionStrings:UserID"];
            myConnection.Password = Configuration["DatabaseOptions:ConnectionStrings:Password"];
            myConnection.InitialCatalog = Configuration["DatabaseOptions:ConnectionStrings:InitialCatalog"];
            myConnection["Encrypt"] = Configuration["Encrypt"];
            myConnection["TrustServerCertificate"] = Configuration["TrustServerCertificate"];
            return myConnection.ConnectionString;
        }

        public string CertDataSQL()
        {
            return Configuration["DatabaseOptions:Query"] + " AND " + Configuration["DatabaseOptions:ValidationField"] + " = @Credentials ORDER BY StartTime";
            //return Configuration["DatabaseOptions:Query"] + " AND " + Configuration["DatabaseOptions:ValidationField"] + " in (@Credentials, 'jerald.s.holtzclaw@exxonmobil.com', 'james_gallion@goodyear.com') ORDER BY StartTime";
        }

        public object ValidateCredentials(string userCredentials)
        {
            if (userCredentials == "" || userCredentials == null)
            {
                return "* No email was provided.";
            }
            else
            {
                try
                {
                    using (SqlConnection connection = new(CreateConnectionString()))
                    {
                        using (SqlCommand cmd = new(CertDataSQL(), connection))
                        {
                            cmd.Parameters.Add("@Credentials", SqlDbType.NVarChar).Value = userCredentials;

                            var da = new SqlDataAdapter(cmd);
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                return "* No data. - " + userCredentials;
                            }
                            else
                            {
                                //
                                // Return the whole data table and store in a session variable
                                //
                                HttpContext.Session.SetString("SessionVariable", JsonConvert.SerializeObject(dt).ToString());
                                //
                                // Return show specific values and store in session variables
                                //
                                HttpContext.Session.SetString("FullName", ((dt.Rows[0]["FirstName"] ?? "").ToString()! + " " + (dt.Rows[0]["LastName"] ?? "").ToString()!).Trim());

                                return "SUCCESS - " + dt.Rows.Count;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return "* - " + e.Message;
                }
            }
        }
    }
}