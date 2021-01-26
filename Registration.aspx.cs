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
    public partial class Registration : System.Web.UI.Page
    {

        string localDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["180512B_DB"].ConnectionString;
        static string finalHash;
        static string password_salt;
        byte[] Key;
        byte[] IV;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_register_Click(object sender, EventArgs e)
        {

            // if page pass all validation check
            if (Page.IsValid)
            {

                if (DateTime.ParseExact(tb_dob.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture) > DateTime.Now)
                {
                    var val = new CustomValidator()
                    {
                        ErrorMessage = "Invalid Date.",
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


                    // Validate unique email
                    if (!checkDuplicateEmail())
                    {

                        string user_password_input = tb_password.Text.Trim();

                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] saltByte = new byte[8];

                        rng.GetBytes(saltByte);
                        password_salt = Convert.ToBase64String(saltByte);

                        SHA512Managed hashing = new SHA512Managed();
                        string password_and_salt = user_password_input + password_salt;
                        //byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(user_password_input));
                        byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(password_and_salt));
                        finalHash = Convert.ToBase64String(hashWithSalt);
                        RijndaelManaged cipher = new RijndaelManaged();
                        cipher.GenerateKey();
                        //Key = cipher.Key;
                        //IV = cipher.IV;


                        // insert into database
                        createUserAccount();

                        Session["Message"] = "Successfully create your account. Please login";
                        // redirect to login
                        Response.Redirect("LoginMain.aspx");
                    }
                    else
                    {
                        var val = new CustomValidator()
                        {
                            ErrorMessage = "Email exist, try entering a new one.",
                            Display = ValidatorDisplay.None,
                            IsValid = false,
                            ValidationGroup = "customValidator"
                        };
                        val.ServerValidate += (object source, ServerValidateEventArgs args) =>
                        { args.IsValid = false; };
                        Page.Validators.Add(val);
                    }
                }

            }

        }

        public bool checkDuplicateEmail()
        {
            try
            {
                using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SELECT COUNT(*) from USER_PROFILE where Email = @paraEmail", db_connection))
                    {

                        command.Parameters.AddWithValue("@paraEmail", tb_email.Text.Trim());
                        db_connection.Open();
                        int userProfileExist = (int)command.ExecuteScalar();
                        db_connection.Close();

                        if (userProfileExist > 0)
                        {
                            // Email exist
                            return true;

                        }
                        else
                        {
                            // Email doesn't exist.
                            return false;
                        }

                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

        }

        public void createUserAccount()
        {
            try
            {
                using (SqlConnection db_connection = new SqlConnection(localDBConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO USER_PROFILE VALUES(@firstName, @lastName, @creditCard, @email, @passwordHash, " +
                        "@passwordSalt, @dob, @loginTries, @unlockDate, @IV, @key, @minimumAge, @maximumAge)"))
                    {

                        // user para to avoid SQL injection attack
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@firstName", tb_firstName.Text.Trim());
                        command.Parameters.AddWithValue("@lastName", tb_lastName.Text.Trim());
                        command.Parameters.AddWithValue("@creditCard", Convert.ToBase64String(encryptData(tb_creditCard.Text.Trim())));
                        command.Parameters.AddWithValue("@email", tb_email.Text.Trim().ToLower());
                        command.Parameters.AddWithValue("@passwordHash", finalHash);
                        command.Parameters.AddWithValue("@passwordSalt", password_salt);
                        command.Parameters.AddWithValue("@dob", DateTime.ParseExact(tb_dob.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture));
                        command.Parameters.AddWithValue("@loginTries", 0);
                        command.Parameters.AddWithValue("@unlockDate", DBNull.Value);
                        command.Parameters.AddWithValue("@IV", Convert.ToBase64String(IV));
                        command.Parameters.AddWithValue("@key", Convert.ToBase64String(Key));
                        command.Parameters.AddWithValue("@minimumAge", DateTime.Now.AddMinutes(5));
                        command.Parameters.AddWithValue("@maximumAge", DateTime.Now.AddMinutes(15));
                        command.Connection = db_connection;
                        db_connection.Open();
                        command.ExecuteNonQuery();
                        db_connection.Close();
                    }


                }
            }

            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.GenerateKey();
                Key = cipher.Key;
                IV = cipher.IV;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally { }
            return cipherText;
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

            else
            {
                args.IsValid = true;
            }
        }

        protected void btn_login_Click(object sender, EventArgs e)
        {
            Response.Redirect("LoginMain.aspx", false);
        }
    }
}