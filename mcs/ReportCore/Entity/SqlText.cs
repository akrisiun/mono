using System;
using System.Collections.Generic;
using System.Data;
#if NET451   // || WPF
using System.Data.Odbc;
#endif
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Mono.Entity
{
    public class SqlText : ISqlProc
    {
        public IDbConnection Connection { get; set; }
        public bool CloseOnDispose { get; set; }

        public string CmdText { get; set; }
        public void Dispose()
        {
            if (Connection != null)
                Connection.Dispose();
        }

        public virtual IDbConnection OpenConnection()
        {
            return Connection;
        }

        // ISqlProc.
        public IDbCommand LastCommand { get; protected set; }

        public virtual IDbCommand CreateCommand()
        {
            IDbCommand cmd = null;
            if (Connection is SqlConnection)
                cmd = new SqlCommand(cmdText: CmdText, connection: Connection as SqlConnection);
#if NET451 && !NET47 || WPF
            else if (Connection is OdbcConnection)
                cmd = new OdbcCommand(cmdText: CmdText, connection: Connection as OdbcConnection);
#endif                

            if (cmd == null)
                return cmd;
            if (cmd.Connection == null)
                throw new System.ArgumentNullException("Connection");

            cmd.CommandType = CommandType.Text;
            this.LastCommand = cmd;
            return cmd;
        }

        public virtual SqlDataReader ExecuteReader(SqlCommand cmd)
            => cmd.ExecuteReader(this.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);
        IDataReader ISqlProc.ExecuteReader(IDbCommand cmd)
            => this.ExecuteReader(cmd as SqlCommand) as IDataReader;

        public virtual Task<SqlDataReader> ExecuteReaderAsync(IDbCommand cmd)
            => (cmd as SqlCommand).ExecuteReaderAsync(this.CloseOnDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);

        // #if ENTITY
        public IList<T> Exec<T>(IList<T> list, Action<double> progress = null) where T : class
        {
            var helper = new DbDataMapHelper<T>();
            DbGetHelper.ExecFill<T>(this, list, helper, progress);

            return list;
        }

        public IList<object[]> ExecEnum()
        {
            var helper = new DbDataMapHelper<object[]>();

            var list = new List<object[]>();
            list.AddRange(
             DbGetHelper.ExecEnumerable(this, helper, null));
            return list;
        }

        // [Obsolete]
        public object[] ExecFirst()
        {
            var numerator = ExecEnum();
            if (numerator.Count > 0)
            {
                using (var num = numerator.GetEnumerator())
                {
                    if (num?.MoveNext() ?? false)
                        return num.Current;
                }
            }
            return null;
        }
// #endif

        public string ConnectionString()
        {
            return Connection.ConnectionString;
        }

        public string DbName
        {
            get { return Connection.Database; }
        }

        public Exception LastError { get; set; }
    }
}
