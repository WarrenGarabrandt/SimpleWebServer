using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.SQLite
{
    public class SQLiteStrings
    {
        private const string COMPATIBLE_DATABASE_VERSION = "1.7";
        public static string[] Format_Database = new string[]
        {
            // Contains configuration and version data.
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);",

            // create unique constraint so that REPLACE INTO will function properly.
            @"CREATE UNIQUE INDEX idx_System_CategorySetting ON System(Category, Setting);",

            // User table
            @"CREATE TABLE User (UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, Enabled INTEGER NOT NULL);",

            // IP Endpoint to listen for incoming connections.
            // Address: IP address to listen on
            // Port: Port number to bind to, such as 80, 443, etc.
            // Protocol: list the Protocol that this endpoint will allow. Other protocols may be implemented in the future.
            //          HTTP/1.1 = unencrypted HTTP traffic, HTTP version 1.1
            //          HTTPS/1.1 = encrypted HTTPS traffic, HTTP version 1.1. Must specify CertFriendlyName
            // CertFriendlyName: name of a certificate installed on the local machine to use for encrypting HTTPS connections
            // Hostname: web hostname that the client must specify for TLS to work correctly
            @"CREATE TABLE IPEndpoint (IPEndpointID INTEGER PRIMARY KEY, Address TEXT, Port INTEGER, Protocol TEXT, CertFriendlyName TEXT, Hostname TEXT);",
                        

        };

        public static List<Tuple<string, string, string>> DatabaseDefaults = new List<Tuple<string, string, string>>()
        {
            // current database version
            new Tuple<string, string, string>("System", "Version", COMPATIBLE_DATABASE_VERSION),

        };

        public static string Table_LastRowID = @"SELECT last_insert_rowid();";

        public static string System_GetAll = @"SELECT Category, Setting, Value FROM System ORDER BY Category ASC, Setting ASC;";
        public static string System_Select = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Insert = @"REPLACE INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";

        public static string User_GetAll = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled FROM User;";
        public static string User_GetByEmail = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled FROM User WHERE Email = $Email COLLATE NOCASE;";
        public static string User_GetByID = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled FROM User WHERE UserID = $UserID;";
        public static string User_Insert = @"INSERT INTO User(DisplayName, Email, Salt, PassHash, Enabled) VALUES ($DisplayName, $Email, $Salt, $PassHash, $Enabled);";
        public static string User_Update = @"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, Enabled = $Enabled WHERE UserID = $UserID;";
        public static string User_DeleteByID = @"DELETE FROM User WHERE UserID = $UserID;";

        public static string IPEndpoint_GetAll = @"SELECT IPEndpointID, Address, Port, Protocol, CertFriendlyName, Hostname FROM IPEndpoint;";
        public static string IPEndpoint_GetByID = @"SELECT IPEndpointID, Address, Port, Protocol, CertFriendlyName, Hostname FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";

    }
}
