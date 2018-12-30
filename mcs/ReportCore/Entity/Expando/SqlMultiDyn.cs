using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using Mono;
using Mono.Reflection;
using System.Xml.Linq;
using System.Data.Common;

namespace Mono.Entity
{
    public static class SqlMultiDyn
    {
        public static T ExecDynFirstVal<T>(ISqlProc proc, string key = "") where T : class
        {
            T result = default(T);
            SqlDataReader readerGet = null;
            DbEnumeratorData<ExpandoObject> numerator
                = SqlMultiDyn.ResultDyn(proc, (reader) => readerGet = reader);

            if (!numerator.Prepare())
                // {"There is already an open DataReader associated with this Command which must be closed first."}
                return result;

            Guard.Check(!readerGet.IsClosed);
            // var err = procText.LastError;

            var data = numerator.First;

            if (typeof(T).Equals(typeof(XElement)))
            {
                if (key == "")
                    key = "xml";
                string dataStr = ExpandoUtils.ValObj<string>(data, key);
                var doc = XDocument.Parse(dataStr);
                result = (T)(doc.Root as object);
            }
            else
                result = ExpandoUtils.ValObj<T>(data, key);

            return result;
        }

        public static DbEnumeratorData<ExpandoObject> ExecDynCmd(this Context db, SqlCommand execCmd
                    , Action<SqlCommand> setupCmd = null
                    , Action<Exception> onError = null)
        {
            Guard.CheckNotNull(execCmd, "execCmd");

            // var proc = new SqlProcText
            var proc = new SqlCmdProc
            {
                Cmd = execCmd,
                Context = db,
            };

            //if (execCmd.Parameters.Count > 0)
            //    proc.Param = new List<SqlParameter>(
            //        Mono.XLinq.Xnumerable.XSelect<SqlParameter>(
            //            execCmd.Parameters as System.Collections.IEnumerable, 
            //            (el) => (el as ICloneable).Clone() as SqlParameter)
            //        );

            SqlDataReader readerGet = null;
            Func<Tuple<DbDataReader, IDbConnection>> lazyReader
                = LazyReader(proc, (reader) => readerGet = reader, setupCmd, onError);

            var numeratorObj = new DbEnumeratorData<ExpandoObject>(lazyReader);
            if (numeratorObj == null)
                return DbEnumeratorData<ExpandoObject>.Empty;

            if (numeratorObj.LastError == null && proc.LastError != null)
                numeratorObj.LastError = proc.LastError;

            if (!numeratorObj.ReaderAvailable)
            {
                numeratorObj.LastError = null;
                numeratorObj.Reset();

                if (numeratorObj.Reader == null
                    && numeratorObj.LastError == null && proc.LastError != null)
                    numeratorObj.LastError = proc.LastError;
            }

            return numeratorObj;
        }

        public static DbEnumeratorData<ExpandoObject> ResultDynPrepared(this ISqlProc proc
            , Action<SqlDataReader> readerGet, Action<SqlCommand> setupCmd = null
            , Action<Exception> onError = null)
        {
            var dynNumerator = new DbEnumeratorData<ExpandoObject>(LazyReader(proc, readerGet, setupCmd, onError));
            dynNumerator.Prepare();
            return dynNumerator;
        }

        // public static IEnumerator<ExpandoObject> 
        public static DbEnumeratorData<ExpandoObject> ResultDyn(this ISqlProc proc
            , Action<SqlDataReader> readerGet, Action<SqlCommand> setupCmd = null
            , Action<Exception> onError = null)
        {
            // Func<SqlDataReader> 
            Func<Tuple<DbDataReader, IDbConnection>>
                lazyReader = LazyReader(proc, readerGet, setupCmd, onError);
            var dynNumerator = new DbEnumeratorData<ExpandoObject>(lazyReader);
            return dynNumerator; // as IEnumerator<ExpandoObject>;
        }

