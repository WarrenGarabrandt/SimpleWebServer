using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.Models.DB
{
    public class tblUser
    {

        /// <summary>
        /// Creates a new instance that has not been saved in the database.
        /// </summary>
        public tblUser(string displayName, string email, string salt, string passHash, bool enabled)
        {
            UserID = null;
            DisplayName = displayName;
            Email = email;
            Salt = salt;
            PassHash = passHash;
            Enabled = enabled;

        }

        /// <summary>
        /// Creates a new instance that exists in the database.
        /// </summary>
        public tblUser(long userID, string displayName, string email, string salt, string passHash, int enabled)
        {
            UserID = userID;
            DisplayName = displayName;
            Email = email;
            Salt = salt;
            PassHash = passHash;
            EnabledInt = enabled;
        }

        public override string ToString()
        {
            return string.Format("{0} <{1}>", DisplayName, Email);
        }

        /// <summary>
        /// Generated when record is inserted. 
        /// On Save: Null = INSERT, NotNull = UPDATE
        /// </summary>
        public long? UserID { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string Salt { get; set; }

        public string PassHash { get; set; }

        public bool Enabled { get; set; }
        public int EnabledInt
        {
            get
            {
                return Enabled ? 1 : 0;
            }
            set
            {
                Enabled = value > 0;
            }
        }


    }
}
