using System;
using System.Data;
using System.Data.SqlClient;

namespace Mono.Entity
{
    #region Mono VM0/BLS servers enums

    public enum Server
    {
        [EnumValue(@".\SQLEXPRESS")]
        [EnumCaption(@"ccwebuser@.\SQL")]
        Local = 0,

        [EnumValue(@"SNTXTX-VM0\SNTXSQL1")]
        [EnumCaption(@"produser@VM0")]
        VM0 = 1,  // produser

        [EnumValue(@"SNTXTX-VM0\SNTXSQL1")]
        [EnumCaption(@"ccusrweb@VM0")]
        VM0_ccUsrWeb = 2,   // webuser

        [EnumValue(@"BLSTX-SQL1.bls.lt")]
        Bls = 3,

        [EnumValue(@"SNTXTX-STAT3")]
        QStat3 = 4,

        [EnumValue(@".\SQLEXPRESS")]
        [EnumCaption(@"megabyte@.\SQL")]
        LocalMegabyte = 5,

        [EnumValue(@"SNTXTX-VM0\SNTXSQL1")]
        [EnumCaption("megabyte@VM0")]
        VM0Megabyte = 6,

        [EnumValue(@"192.168.2.172")]
        [EnumCaption("megasqlbyte@2.172")]
        TST_MegabytSql = 7,

        [EnumValue(@"SNTXTX-STAT3")]
        [EnumCaption("megabyte@STAT3")]
        STAT3_Megabyte = 8,

        [EnumValue(@"172.16.1.225")]    //SERVICEDB
        [EnumCaption("megabyte@172.16.1.225")]
        ODAY_ServiceDb = 9
    }

    public static class ServerDb
    {
        public const string SNTXDB = "SNTXDB";
        public const string SNTXECOM = "SNTXECOM";
        public const string SNTXCC = "SNTXCC";
        public const string SNTXDATA = "SNTXDATA";
        public const string SNTXWRK = "SNTXWRK";
    }

    public enum SntxDbName
    {
        [EnumValue(null)]
        Default = 0,

        [EnumValue(ServerDb.SNTXDB)]
        SNTXDB = 1,
        [EnumValue(ServerDb.SNTXECOM)]
        SNTXECOM = 2,
        [EnumValue(ServerDb.SNTXCC)]
        SNTXCC = 3,
        [EnumValue(ServerDb.SNTXDATA)]
        SNTXDATA = 4,
        [EnumValue(ServerDb.SNTXWRK)]
        SNTXWRK = 5
    }

    #endregion

    // Mono Sql server connection helper

    public static class ServerConn
    {
        public const string connKey_sntx = "sntxdb";
        public const string connKey_bls = "blsdb";

        public const Server DefaultServerEnum = Server.Local;
        public const string DefaultServer = @".\SQLEXPRESS"; // EnumValue.Get(Server.Local);


#if !NET45
        // none
#else
        #region Open connection

        public static Context Open(Server serverEnum, SntxDbName dbName = SntxDbName.SNTXDB)
        {
            string dbNameStr = EnumString<SntxDbName>(dbName) ?? ServerDb.SNTXDB;
            return Open(serverEnum, dbNameStr, true);
        }

        public static Context OpenCC(Server serverEnum)
        {
            return Open(serverEnum, ServerDb.SNTXCC, true);
        }

        public static Context OpenECOM(Server serverEnum)
        {
            return Open(serverEnum, ServerDb.SNTXECOM, true);
        }
        #endregion

        #region New Conn, not Open() (but OpenAsync)

        public static Context NewConn(Server serverEnum, SntxDbName dbName = SntxDbName.SNTXDB)
        {
            string dbNameStr = EnumString<SntxDbName>(dbName) ?? "";
            return Open(serverEnum, dbNameStr, openConnection: false);
        }

        public static Context NewConnCC(Server serverEnum)
        {
            string dbNameStr = ServerDb.SNTXDB;
            return Open(serverEnum, dbNameStr, openConnection: false);
        }

        public static Context NewConnECOM(Server serverEnum)
        {
            return Open(serverEnum, ServerDb.SNTXECOM, openConnection: false);
        }

        #endregion

