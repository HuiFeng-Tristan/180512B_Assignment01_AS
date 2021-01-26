using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace _180512B_Assignment01
{
    public partial class PasswordChange : System.Web.UI.Page
    {
        string localDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["180512B_DB"].ConnectionString;
        string user_email = null;
        string old_password_hash = null;
        string old_password_salt = null;
        int user_profile_ID = -1;
        string new_password_hash = null;
        string new_salt = null;


        protected void Page_Load(object sender, EventArgs e)
        {
           
            if (Session["authenticated_user"] != null && Session["Access_Token"] != null && Request.Cookies["Access_Token"] != null)
            {
                if (Session["Access_Token"].ToString().Equals(Request.Cookies["Access_Token"].Value))
                {
                    user_email = Session["authenticated_user"].ToString().Trim();
                    if (!violateMinimumPasswordAge())
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
                    }
                }
                else
                {
                    Response.Redirect("LoginMain.aspx", false);
                }
            }
            else
            {
                Response.Redirect("LoginMain.aspx", false);
            }



        }

        protected bool violateMinimumPasswordAge()
        {
            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Select Minimum_Password_Age From USER_PROFILE Where Email = @paraEmail", db_connection))
                {
                    try
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", Session["authenticated_user"].ToString());
                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            DateTime minimumPasswordAge;
                            DateTime.TryParse(reader["Minimum_Password_Age"].ToString(), out minimumPasswordAge);

                            if (minimumPasswordAge > DateTime.Now)
                            {
                                Session["Message"] = "You have to wait 5 minutes before chaining password again";
                                Response.Redirect("UserProfile.aspx", false);
                                return true;
                            }
                            else
                            {
                                return false;
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
            return true;
        }

        protected void cv_password_ServerValidate(object source, ServerValidateEventArgs args)
        {

            if (Regex.IsMatch(args.Value, @"[^\S*$]"))
            {

                args.IsValid = false;
                cv_password.ErrorMessage = "Password cannot contain space";
            }
            else if (args.Value.Length < 8)
            {
                args.IsValid = false;
                cv_password.ErrorMessage = "Password must be at least 8 characters";
            }
            else if (!Regex.IsMatch(args.Value, "[0-9]"))
            {
                args.IsValid = false;
                cv_password.ErrorMessage = "Password must have at least one number";
            }
            else if (!Regex.IsMatch(args.Value, "[a-z]"))
            {

                args.IsValid = false;
                cv_password.ErrorMessage = "Password must have at least one lowercase";
            }
            else if (!Regex.IsMatch(args.Value, "[A-Z]"))
            {

                args.IsValid = false;
                cv_password.ErrorMessage = "Password must have at leastone uppercase";
            }
            else if (!Regex.IsMatch(args.Value, "[^a-zA-Z0-9]"))
            {

                args.IsValid = false;
                cv_password.ErrorMessage = "Password must have at least one special character";
            }
            else if (args.Value != tb_cPassword.Text)
            {

                args.IsValid = false;
                cv_password.ErrorMessage = "Both password field must be the same";
            }
            else
            {
                args.IsValid = true;
            }
        }

        protected void btn_changePassword_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                RetrieveOldPass();
                if (old_password_hash == null || old_password_salt == null)
                {
                    var val = new CustomValidator()
                    {
                        ErrorMessage = "Database error.",
                        Display = ValidatorDisplay.None,
                        IsValid = false,
                        ValidationGroup = "customValidator"
                    };
                    val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                    { args.IsValid = false; };
                    Page.Validators.Add(val);
                }

                string user_password_input = tb_password.Text.Trim();

                SHA512Managed hashing = new SHA512Managed();
                string new_password_old_salt = user_password_input + old_password_salt;
                byte[] passWithPreviousSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(new_password_old_salt));

                string new_password_old_salt_hash = Convert.ToBase64String(passWithPreviousSalt);

                if (new_password_old_salt_hash == old_password_hash)
                {
                    var val = new CustomValidator()
                    {
                        ErrorMessage = "Password cannot be the same as previous one.",
                        Display = ValidatorDisplay.None,
                        IsValid = false,
                        ValidationGroup = "customValidator"
                    };
                    val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                    { args.IsValid = false; };
                    Page.Validators.Add(val);
                }
                else
                {
                    if (violatePasswordHistory())
                    {
                        var val = new CustomValidator()
                        {
                            ErrorMessage = "Password cannot be the same as the ones you use previosuly up to 3 times.",
                            Display = ValidatorDisplay.None,
                            IsValid = false,
                            ValidationGroup = "customValidator"
                        };
                        val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                        { args.IsValid = false; };
                        Page.Validators.Add(val);
                    }
                    else
                    {
                        InsertOldPassword();

                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] saltByte = new byte[8];

                        rng.GetBytes(saltByte);
                        // new salt 
                        new_salt = Convert.ToBase64String(saltByte);
                        // password + salt
                        string new_password_and_salt = user_password_input + new_salt;
                        // hashed password + salt
                        byte[] newPassHashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(new_password_and_salt));

                        new_password_hash = Convert.ToBase64String(newPassHashWithSalt);

                        changeToNewPass();

                        Session["Message"] = "Password has been changed";
                        Response.Redirect("UserProfile.aspx", false);
                    }
                   
                }

            }


        }

        protected bool violatePasswordHistory()
        {
            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Select Password_Hash, Password_Salt from HPASS where User_Profile_ID = @paraID", db_connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@paraID", user_profile_ID);
                    try
                    {
                        db_connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            string user_password_input = tb_password.Text.Trim();
                            SHA512Managed hashing = new SHA512Managed();

                            while (reader.Read())
                            {
                                string tempHash = reader["Password_Hash"].ToString();
                                string tempSalt = reader["Password_Salt"].ToString();

                                string tempCurrentPasswordWithSalt = user_password_input + tempSalt;
                                byte[] tempPasswordWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(tempCurrentPasswordWithSalt));

                                string new_password_old_salt_hash = Convert.ToBase64String(tempPasswordWithSalt);

                                if (new_password_old_salt_hash == tempHash)
                                {
                                    return true;
                                }

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                    finally { db_connection.Close(); }
                    return false;
                }
            }
        }

        protected void changeToNewPass()
        {
            using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
            {
                using (SqlCommand command = new SqlCommand("Update USER_PROFILE SET Password_Hash = @paraPasswordHash, Password_Salt = @paraPasswordSalt, Minimum_Password_Age = @minPassAge, " +
                    "Maximum_Password_Age = @maxPassAge where Email=@paraEmail", db_connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@paraPasswordHash", new_password_hash);
                    command.Parameters.AddWithValue("@paraPasswordSalt", new_salt);
                    command.Parameters.AddWithValue("@minPassAge", DateTime.Now.AddMinutes(5));
                    command.Parameters.AddWithValue("@maxPassAge", DateTime.Now.AddMinutes(15));
                    command.Parameters.AddWithValue("@paraEmail", Session["authenticated_user"].ToString().Trim());
                    db_connection.Open();
                    command.ExecuteNonQuery();
                    db_connection.Close();
                }
            }
        }

        protected void InsertOldPassword()
        {
            try
            {
                using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("Insert into HPASS VALUES(@passHash, @passSalt, @datetime, @userID)", db_connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@passHash", old_password_hash);
                        command.Parameters.AddWithValue("@passSalt", old_password_salt);
                        command.Parameters.AddWithValue("@datetime", DateTime.Now);
                        command.Parameters.AddWithValue("@userID", user_profile_ID);
                        db_connection.Open();
                        command.ExecuteNonQuery();
                        db_connection.Close();
                    }
                    int numPass = -1;
                    // number of past password we keep
                    using (SqlCommand command = new SqlCommand("Select Count(*) from HPASS where User_Profile_ID = @paraID", db_connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraID", user_profile_ID);
                        db_connection.Open();
                        numPass = (int)command.ExecuteScalar();
                        db_connection.Close();

                    }
                    // delete if record exceed 3 as we dont need it

                    if (numPass > 2)
                    {
                        using (SqlCommand command = new SqlCommand("delete from HPASS where Pass_ID in (select top 1 Pass_ID from HPASS where User_Profile_ID = @paraID order by Change_DateTime asc);", db_connection))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@paraID", user_profile_ID);
                            db_connection.Open();
                            command.ExecuteNonQuery();
                            db_connection.Close();

                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        protected void RetrieveOldPass()
        {
            try
            {
                using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("Select User_Profile_ID, Password_Hash, Password_Salt From USER_PROFILE Where Email = @paraEmail", db_connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@paraEmail", user_email);


                        db_connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            // Record exist 

                            user_profile_ID = Convert.ToInt32(reader["User_Profile_ID"]);
                            old_password_hash = reader["Password_Hash"].ToString();
                            old_password_salt = reader["Password_Salt"].ToString();

                        }
                        else
                        {
                            // Record doesn't exist.
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

                        db_connection.Close();

                    }

                }
            }

            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}