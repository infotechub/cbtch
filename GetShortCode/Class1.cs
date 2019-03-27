using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GetShortCode
{
    public class Class1
    {

        public struct Response
        {
            public int Id;
            public string Phonenumber;
            public string Message;
        }

        public IList<Response> Collect()
        {
            var connectionstring =
                "server=novohealthafrica.org;uid=novoheal_dms;pwd=P@ssw0rd1234;database=novoheal_DMS;";
            IList<Response> output = new List<Response>();
            NewMethod(connectionstring, output);

            return output;
        }

        private static void NewMethod(string connectionstring, IList<Response> output)
        {
            try
            {
                var conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = connectionstring;
                conn.Open();

                using (
                    MySqlCommand cmd =
                        new MySqlCommand("SELECT id,phonenumber,message FROM novoheal_DMS.Messages where status='1';",
                                         conn))
                {
                    //cmd.Parameters.AddWithValue("@pname", x);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var phonenumber = reader.GetString(1);
                            var message = reader.GetString(2);

                            var item = new Response()
                            {
                                Id = id,
                                Phonenumber = phonenumber,
                                Message = message
                            };


                            output.Add(item);

                        }



                    }
                }
            }
            catch (MySqlException ex)
            {

            }
        }

        public bool Read(int id)
        {
            var connectionstring =
                "server=novohealthafrica.org;uid=novoheal_dms;pwd=P@ssw0rd1234;database=novoheal_DMS;";

            try
            {
                using (var cn = new MySqlConnection(connectionstring))
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = cn;
                    cmd.CommandText = "UPDATE novoheal_DMS.Messages SET status = 0 WHERE id =" + id.ToString();
                    cn.Open();
                    int numRowsUpdated = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (numRowsUpdated > 0)
                    {
                        return true;
                    }
                    return false;
                }

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                return false;
            }
        }
    }

}
