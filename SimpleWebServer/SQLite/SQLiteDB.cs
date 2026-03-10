using SimpleWebServer.Models;
using SimpleWebServer.Models.DB;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.SQLite
{
    public class SQLiteDB : IDisposable
    {
        protected const string COMPATIBLE_DATABASE_VERSION = "1.7";
        public bool ConnectionInitialized = false;
        protected string DBPathOverride = null;
        protected SQLiteConnection SQLConnection = null;

        protected string DatabasePath
        {
            get
            {
                string progdata;
                if (string.IsNullOrEmpty(DBPathOverride))
                {
                    progdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    progdata = System.IO.Path.Combine(progdata, "SimpleWebServer");
                }
                else
                {
                    progdata = System.IO.Path.GetDirectoryName(DBPathOverride);
                }
                return progdata;
            }
        }

        protected string DatabaseFile
        {
            get
            {
                string filePath;
                if (string.IsNullOrEmpty(DBPathOverride))
                {
                    filePath = System.IO.Path.Combine(DatabasePath, "config.db");
                }
                else
                {
                    filePath = DBPathOverride;
                }
                return filePath;
            }
        }

        protected string _cached_DatabaseConnectionString = null;

        protected string DatabaseConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_cached_DatabaseConnectionString))
                {
                    _cached_DatabaseConnectionString = string.Format(string.Format("Data Source={0}", DatabaseFile));
                }
                return _cached_DatabaseConnectionString;
            }
        }


        #region Public Methods
        /// <summary>
        /// Generates and sets the salt and password hash on a user for a given password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"></param>
        public void GeneratePasswordHash(tblUser user, string newPassword)
        {
            user.Salt = GenerateNonce(16);
            string Password = string.Format("{0}:{1}", user.Salt, newPassword);
            byte[] passbytes = UTF8Encoding.UTF8.GetBytes(Password);
            using (SHA256 sha = SHA256.Create())
            {
                passbytes = sha.ComputeHash(passbytes);
            }
            user.PassHash = Convert.ToBase64String(passbytes);
        }

        /// <summary>
        /// Compute a password hash for a provided password and verify that it matches the expected value
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidatePasswordHash(tblUser user, string password)
        {
            string Password = string.Format("{0}:{1}", user.Salt, password);
            byte[] passbytes = UTF8Encoding.UTF8.GetBytes(Password);
            using (SHA256 sha = SHA256.Create())
            {
                passbytes = sha.ComputeHash(passbytes);
            }
            if (user.PassHash == Convert.ToBase64String(passbytes))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a random string of letters and numbers.
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public string GenerateNonce(int len)
        {
            Random rnd = new Random();
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets up the connection to the database. Will create a new database if one doesn't exist already.
        /// </summary>
        /// <returns></returns>
        public bool InitDatabase(string dbPath = null)
        {
            if (SQLConnection != null)
            {
                try
                {
                    SQLConnection.Dispose();
                }
                catch { }
                SQLConnection = null;
            }
            ConnectionInitialized = false;
            DBPathOverride = dbPath;
            _cached_DatabaseConnectionString = null;

            if (!System.IO.File.Exists(DatabaseFile))
            {
                try
                {
                    _formatNewDatabase();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error formattting database: {0}", ex.Message));
                }
            }
            SQLConnection = new SQLiteConnection(DatabaseConnectionString);
            SQLConnection.Open();
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("$Category", "System"));
            parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
            string value = _runValueQuery(SQLiteStrings.System_Select, parms);
            if (value != COMPATIBLE_DATABASE_VERSION)
            {
                throw new IncompatibleDatabaseVersionException("Incompatible database version.");
            }
            ConnectionInitialized = true;
            return true;

        }

        public void Dispose()
        {
            ConnectionInitialized = false;
            if (SQLConnection != null)
            {
                try
                {
                    SQLConnection.Close();
                    SQLConnection.Dispose();
                }
                catch { }
                finally
                {
                    SQLConnection = null;
                }
            }
        }
        #endregion

        #region Private Methods

        protected void _formatNewDatabase()
        {
            if (!System.IO.Directory.Exists(DatabasePath))
            {
                System.IO.Directory.CreateDirectory(DatabasePath);
            }
            SQLiteConnection.CreateFile(DatabaseFile);
            var parms = new List<KeyValuePair<string, string>>();
            SQLConnection = new SQLiteConnection(DatabaseConnectionString);
            SQLConnection.Open();
            // create all tables
            foreach (string cmdstr in SQLiteStrings.Format_Database)
            {
                _runNonQuery(cmdstr, parms);
            }

            // create all default values
            foreach (var setting in SQLiteStrings.DatabaseDefaults)
            {
                parms.Add(new KeyValuePair<string, string>("$Category", setting.Item1));
                parms.Add(new KeyValuePair<string, string>("$Setting", setting.Item2));
                parms.Add(new KeyValuePair<string, string>("$Value", setting.Item3));
                _runNonQuery(SQLiteStrings.System_Insert, parms);
            }

            //// Create the admin user
            //tblUser newAdminUser = new tblUser("Administrator", "admin@local", "", "", true);
            //GeneratePasswordHash(newAdminUser, "password");
            //qrySetUser q = new qrySetUser(newAdminUser);
            //_user_AddUpdate(s, q);

            // Create a default IP Endpoint
            //tblIPEndpoint newEndpoint = new tblIPEndpoint("0.0.0.0", 80, tblIPEndpoint.IPEndpointProtocols.ESMTP, tblIPEndpoint.IPEndpointTLSModes.Disabled, "smtprelay.local", "", false);
            //qrySetIPEndpoint newepq = new qrySetIPEndpoint(newEndpoint);
            //_ipendpoint_AddUpdate(s, newepq);
            //SQLConnection.Close();
        }

        protected string _runValueQuery(string query, List<KeyValuePair<string, string>> parms)
        {
            string result = null;
            using (var command = SQLConnection.CreateCommand())
            {
                command.CommandText = query;
                foreach (var kv in parms)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value);
                }
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetString(0);
                    }
                }
            }
            return result;
        }

        protected void _runNonQuery(string query, List<KeyValuePair<string, string>> parms)
        {
            using (var command = SQLConnection.CreateCommand())
            {
                command.CommandText = query;
                foreach (var kv in parms)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value);
                }
                command.ExecuteNonQuery();
            }
        }



        #endregion

    }
}
