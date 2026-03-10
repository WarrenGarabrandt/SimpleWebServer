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
            // TOTPSecret is the shared secret used to generated TOTP codes in an app like google authenticator
            // LastLogin is the datetime of the last successful login
            // LastFailLogin is the datetime of the last failed login attempt
            // FailCount is the number of times a failed login happened since the last successful login.
            // LockedUntil is the datetime that the account will remained locked out until.
            // RecoveryToken is the nonce generated in a password reset email to verify identify when resetting a password
            @"CREATE TABLE User (UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, TOTPSecret TEXT, Enabled INTEGER NOT NULL, LastLogin TEXT, LastFailLogin TEXT, FailCount INTEGER NOT NULL, LockedUntil TEXT, RecoveryToken TEXT);",
                
            // Named roles that can be assigned to users
            @"CREATE TABLE Role (RoleID INTEGER PRIMARY KEY, DisplayName TEXT);",

            // Assignment of a role to a user
            @"CREATE TABLE UserRole (UserID INTEGER, RoleID INTEGER);",

            // index to prevent duplicate entries
            @"CREATE UNIQUE INDEX idx_UserRole ON System(UserID, RoleID);",

            // Named permissions that can be assigned to a role. 
            // Value is a string token to uniquely identify the permission so that a web page can quickly determine if the logged in user has a specific permission.
            @"CREATE TABLE Permission (PermissionID INTEGER PRIMARY KEY, DisplayName TEXT, Value TEXT);",

            // assignment of a permission to a role
            @"CREATE TABLE RolePermission (RoleID INTEGER, PermissionID INTEGER);",

            // index to prevent duplicate entries
            @"CREATE UNIQUE INDEX idx_RolePermission ON System(RoleID, PermissionID);",

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

        ////UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, TOTPSecret TEXT, Enabled INTEGER NOT NULL, LastLogin TEXT, LastFailLogin TEXT, FailCount INTEGER NOT NULL, LockedUntil TEXT, RecoveryToken TEXT
        public static string User_GetAll = @"SELECT UserID, DisplayName, Email, Salt, PassHash, TOTPSecret, Enabled, LastLogin, LastFailLogin, FailCount, LockedUntil, RecoveryToken FROM User;";
        public static string User_GetByEmail = @"SELECT UserID, DisplayName, Email, Salt, PassHash, TOTPSecret, Enabled, LastLogin, LastFailLogin, FailCount, LockedUntil, RecoveryToken FROM User WHERE Email = $Email COLLATE NOCASE;";
        public static string User_GetByID = @"SELECT UserID, DisplayName, Email, Salt, PassHash, TOTPSecret, Enabled, LastLogin, LastFailLogin, FailCount, LockedUntil, RecoveryToken FROM User WHERE UserID = $UserID;";
        public static string User_Insert = @"INSERT INTO User(DisplayName, Email, Salt, PassHash, TOTPSecret, Enabled, LastLogin, LastFailLogin, FailCount, LockedUntil, RecoveryToken) VALUES ($DisplayName, $Email, $Salt, $PassHash, $TOTPSecret, $Enabled, $LastLogin, $LastFailLogin, $FailCount, $LockedUntil, $RecoveryToken);";
        public static string User_Update = @"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, TOTPSecret = $TOTPSecret, Enabled = $Enabled, LastLogin = $LastLogin, LastFailLogin = $LastFailLogin, FailCount = $FailCount, LockedUntil = $LockedUntil, RecoveryToken = $RecoveryToken WHERE UserID = $UserID;";
        public static string User_DeleteByID = @"DELETE FROM UserRole WHERE UserID = $UserID; DELETE FROM User WHERE UserID = $UserID;";

        //RoleID INTEGER PRIMARY KEY, DisplayName TEXT
        public static string Role_GetAll = @"SELECT RoleID, DisplayName FROM Role;";
        public static string Role_GetByID = @"SELECT RoleID, DisplayName FROM Role WHERE RoleID = $RoleID;";
        public static string Role_Insert = @"INSERT INTO Role(DisplayName) VALUES ($DisplayName);";
        public static string Role_Update = @"UPDATE Role SET DisplayName = $DisplayName WHERE RoleID = $RoleID;";
        public static string Role_DeleteByID = @"DELETE FROM RolePermission WHERE RoleID = $RoleID; DELETE FROM UserRole WHERE RoleID = $RoleID; DELETE FROM Role WHERE RoleID = $RoleID;";

        public static string UserRole_Assign = @"REPLACE INTO UserRole(UserID, RoleID) VALUES ($UserID, $RoleID);";
        public static string UserRole_Remove = @"DELETE FROM UserRole WHERE UserID = $UserID AND RoleID = $RoleID;";

        //PermissionID INTEGER PRIMARY KEY, DisplayName TEXT, Value TEXT
        public static string Permission_GetAll = @"SELECT PermissionID, DisplayName, Value FROM Permission;";
        public static string Permission_GetByID = @"SELECT PermissionID, DisplayName, Value FROM Permission WHERE PermissionID = $PermissionID;";
        public static string Permission_Insert = @"INSERT INTO Permission(DisplayName, Value) VALUES ($DisplayName, $Value);";
        public static string Permission_Update = @"UPDATE Permission SET DisplayName = $DisplayName, Value = $Value WHERE PermissionID = $PermissionID;";
        public static string Permission_DeleteByID = @"DELETE FROM RolePermission WHERE PermissionID = $PermissionID; DELETE FROM Permission WHERE PermissionID = $PermissionID;";

        public static string RolePermission_Assign = @"REPLACE INTO RolePermission(RoleID, PermissionID) VALUES ($RoleID, $PermissionID);";
        public static string RolePermission_Remove = @"DELETE FROM RolePermission WHERE RoleID = $RoleID AND PermissionID = $PermissionID;";

        // Return 1 if there exists a role connecting ther UserID to the Permission Value, or a 0 if not.
        public static string User_CheckPermission = @"SELECT EXISTS (SELECT 1 FROM UserRole AS UR INNER JOIN RolePermission AS RP ON UR.RoleID = RP.RoleID INNER JOIN Permission AS P ON RP.PermissionID = P.PermissionID WHERE UR.UserID = $UserID AND P.Value = $Value);";

        //IPEndpointID INTEGER PRIMARY KEY, Address TEXT, Port INTEGER, Protocol TEXT, CertFriendlyName TEXT, Hostname TEXT
        public static string IPEndpoint_GetAll = @"SELECT IPEndpointID, Address, Port, Protocol, CertFriendlyName, Hostname FROM IPEndpoint;";
        public static string IPEndpoint_GetByID = @"SELECT IPEndpointID, Address, Port, Protocol, CertFriendlyName, Hostname FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndPoint_Insert = @"INSERT INTO IPEndpoint (Address, Port, Protocol, CertFriendlyName, Hostname) VALUES ($Address, $Port, $Protocol, $CertFriendlyName, $Hostname);";
        public static string IPEndpoint_Update = @"UPDATE IPEndpoint SET Address = $Address, Port = $Port, Protocol = $Protocol, CertFriendlyName = $CertFriendlyName, Hostname = $Hostname WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndpoint_DeleteByID = @"DELETE FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";

    }
}