        public static Context Open(Server serverEnum, string database, bool openConnection, int retryCount = 3)
        {
            string connStr = ConnectionString(serverEnum, database);

            Context instance = Context.NewWithConnStr(connStr, database);

            if (openConnection)
            {
                do
                {
                    if (instance.OpenConnection(serverEnum, database, openConnection)
                        && instance.SPID == null)
                        instance.UpdateSpid();
                    retryCount--;
                } while (instance.SPID == null && retryCount > 0);
            }

            if (instance.SPID == null && instance.LastError != null)
                throw instance.LastError;

            if (string.IsNullOrWhiteSpace(instance.DbName))
                instance.ChangeDatabase(instance.SqlConnection.Database);

            return instance;
        }
#endif

        public static Context OpenConnection(Server serverEnum) { return OpenContext(new Context(), serverEnum, null, false); }

        public static bool OpenConnection(this Context db, Server serverEnum, string database = null, bool openConnection = true)
        {
            db = OpenContext(db, serverEnum, database, openConnection);

            return db.Connection.State == ConnectionState.Open;
        }

        public static Context OpenContext(this Context db, Server serverEnum, string database, bool openConnection)
        {
            db = db ?? new Context();

            string serverName = EnumValue.Get(serverEnum);
            var conn = db.SqlConnection;
            if (conn == null || conn.State != ConnectionState.Open && string.IsNullOrEmpty(conn.ConnectionString))
            {
                var connectionString = ConnectionString(serverEnum, database);
                db.Connection = new SqlConnection(connectionString);
                conn = db.SqlConnection;

                Context.SetLastConnString(connectionString);
            }

            if (!conn.DataSource.Equals(serverName))
                throw new OperationCanceledException("connection server error");

            if (openConnection && conn.State != ConnectionState.Open)
            {
                if (db.OnBeforeOpen != null)
                    db.OnBeforeOpen(db, new SqlConnEventArgs(conn as SqlConnection));
                conn.Open();
            }

            // db.dbName = database;
            if (!string.IsNullOrWhiteSpace(database))
            {
                if (conn.State == ConnectionState.Open)
                    conn.ChangeDatabase(database);

                if (!conn.Database.Equals(database))
                    throw new System.OperationCanceledException("connection database server error");
            }

            Context.SetLastConnString(conn.ConnectionString);
            db.CommandTimeout = Context.defCommandTimeout;

            return db;
        }

        public static string ConnectionString(Server serverEnum, string database)
        {
            string serverName = EnumValue.Get(serverEnum);

            //Context Instance; // = Context.Instance;
            //if (Context.Instance != null)
            //    Context.Instance.ChangeDatabase(database); //  .dbName = database;

            Context.IntegratedSecurity = false;
            if (serverEnum == Server.LocalMegabyte)
            {
                Context.UserID = null; Context.Password = null; Context.IntegratedSecurity = true;
                return new SqlConnectionStringBuilder()
                {
                    DataSource = serverName,
                    InitialCatalog = Context.InitialCatalog, // Instance == null ? database : Instance.DbName,
                    PersistSecurityInfo = true,
                    IntegratedSecurity = Context.IntegratedSecurity.Value,
                    // UserID = UserID, Password = Password,
                    ConnectTimeout = 5,
                    MultipleActiveResultSets = true
                }.ConnectionString
                    + ";Trusted_Connection=true;";
            }
            else if (serverEnum == Server.VM0Megabyte || serverEnum == Server.STAT3_Megabyte || serverEnum == Server.ODAY_ServiceDb)
            {
                Context.UserID = "megabyt" + "sql";
                Context.Password = "mega" + "byte";
            }
            else if (serverEnum == Server.TST_MegabytSql)
            {
                Context.UserID = "megabyt" + "sql";
                Context.Password = "mega" + "bytesql";
            }
            else if (serverEnum == Server.VM0_ccUsrWeb)
            {
                Context.UserID = "ccusrweb";
                Context.Password = "ccwebusr";
            }
            else
            {
                Context.UserID = Context.DefaultUser;
                Context.Password = Context.DefaultPass;
            }

            return new SqlConnectionStringBuilder()
            {
                DataSource = serverName,
                InitialCatalog = Context.InitialCatalog, // Instance == null ? database : Context.Instance.DbName,
                PersistSecurityInfo = true,
                IntegratedSecurity = Context.IntegratedSecurity ?? Context.DefaultIntegratedSecurity,
                UserID = Context.UserID ?? Context.DefaultUser,
                Password = Context.Password ?? Context.DefaultPass,
                ConnectTimeout = 5,
                MultipleActiveResultSets = true
            }.ConnectionString
            + ";Trusted_Connection=false;";
        }

        public static string EnumString<T>(object value)
        {
            return Enum.GetName(typeof(T), value) as string;
        }

    }

}
