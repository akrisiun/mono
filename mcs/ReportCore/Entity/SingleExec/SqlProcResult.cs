using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace Mono.Entity
{
#if !SNEX
    using Mono.Entity.Schema;
    using System.Threading.Tasks;
#endif
    using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;

    public class SqlProcResult : SqlProc, ISqlProcReader, ISqlProcContext, IReaderNextResult, IDisposable
    {
        public static SqlProcResult ProcNamed(object namedParam, Context db)
        {
            var properties = new NameProperties(namedParam);
            var proc = new SqlProcResult { Context = db };

            string cmdText = properties.GetValue(namedParam, properties.FirstName()) as string;
            Mono.Guard.Check(cmdText.Length > 4);

            proc.CmdText = cmdText;
            proc.Param = SqlProcExt.ListParam(proc, namedParam, properties);
            proc.Context = db;

            return proc;
        }

        public SqlDataReader Reader {
            [DebuggerStepThrough]
            get; private set;
        }

        IDataReader ISqlProcReader.Reader { get => Reader as IDataReader; }

        public SqlCommand command { get; private set; }

#if !SNEX
        public SqlProcResult SetReader(SqlDataReader reader) { Reader = reader; return this; }
        public SqlProcResult SetCommand(SqlCommand cmd) { command = cmd; return this; }

        public IFirstRecord<KeyValuePair<string, SqlFieldInfo>> Schema 
        { get => Reader == null ? null : Build.GetSchemaTable(Reader);  }
#endif

        public SqlDataReader ExecuteReader(SqlCommand cmd, CommandBehavior behavior = CommandBehavior.SingleResult)
        {
            Guard.Check(Reader == null);
            Reader = cmd.ExecuteReader(behavior);
            return Reader;
        }
        public override IDbCommand CreateCommand()
        {
            command = base.CreateCommand() as SqlCommand;
            Guard.CheckNotNull(command.Connection);
            return command;
        }

        public override void Dispose()
        {
            Reader = null;
            if (command != null)
            {
                command.Dispose();
                command = null;
            }
            base.Dispose();
        }

        #region IReaderNextResult

        DbDataReader IReaderNextResult.Reader { get { return Reader as DbDataReader; } }
        public bool ReaderAvailable { get { return Reader != null; } }
        public Func<Tuple<DbDataReader, IDbConnection>> GetReader {
            get {
                return new Func<Tuple<DbDataReader, IDbConnection>>(() =>
                                new Tuple<DbDataReader, IDbConnection>(Reader, Connection));
            }
        }
        public bool NextResult() { return false; }

        #endregion
    }

    public static class SqlProcResultStatic
    {
        public static DataReaderArray DataReaderArray(this ISqlProcReader procRes)
        {
            if (procRes.Reader != null)
                procRes.Dispose();
            return new DataReaderArray(procRes);
        }

        public static DataReaderExpando DataReaderExpando(this ISqlProcReader procRes)
        {
            if (procRes.Reader != null)
                procRes.Dispose();
            return new DataReaderExpando(procRes);
        }

        public static Task<DataReaderExpando> DataReaderExpandoAsync(this ISqlProcReader procRes)
        {
            if (procRes.Reader != null) {
                procRes.Dispose();
            }
            return global::Mono.Entity.DataReaderExpando.CreateTask(procRes);
        }

        public static DataReaderEnum<T> DataReaderMap<T>(this ISqlProcReader procRes) where T : class
        {
            return new DataReaderEnum<T>(procRes);
        }

        public static DataReaderEnum<XElement> DataReaderXElement(this ISqlProcReader procRes)
        {
            if (procRes.Reader != null)
                procRes.Dispose();
            var firstNode = new DataReaderEnum<XElement>(procRes, ExecXml());
            return firstNode;
        }


        public static Func<ISqlProc, KeyReaderCmdEnum> ExecXml()
        {
            return new Func<ISqlProc, KeyReaderCmdEnum>((proc) =>
            {
                if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText))
                    return DataReaderEnum<XElement>.Empty.Data;

                MultiResult<XElement> result = new MultiResultReader<XElement>(proc, (reader) =>
                {
                    IDataMapHelper<XElement> helper = new XHelper().GetProperties(reader, null);
                    return helper;
                });

                result.Prepare(proc as ISqlProcReader, noMoveFirst: false);
                return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
            });
        }

    }

    public class XHelper : DbObjectSimple, IDataMapHelper<XElement>
    {
        public override object[] SetValues(DbDataReader reader, object[] objVal)
        { return new object[1] { XSetValues(reader, objVal) }; }

        public object XSetValues(DbDataReader reader, object[] objVal, string rootName = "Root")
        {
            var obj = new XElement(rootName);
            if (objVal != null)
                for (int i = 0; i < objVal.Length; i++)
                    if (objVal[i] != null)
                        obj.Add(new XElement(FieldNames[i], objVal[i]));
            return obj;
        }

        public new IDataMapHelper<XElement> GetProperties(DbDataReader dataReader, Action<Exception> onError = null)
        {
            base.GetProperties(dataReader);
            return this;
        }

        XElement IDataMapHelper<XElement>.SetValues(DbDataReader dataReader, object[] objVal)
        {
            return (XElement)XSetValues(dataReader, objVal);
        }

        DbDataReader IDataMapHelper<XElement>.Reader { get { return null; } }
    }
}
