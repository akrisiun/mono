using System;
using System.Data;
using System.Data.SqlClient;
//using System.Management;
//using System.Reflection;

namespace Mono.Entity
{
    // Connection create and dispose

    public static class ConnectionPool
    {
        public static SqlConnection NewConn(string connectionString)
        {
            SqlConnection conn = null;
            //if (Context.Instance != null && (Context.Instance.IsDisposed || Context.Instance.SqlConnection != null))
            //{
            //    var instance = Context.Instance;
            //    if (instance.IsDisposed)
            //    {
            //        instance = Context.OpenWithConnStr(instance.ConnectionString());
            //        Context.Instance = instance;
            //    }
            //    else 
            //    if (instance.ConnectionString() == connectionString
            //        && !string.IsNullOrWhiteSpace(instance.SqlConnection.DataSource)
            //        && instance.SqlConnection.State != ConnectionState.Executing)
            //    {
            //        try
            //        {
            //            instance.UpdateSpid(null);
            //            conn = instance.SqlConnection;
            //        }
            //        catch { }

            //        if (conn != null)
            //            return conn;                // if no associated DataReader
            //    }
            //}

            conn = new SqlConnection(connectionString);
            if (!string.IsNullOrWhiteSpace(conn.DataSource))
                Context.SetLastConnString(connectionString);
            return conn;
        }

        public static void CloseConn(this IDbConnection connection, bool withPool = true, ILastError onError = null)
        {
            if (connection == null)
                return;
            try
            {
                connection.Dispose();
                if (withPool && connection is SqlConnection)
                    SqlConnection.ClearPool(connection as SqlConnection);
            }
            catch (Exception ex)
            {
                // DbObject PrePush internal exception 
                if (onError != null) onError.LastError = ex;
            }
        }
    }

    public static class SqlPoolInfo
    {
        // http://blah.winsmarts.com/2007-3-Determining_number_of_open_connections_in_your_connection_pools.aspx
    }
}
