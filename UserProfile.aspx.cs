using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Data;

namespace _180512B_Assignment01
{
    public partial class UserProfile : System.Web.UI.Page
    {
        string localDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["180512B_DB"].ConnectionString;

        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["authenticated_user"] != null && Session["Access_Token"] != null && Request.Cookies["Access_Token"] != null)
            {
                if (Session["Access_Token"].ToString().Equals(Request.Cookies["Access_Token"].Value))
                {

                    if (Session["Message"] != null)
                    {

                        lb_smessage.Text = Session["Message"].ToString();
                        Session["Message"] = null;
                        Session.Remove("Message");

                    }
                    else
                    {
                        lb_smessage.Visible = false;
                    }

                    tb_firstName.Attributes["disabled"] = "disabled";
                    tb_lastName.Attributes["disabled"] = "disabled";
                    tb_dob.Attributes["disabled"] = "disabled";
                    tb_creditCard.Attributes["disabled"] = "disabled";
                    tb_email.Attributes["disabled"] = "disabled";

                    // retrieve info
                    retrieveUserInfo(Session["authenticated_user"].ToString().Trim());

                    checkPasswordMaximumAge();
                }
                else
                {
                    Response.Redirect("LoginMain.aspx", true);
                }
            }
            else
            {
                Response.Redirect("LoginMain.aspx", true);
            }
        }

         protected void checkPasswordMaximumAge()
        {
            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Select Maximum_Password_Age From USER_PROFILE Where Email = @paraEmail", db_connection))
                {
                    try
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", Session["authenticated_user"].ToString());
                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            DateTime maximumPasswordAge;
                            DateTime.TryParse(reader["Maximum_Password_Age"].ToString(), out maximumPasswordAge);

                            if (maximumPasswordAge < DateTime.Now)
                            {
                                Session["Message"] = "Your password have expired, please change to proceed";
                                Response.Redirect("PasswordChange.aspx", false);
                            }

                        }


                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                    finally
                    {
                        db_connection.Close();
                    }
                }
            }
        }

        private void retrieveUserInfo(string email)
        {
            try
            {
                using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SELECT * from USER_PROFILE where Email = @paraEmail", db_connection))
                    {

                        command.Parameters.AddWithValue("@paraEmail", email);
                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();



                        if (reader.Read())
                        {
                            // Record exist
                            IV = Convert.FromBase64String(reader["IV"].ToString());
                            Key = Convert.FromBase64String(reader["Key"].ToString());

                            
                            tb_firstName.Text = HttpUtility.HtmlEncode(reader["Last_Name"].ToString());
                            tb_lastName.Text = HttpUtility.HtmlEncode(reader["First_name"].ToString());
                            tb_dob.Text = Convert.ToDateTime(reader["DOB"]).ToString("dd/MM/yyyy");
                            tb_email.Text = HttpUtility.HtmlEncode(reader["Email"].ToString());
                            tb_creditCard.Text = HttpUtility.HtmlEncode(decryptData(Convert.FromBase64String(reader["Credit_Card"].ToString())));


                        }
                        else
                        {
                            // Record doesn't exist.
                            Session.Clear();
                            Session.Abandon();
                            Session.RemoveAll();

                            Response.Redirect("LoginMain.aspx", true);

                            if (Request.Cookies["ASP.NET_SessionId"] != null)
                            {
                                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
                            }

                            if (Request.Cookies["Access_Token"] != null)
                            {
                                Response.Cookies["Access_Token"].Value = string.Empty;
                                Response.Cookies["Access_Token"].Expires = DateTime.Now.AddMonths(-20);
                            }
                        }

                        db_connection.Close();

                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        protected string decryptData(byte[] cipherText)
        {
            string plainText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptTransform = cipher.CreateDecryptor();

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDescrypt = new CryptoStream(msDecrypt, decryptTransform, CryptoStreamMode.Read))
                    {
                        using(StreamReader seDescrypt = new StreamReader(csDescrypt))
                        {
                            plainText = seDescrypt.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return plainText;
        }

        protected void btn_logout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            Response.Redirect("LoginMain.aspx", false);

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            if (Request.Cookies["Access_Token"] != null)
            {
                Response.Cookies["Access_Token"].Value = string.Empty;
                Response.Cookies["Access_Token"].Expires = DateTime.Now.AddMonths(-20);
            }
        }

        protected void btn_changePassword_Click(object sender, EventArgs e)
        {
            Response.Redirect("PasswordChange.aspx");
        }
    }
}