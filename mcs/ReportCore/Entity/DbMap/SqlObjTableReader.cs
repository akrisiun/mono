using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class SqlObjTableReader : SqlTableMapper, IDisposable
    {
        #region Properties

        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataReader dataReader;
        private StateExec state;

        public StateExec State { get { return state; } }
        public SqlConnection Connection { get { return conn; } }
        public SqlDataReader DataReader { get { return dataReader; } }

        public SqlObjTableReader(Action<SqlTableMapper, DbDataReader> propertiesParser)
            : base(propertiesParser)
        {
            conn = null;
            cmd = null;
            dataReader = null;
            state = StateExec.Init;
        }

        public SqlConnection OpenConnection(Context db)
        {
            conn = ConnectionPool.NewConn(db.ConnectionString());
            conn.Open();
            if (conn.State != ConnectionState.Open)
                return null;

            conn.ChangeDatabase(db.DbName);
            return conn;
        }

        #endregion

        #region Prepare Reader

        public bool PrepareWithParm(ISqlProc proc, IList<SqlParameter> parm, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            if (parm != null && parm.Count > 0)
                Parameters = parm;

            return Prepare(proc, parser, commandTimeout);
        }

        public IList<SqlParameter> Parameters { get; set; }

        public bool Prepare(ISqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            if (state != StateExec.Init)
                this.Dispose();
            if (parser != null)
                base.propertiesParser = parser;

            LastRow = null;
            var conn = proc.OpenConnection();
            if (conn == null)
                return false;

            cmd = proc.CreateCommand() as SqlCommand;
            cmd.Connection = conn as SqlConnection;
            if (conn.State != ConnectionState.Open)
                conn.Open();

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            proc.LastError = null;

            if (this.Parameters?.Count > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(this.Parameters.ToArray());
            }
            cmd.Prepare();

            RecordIndex = -2;
            try
            {
                dataReader = (SqlDataReader)proc.ExecuteReader(cmd)
                    ?? throw new ArgumentNullException("SqlObjTable ExecuteReader error", proc.LastError);
            }
            catch (SqlException ex)
            {
                // Breakpoint?
                this.LastError = ex.InnerException ?? ex;
            }
            if (dataReader == null)
                return false;

            state = StateExec.Prepared;
            RecordIndex = -1;

            try
            {
                if (dataReader.Read())
                    RecordIndex = 0;
                else if (!dataReader.IsClosed)
                    dataReader.Close();
            }
            catch (SqlException ex) {
                // Breakpoint?
                this.LastError = ex.InnerException ?? ex;
            }
            catch (Exception ex) { this.LastError = ex; }

            if (RecordIndex == 0)   // if Read() success
            {

                GetFirstRecord(this, dataReader);
                return true;
            }

            //  else
            if (dataReader != null && !dataReader.IsClosed && dataReader.FieldCount > 0)
            {
                // empty record
                (this as IDataMapHelper<object[]>).GetProperties(dataReader, null);
                var helper = this;
                object[] objVal = helper.DbRecordArray();
                LastRow = objVal ?? new object[] { };
                return false; // empty RowSet
            }

            return false;
        }

        //int iLen;
        //string[] FieldNames;

        public static IDataMapHelper<object[]> GetFirstRecord(IDataMapHelper<object[]> self, SqlDataReader dataReader)
        {
            if (self is ILastError)
                (self as ILastError).LastError = null;

            try
            {
                // first record array
                if (self.RecordNumber <= -1) //  && dataReader.RecordsAffected <= -1)
                {
                    if (!dataReader.Read())
                        dataReader.Dispose();
                }

                // after Read:

                // First record
                (self as IDataMapHelper<object[]>).GetProperties(dataReader, null);

                var helper = self;
                object[] objVal = helper.DbRecordArray();

                if (!dataReader.IsClosed && dataReader.FieldCount > 0)
                {
                    dataReader.GetValues(objVal);

                    if (self is SqlObjTableReader)
                        (self as SqlObjTableReader).LastRow = helper.SetValues(dataReader, objVal);
                    else if (self is SqlObjTable) // IFirstRecord<object[]>)
                        (self as SqlObjTable).First = (self as SqlObjTable).SetValues(dataReader, objVal);
                    //        (self as IFirstRecord<object[]>).First = ...
                }

            }
            catch (Exception ex)
            {
                if (self is ILastError)
                    (self as ILastError).LastError = ex;
            }

            return self;   // fluent result
        }

        public bool PrepareNextResult(ISqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null)
        {
            this.LastRow = null;
            this.RecordIndex = -1;  // po reader.Read() ++

            SqlDataReader reader = this.dataReader;
            if (reader == null || !DbCheck.IsReaderAvailable(reader))
                return false;

            if (!reader.Read() || reader.IsClosed || reader.FieldCount == 0)
            {
                reader.Dispose();
                return false;
            }

            SqlObjTableReader.GetFirstRecord(this, dataReader);

            return LastRow != null;
        }

        public int RecordIndex { get; set; }
        public object[] LastRow { get; private set; }
        public Exception LastError { get; set; }

        public object[] FirstRecord(SqlProc proc, Action<SqlTableMapper, DbDataReader> parser = null, int? commandTimeout = null)
        {
            if (parser != null)
                base.propertiesParser = parser;

            LastRow = null;
            var conn = OpenConnection(proc.Context);
            if (conn == null)
                return LastRow;

            using (var cmd = proc.CreateCommand())
            {
                cmd.Connection = conn;
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Prepare();
                var dataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)
                    as SqlDataReader; //  CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);

                if (dataReader != null)
                {
                    (this as IDataMapHelper<object[]>).GetProperties(dataReader, null);

                    // CommandBehavior.
                    //  SingleResult = 1,   The query returns a single result set.
                    //    SchemaOnly = 2,
                    //     The query returns column information only. When using System.Data.CommandBehavior.SchemaOnly,
                    //     the .NET Framework Data Provider for SQL Server precedes the statement being
                    //     executed with SET FMTONLY ON.
                    //  KeyInfo = 4,
                    //     The query returns column and primary key information.
                    //  SingleRow = 8,
                    //     The query is expected to return a single row of the first result set. Execution
                    //     may, but are not required to, use this information to optimize the performance
                    //     of the command. When you specify System.Data.CommandBehavior.SingleRow with
                    //     the System.Data.OleDb.OleDbCommand.ExecuteReader() method of the System.Data.OleDb.OleDbCommand
                    //     object, the .NET Framework Data Provider for OLE DB performs binding using
                    //     the OLE DB IRow interface if it is available. Otherwise, it uses the IRowset
                    //     interface. If your SQL statement is expected to return only a single row, also improve performance.

                    var helper = this;
                    object[] objVal = helper.DbRecordArray();
                    dataReader.GetValues(objVal);
                    LastRow = helper.SetValues(dataReader, objVal);

                    cmd.Cancel();
                    // The name/value pair "Asynchronous Processing=true" was not included within
                    dataReader.Dispose();
                }
            }

            return LastRow;
        }

        public void StateExecuting()
        {
            state = StateExec.Executing;
        }

        public int? Records { get { return this.dataReader == null ? null : (int?)dataReader.RecordsAffected; } }

        #endregion

        public IEnumerable<object[]> Query()
        {
            StateExecuting();

            if (dataReader == null)
            {
                Dispose();
                state = StateExec.Init;

                yield break;
            }
            else
            {
                if (LastRow != null)
                    yield return LastRow;

                var helper = this;
                while (!dataReader.IsClosed && dataReader.Read())
                {
                    object[] objVal = helper.DbRecordArray();
                    if (objVal.Length < dataReader.FieldCount)
                    {
                        Array.Resize(ref objVal, dataReader.FieldCount);
                    }
                    dataReader.GetValues(objVal);

                    this.RecordNumber++;
                    LastRow = helper.SetValues(dataReader, objVal);

                    yield return LastRow;
                }

                if (!dataReader.IsClosed)
                    dataReader.Close();
                Dispose();
                state = StateExec.Init;
            }
        }

        public enum StateExec
        {
            Init = 0,
            Prepared = 1,
            Executing = 2
            // Finished = 3
        }

        public override void Dispose()
        {
            base.Dispose();
            if (dataReader != null)
                dataReader.Dispose();
            dataReader = null;

            if (cmd != null)
                cmd.Dispose();
            cmd = null;

            if (conn != null && conn.State != ConnectionState.Closed)
                conn.Close();
            if (conn != null)
                conn.Dispose();
            conn = null;
            state = StateExec.Init;
        }
    }
}