        // public static Func<SqlDataReader> LazyReader(this ISqlProc proc
        public static Func<Tuple<DbDataReader, IDbConnection>> LazyReader(this ISqlProc proc
            , Action<SqlDataReader> readerGet
            , Action<SqlCommand> setupCmd = null
            , Action<Exception> onError = null)
        {
            return new Func<Tuple<DbDataReader, IDbConnection>>(() =>
                {
                    SqlDataReader reader = null;
                    string errorStr = null;
                    bool retry = false;
                    try
                    {
                        reader = ExecMultiReader(proc, setupCmd, onError: null, progress: null);
                    }
                    catch (SqlException exSql)
                    {
                        // Human read short error message:
                        errorStr = String.Format("error in sql {0}, line {1} : {2}"
                                    , exSql.Procedure ?? (proc.CmdText ?? "???"), exSql.LineNumber, exSql.Message);
                        ErrorCase(errorStr, exSql, onError, proc);
                    }
                    catch (Exception ex)
                    {
                        errorStr = String.Format("error in sql {0}: {1}", proc.CmdText ?? "???", ex.Message);

                        if (errorStr.Contains(strAgain))
                            retry = true;
                        else
                            ErrorCase(errorStr, ex, onError, proc);
                    }

                    if (retry)
                    {
                        // already an open DataReader associated";
                        // ValidateConnectionForExecute
                        try
                        {
                            reader = ExecMultiReader(proc
                                , setup: (cmd) => cmd.Connection
                                        = ConnectionPool.NewConn(proc.ConnectionString())
                                , onError: null, progress: null);
                        }
                        catch (Exception ex)
                        {
                            errorStr = String.Format("error in sql {0}: {1}", proc.CmdText ?? "???", ex.Message);
                            ErrorCase(errorStr, ex, onError, proc);
                        }
                    }

                    // System.AppDomain.CurrentDomain.UnhandledException 
                    if (reader == null || reader.IsClosed || reader.Depth != 0)
                        return null;

                    if (readerGet != null)
                    {
                        try { readerGet(reader); }
                        catch (Exception ex) { proc.LastError = ex; }
                    }
                    return new Tuple<DbDataReader, IDbConnection>(reader, proc.Connection);
                });
        }

        static string strAgain = "already an open DataReader associated";

        static void ErrorCase(string errorStr, Exception ex, Action<Exception> onError, ISqlProc proc)
        {
            System.Diagnostics.Trace.Write(errorStr);
            if (onError != null)
                onError(ex);
            if (proc is SqlProc && (proc as SqlProc).Context != null)
                (proc as SqlProc).Context.LastError = ex;
            if (proc is ILastError)
                (proc as ILastError).LastError = ex;
        }

        // Unsafe
        public static SqlDataReader ExecMultiReader(this ISqlProc proc,
                          Action<SqlCommand> setup = null,
                          Action<Exception> onError = null,
                          Action<double> progress = null)
        {
            var command = proc.CreateCommand() as SqlCommand;
            var connStr = proc.ConnectionString();

            SqlConnection connection = null;
            if (setup != null)
            {
                setup(command);
                if (command.Connection == null) { 
                    connection = proc.OpenConnection() as SqlConnection;
                    if (connStr != null && connection == null)
                        connection = ConnectionPool.NewConn(connStr);

                    command.Connection = connection;
                }

                connection = command.Connection;
            }
            if (connection == null)
            {
                if (connStr == null)
                    connection = proc.PreparedSqlConnection(); // .Connection as SqlConnection;
                else
                {
                    Guard.Check(!string.IsNullOrWhiteSpace(connStr), "Exec MultiReader connection error");
                    connection = ConnectionPool.NewConn(connStr);
                }
            }

            Guard.Check(connection != null);
            if (connection.State != ConnectionState.Open)
            {
                if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                {
                    // if Broken connection
                    try
                    {
                        connection.Open();
                    }
                    catch
                    {
                        connection = ConnectionPool.NewConn(connStr);
                    }
                }

                if (connection.State != ConnectionState.Open)
                    connection.Open();
                if (connection.State != ConnectionState.Open)
                    return null;
            }

            if (connection.Database != proc.DbName)
                connection.ChangeDatabase(proc.DbName);
            command.Connection = connection;
            proc.Connection = command.Connection;   // for dispose

            if (progress != null)
                progress(0.0);

            SqlDataReader dataReader = null;
            proc.CloseOnDispose = true;     //  CommandBehavior.CloseConnection

            if (onError != null)
                try
                {
                    dataReader = command.ExecuteReader(behavior: CommandBehavior.CloseConnection);
                }
                catch (SqlException exSql) { onError(exSql); }
                catch (Exception ex) { onError(ex); }
            else
                dataReader = proc.ExecuteReader(command) as SqlDataReader;

            if (dataReader != null && dataReader.IsClosed) // !dataReader.Read())
            {
                if (progress != null)
                    progress(1.0);

                return null;
            }

            return dataReader;
        }

    }

}