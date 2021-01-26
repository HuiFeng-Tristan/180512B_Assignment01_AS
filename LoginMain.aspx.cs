using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace _180512B_Assignment01
{
    public partial class LoginMain : System.Web.UI.Page
    {

        string localDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["180512B_DB"].ConnectionString;
        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create
            (" https://www.google.com/recaptcha/api/siteverify?secret=6LcCBTsaAAAAAEx13HJ-xRWuWQKbpdG7DLNPhGTg &response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        //lb_score.Text = jsonResponse.ToString();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);

                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }

                return result;
            }
            catch (WebException e)
            {
                throw e;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["authenticated_user"] != null && Session["Access_Token"] != null && Request.Cookies["Access_Token"] != null)
            {
                if (Session["Access_Token"].ToString().Equals(Request.Cookies["Access_Token"].Value))
                {
                    Response.Redirect("UserProfile.aspx", false);
                }
            }


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

        }

       

        protected void btn_login_Click(object sender, EventArgs e)
        {
            //Response.Redirect("Registration.aspx");
            if (ValidateCaptcha())
            {
                string user_password = tb_password.Text.ToString().Trim();
                string user_loginID = tb_loginID.Text.ToString().Trim();
                SHA512Managed hashing = new SHA512Managed();
                

                try
                {
                    if (!isAccountLocked(user_loginID))
                    {
                        string dbHash = getDBHash(user_loginID);
                        string dbSalt = getDBSalt(user_loginID);

                        if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                        {
                            // check if acc is locked

                            string pwdWithSalt = user_password + dbSalt;
                            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                            string userHash = Convert.ToBase64String(hashWithSalt);
                            if (userHash.Equals(dbHash))
                            {
                                resetLoginTries(user_loginID);
                                Session["authenticated_user"] = tb_loginID.Text.Trim();
                                string myGUID = Guid.NewGuid().ToString();
                                Session["Access_Token"] = myGUID;

                                Response.Cookies.Add(new HttpCookie("Access_Token", myGUID));

                                Response.Redirect("UserProfile.aspx", false);
                            }
                            else
                            {
                                increaseLoginTries(user_loginID);
                                var val = new CustomValidator()
                                {
                                    ErrorMessage = "Username or Password is wrong, please try again.",
                                    Display = ValidatorDisplay.None,
                                    IsValid = false,
                                    ValidationGroup = "customValidator"
                                };
                                val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                                { args.IsValid = false; };
                                Page.Validators.Add(val);
                            }

                        }
                        else
                        {
                            var val = new CustomValidator()
                            {
                                ErrorMessage = "Database Error.",
                                Display = ValidatorDisplay.None,
                                IsValid = false,
                                ValidationGroup = "customValidator"
                            };
                            val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                            { args.IsValid = false; };
                            Page.Validators.Add(val);
                        }


                    }
                    else
                    {

                        var val = new CustomValidator()
                        {
                            ErrorMessage = "Account has been locked due to multiple invalid attempts. Please come back later",
                            Display = ValidatorDisplay.None,
                            IsValid = false,
                            ValidationGroup = "customValidator"
                        };
                        val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                        { args.IsValid = false; };
                        Page.Validators.Add(val);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
                finally { }
            }

        }

        // true - locked
        // false - not locked
        protected bool isAccountLocked(string loginID)
        {

            bool unlock_account = false;

            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Select Account_Unlocked_Date From USER_PROFILE Where Email = @paraEmail", db_connection))
                {
                    command.CommandType = CommandType.Text;
                    try
                    {
                        command.Parameters.AddWithValue("@paraEmail", loginID);
                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {


                            // null means not locked
                            if (reader["Account_Unlocked_Date"] == DBNull.Value)
                            {
                                return false;
                            }
                            else
                            {
                                // if account is locked

                                DateTime lockout_datetime;
                                DateTime.TryParse(reader["Account_Unlocked_Date"].ToString(), out lockout_datetime);

                                // if account locked time has elapsed
                                if (lockout_datetime < DateTime.Now)
                                {
                                    unlock_account = true;
                                }
                                else
                                {
                                    // if account still under locking time
                                    return true;
                                }
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());

                    }
                    finally { db_connection.Close(); };
                }
            }

            if (unlock_account == true)
            {
                resetLockout(loginID);
                return false;
            }
            else
            {
                throw new InvalidOperationException("The checking of account logout have error");
            }

        }

        // upon unlock of account
        protected void resetLockout(string loginID)
        {

            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Update USER_PROFILE SET Account_Unlocked_Date = @paraAccountUnlockedDate, Login_Tries = @paraLoginTries Where Email = @paraEmail", db_connection))
                {
                    try
                    {

                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", loginID);
                        command.Parameters.AddWithValue("@paraAccountUnlockedDate", DBNull.Value);
                        command.Parameters.AddWithValue("@paraLoginTries", 0);
                        db_connection.Open();
                        command.ExecuteNonQuery();

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

        // upon successful authentication
        protected void resetLoginTries(string loginID)
        {

            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Update USER_PROFILE SET Login_Tries = @paraLoginTries Where Email = @paraEmail", db_connection))
                {
                    try
                    {

                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", loginID);
                        command.Parameters.AddWithValue("@paraLoginTries", 0);
                        db_connection.Open();
                        command.ExecuteNonQuery();

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

        protected void increaseLoginTries(string loginID)
        {
            int user_login_tries = -1;

            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Select Login_Tries From USER_PROFILE Where Email = @paraEmail", db_connection))
                {
                    try
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", loginID);
                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            user_login_tries = Convert.ToInt32(reader["Login_Tries"]); ;
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

            if (user_login_tries != -1)
            {
                // perform 3 minutes block
                if (user_login_tries >= 2)
                {
                    using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                    {
                        using (SqlCommand command = new SqlCommand("Update USER_PROFILE SET Login_Tries = @paraLoginTries, Account_Unlocked_Date = @paraUnlockedDate Where Email = @paraEmail", db_connection))
                        {
                            try
                            {
                                command.CommandType = CommandType.Text;
                                command.Parameters.AddWithValue("@paraEmail", loginID);
                                command.Parameters.AddWithValue("@paraLoginTries", user_login_tries);
                                command.Parameters.AddWithValue("@paraUnlockedDate", DateTime.Now.AddMinutes(2));
                                db_connection.Open();
                                command.ExecuteNonQuery();

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
                else
                {
                    // increase user_login_tries by 1
                    using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                    {
                        using (SqlCommand command = new SqlCommand("Update USER_PROFILE SET Login_Tries = @paraLoginTries Where Email = @paraEmail", db_connection))
                        {
                            try
                            {
                                command.CommandType = CommandType.Text;
                                command.Parameters.AddWithValue("@paraEmail", loginID);
                                command.Parameters.AddWithValue("@paraLoginTries", user_login_tries + 1);
                                db_connection.Open();
                                command.ExecuteNonQuery();
                                db_connection.Close();

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
            }
            else
            {
                throw new Exception("ERROR CANNOT FIND USER UNDER INCREASELOGINTRIES");
            }


        }

        protected string getDBHash(string loginID)
        {
            string password_hash = null;
            SqlConnection connection = new SqlConnection(localDBConnectionString);

            string sql = "select Password_Hash FROM USER_PROFILE WHERE Email=@loginID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@loginID", loginID);

            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["Password_Hash"] != null)
                        {
                            if (reader["Password_Hash"] != DBNull.Value)
                            {
                                password_hash = reader["Password_Hash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return password_hash;
        }

        protected string getDBSalt(string loginID)
        {
            string password_salt = null;
            SqlConnection connection = new SqlConnection(localDBConnectionString);
            string sql = "select Password_Salt FROM USER_PROFILE WHERE Email=@loginID";
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@loginID", loginID);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Password_Salt"] != null)
                        {
                            if (reader["Password_Salt"] != DBNull.Value)
                            {
                                password_salt = reader["Password_Salt"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return password_salt;
        }

        protected void btn_register_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registration.aspx", false);
        }
    }
}