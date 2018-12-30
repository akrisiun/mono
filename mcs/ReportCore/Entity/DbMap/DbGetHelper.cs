using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Mono.Entity
{
#if XLSX && !WEB && !NPOI_ORIGIN
    using Mono.Internal;
#else 
    using Mono.Reflection;
#endif
    using System.Data.Common;

    // DbDataMapHelper
    // IDataMapHelper

    public static class DbGetHelper
    {
        public static ICollection<object[]> SqlGetCollectionArray(this ISqlProc procedure)
        {
            return SqlGetCollection<object[]>(procedure);
        }

        public static IList<T> ListWithRange<T>(this IEnumerable<T> source, IList<T> list = null)
        {
            list = list ?? (IList<T>)new List<T>();
            if (source != null)
            {
                var num = source.GetEnumerator();
                if (num != null)
                    list.AddRange(num);
            }
            return list;
        }

        #region Transform

        public static ICollection<T> SqlGetCollection<T>(this ISqlProc procedure) where T : class
        {
            var list = new Collection<T>();
            var helper = new DbDataMapHelper<T>();

            procedure.LastError = null;
            DbGetHelper.ExecFill<T>(procedure, list, helper, progress: null);
            if (procedure.LastError != null)
                throw procedure.LastError;

            return list;
        }

        public static ICollection<T> SqlGetCollection<T>(this ISqlProc procedure, int index) where T : class
        {
            var list = new Collection<object[]>();
            var helper = new DbDataMapHelper<object[]>();

            procedure.LastError = null;
            DbGetHelper.ExecFill<object[]>(procedure, list, helper, progress: null);
            if (procedure.LastError != null)
                throw procedure.LastError;

            return list.ArrayTransform<T>(index);
        }

        public static ICollection<T> ArrayTransform<T>(this IEnumerable<object[]> source, int index) where T : class
        {
            var target = new Collection<T>();
            foreach (object[] item in source)
            {
                T value = (T)item[index];
                if (value != null)
                    target.Add(value);
            }

            return target;
        }

        public static ICollection<T> ArrayTransform<T>(this IEnumerator<object[]> source, int index) where T : class
        {
            var target = new Collection<T>();
            while (source.MoveNext())
            {
                T value = (T)source.Current[index];
                if (value != null)
                    target.Add(value);
            }

            return target;
        }

        public static Collection<T> EnumTransform<T>(this IEnumerable source, string path) where T : class
        {
            var target = new Collection<T>();
            foreach (var item in source)
            {
                T value = item.GetValue<T>(path);
                if (value != null)
                    target.Add(value);
            }

            return target;
        }

        #endregion

        // in T
        public static bool ExecFill<T>(this ISqlProc proc, ICollection<T> list,
                    IDataMapHelper<T> mapper,
                    Action<double> progress = null) where T : class
        {
            Guard.Check(proc.Connection != null, "proc.Connection null error in ExecFill");
            Guard.Check(list != null, "list null error in ExecFill");
            if (proc.Connection.State != ConnectionState.Open)
                proc.Connection.Open();

            using (var command = proc.CreateCommand())
            {
                proc.Connection = command.Connection;
                if (progress != null)
                    progress(0.0);

                using (SqlDataReader dataReader = SqlProcConnect.ExecuteWithReconnect(proc))
                {
                    var results = list as IList<T>;
                    if (!dataReader.Read())
                    {
                        dataReader.Dispose();
                        if (progress != null)
                            progress(1.0);
                        return false;
                    }

                    var helper = mapper; //  DbDataMapHelper<T>
                    helper.GetProperties(dataReader, null);

                    do
                    {
                        object[] objVal = helper.DbRecordArray();
                        // int ret = 
                        dataReader.GetValues(objVal);
                        T val = helper.SetValues(dataReader, objVal);
                        results.Add(val);
                    }
                    while (dataReader.Read());

                    dataReader.Dispose(); // Close();
                    if (progress != null)
                        progress(1.0);

                    return true;
                }
            }
        }

        public class FirstArray : IFirstRecord<object[]>, IEnumerator, IDisposable // , IDataArray
        {
            public IDataMapHelper<object[]> Mapper { [DebuggerStepThrough] get; set; }
            public IDbCommand Command { [DebuggerStepThrough] get; set; }
            public SqlDataReader DataReader { [DebuggerStepThrough] get; set; }
            public Action<double> Progress { [DebuggerStepThrough] get; set; }
            public Exception LastError { [DebuggerStepThrough] get; set; }

            public object[] First { [DebuggerStepThrough] get; protected set; }
            public int RecordNumber { [DebuggerStepThrough] get; protected set; }

            public FirstArray()
            {
                RecordNumber = -1;
            }

            public void Dispose()
            {
                if (DataReader != null)
                    DataReader.Dispose();
                if (Command != null)
                    Command.Dispose();
            }

            public bool Read() {
                if (DataReader != null && DataReader.Read())
                {
                    RecordNumber++;
                    return true;
                }
                if (!DataReader.IsClosed)
                    DataReader.Dispose(); // Close();
                return false;
            }
            public FirstArray SetFirst()
            {
                var dataReader = DataReader;
                if (dataReader == null || dataReader.IsClosed || dataReader.FieldCount <= 0
                    || RecordNumber != 0)
                {
                    if (RecordNumber == -2)
                        return this;
                    return null;
                }

                var helper = Mapper; //  DbDataMapHelper<T>
                helper.GetProperties(dataReader, null);

                object[] first = helper.DbRecordArray();
                dataReader.GetValues(first);
                First = first;
                Current = first;

                RecordNumber = -2;
                return this;
            }

            public bool Any() { return First != null; }
            public bool Prepare() { return Any(); }

            public void Reset() { } // no sql reset, forward only
            public bool MoveNext()
            {
                if (RecordNumber == -2 && Current != null)
                {
                    RecordNumber = 0;
                    return true;
                }

                bool success = Read();
                if (!success)
                {
                    if (Progress != null)
                        Progress(1.0);
                    return false;
                }

                object[] objVal = Mapper.DbRecordArray();
                DataReader.GetValues(objVal);
                Current = objVal;
                return true;
            }

            public object[] Current { get; protected set; }
            object IEnumerator.Current { get { return this.Current; } }

            //void IDataArray.ReadSource(IEnumerable<object[]> source) { }
            //XDocument IDataArray.GetXml(string[] names) { return null; }
        }

        public static IFirstRecord<object[]> ExecFillArray(this ISqlProc proc,
                    Action<double> progress = null)
        {
            Mono.Guard.Check(proc.Connection != null, "proc.Connection null error in ExecFill");
            if (proc.Connection.State != ConnectionState.Open)
                proc.Connection.Open();

            var result = new FirstArray();
            result.Mapper = new DbDataMapHelper<object[]>();
            var command = proc.CreateCommand();
            result.Command = command;

            proc.Connection = command.Connection;
            if (progress != null)
                progress(0.0);

            SqlDataReader dataReader = SqlProcConnect.ExecuteWithReconnect(proc);
            result.DataReader = dataReader;

            if (!result.Read())
            {
                if (progress != null)
                    progress(1.0);
                return result;
            }

            result.SetFirst();
            return result;
        }

        public static Tuple<object, SqlCommand> ExecuteScalar(this SqlProc proc)
        {
            var tuple = ConnCmd(proc);
            var cmd = tuple.Item2;
            Tuple<object, SqlCommand> result = new Tuple<object, SqlCommand>(null, null);
            try
            {
                result = new Tuple<object, SqlCommand>(cmd.ExecuteScalar(), cmd);
            }
            catch (Exception ex) { if (proc != null) proc.Context.LastError = ex; }

            if (proc.Connection != null)
                proc.Connection.Dispose();
            return result;
        }

        public static Tuple<SqlConnection, SqlCommand> ConnCmd(ISqlProc proc)
        {
            var connection = ConnectionPool.NewConn(proc.ConnectionString());

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                if (connection.State != ConnectionState.Open)
                    return new Tuple<SqlConnection, SqlCommand>(null, null);
            }
            if (connection.Database != proc.DbName && !string.IsNullOrWhiteSpace(proc.DbName))
                connection.ChangeDatabase(proc.DbName);

            var command = proc.CreateCommand();
            command.Connection = connection;

            return new Tuple<SqlConnection, SqlCommand>(connection, command as SqlCommand);
        }

        public static Tuple<IEnumerable<object[]>, SqlDataReader> ExecEnumerableReader(this ISqlProc proc,
                    IDataMapHelper<object[]> mapper,
                    Action<double> progress = null, Action<SqlField[]> onReadFields = null)
        {
            Tuple<SqlConnection, SqlCommand> getConnCmd = ConnCmd(proc);
            var command = getConnCmd.Item2;
            if (command == null)
                return new Tuple<IEnumerable<object[]>, SqlDataReader>(System.Linq.Enumerable.Empty<object[]>(), null);

            SqlDataReader dataReader = (SqlDataReader)proc.ExecuteWithReconnect();
            // command.ExecuteReader(CommandBehavior.CloseConnection);
            {
                IEnumerable<object[]> iterate = Iterate(dataReader, command, getConnCmd.Item1,
                        mapper, progress, onReadFields);
                return new Tuple<IEnumerable<object[]>, SqlDataReader>(iterate, dataReader);
            }
        }

        public static IEnumerable<object[]> ExecEnumerable(this ISqlProc proc,
                    IDataMapHelper<object[]> mapper,
                    Action<double> progress = null, Action<SqlField[]> onReadFields = null)
        {
            Mono.Guard.Check(proc.Connection != null, "proc.Connection null error in ExecFill");

            Tuple<SqlConnection, SqlCommand> getConnCmd = ConnCmd(proc);

            if (progress != null)
                progress(0.0);
            var command = getConnCmd.Item2;
            if (command == null)
                return System.Linq.Enumerable.Empty<object[]>();

            IEnumerable<object[]> iterate = null;
            using (SqlDataReader dataReader = (SqlDataReader)proc.ExecuteWithReconnect())
            // command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                iterate = System.Linq.Enumerable.ToList(
                    Iterate(dataReader, command, getConnCmd.Item1,
                        mapper,
                        progress, onReadFields)
                        );
            }

            getConnCmd.Item1.Dispose();
            getConnCmd.Item2.Dispose();
            return iterate;
        }

        private static IEnumerable<object[]> Iterate(SqlDataReader dataReader, SqlCommand command, SqlConnection conn,
            IDataMapHelper<object[]> mapper,
            Action<double> progress = null, Action<SqlField[]> onReadFields = null)
        {

            if (dataReader == null || dataReader.IsClosed || !dataReader.Read())
            {
                if (!dataReader.IsClosed)
                    dataReader.Dispose(); // Close();
                if (progress != null)
                    progress(1.0);

                yield break;
            }

            // Task<T> GetFieldValueAsync<T>(int i, CancellationToken cancellationToken);
            var helper = mapper; //  new DbDataMapHelper<T>();
            helper.GetProperties(dataReader, null);

            if (onReadFields != null)
                onReadFields(helper.GetFields(dataReader));

            do
            {
                object[] objVal = helper.DbRecordArray();
                dataReader.GetValues(objVal);

                object[] val = helper.SetValues(dataReader, objVal);
                yield return val;
            }
            while (!dataReader.IsClosed && dataReader.Read());

            if (progress != null)
                progress(1.0);

            dataReader.Dispose();
            command.Dispose();
            if (conn.State != ConnectionState.Executing)
                conn.Dispose();
        }

    }

    public class DbObject : DbObjectSimple, IDbObject, IFirstRecord<object[]>, IDataMapHelperSet<object[]>, IDisposable
    {
        public override IDataMapHelper<object[]> GetProperties(DbDataReader dataReader, Action<Exception> onError = null)
        {
            if (dataReader is SqlDataReader)
                Reader = dataReader as SqlDataReader;
            return base.GetProperties(dataReader, onError: onError);
        }

        DbDataReader IDbObject.Reader { get { return this.Reader; } }
        IDbConnection IDbObject.Connection {
            get { return SqlProc?.Connection; }
            set { if (SqlProc != null) SqlProc.Connection = value; }
        }

        public new object[] SetValues(object[] objVal) { return DbRecordArray(Reader, objVal.Length); }
        public new object[] DbRecordArray() { return DbRecordArray(Reader, Reader.IsClosed ? 0 : Reader.FieldCount); }

        public int? GetOrdinal(string columnName) { return Reader.GetOrdinal(columnName); }
        public object GetField(string columnName, object[] arrayItem)
        {
            int? ord = GetOrdinal(columnName);
            if (arrayItem == null || ord == null || ord < arrayItem.Length)
                return null;
            return arrayItem[ord.Value];
        }

        public static DbObject Exec(SqlProc proc, Action<double> progress = null, bool withFirst = true)
        {
            var mapper = new DbObject();
            // mapper.Worker = DbGetHelper.ExecEnumerable(proc, mapper, progress).GetEnumerator();

            mapper.SqlProc = proc as ISqlProc;
            if (!mapper.Prepare())
                return null;
            if (withFirst && mapper.Worker.Current == null)
                mapper.Worker.MoveNext();

            return mapper;
        }

        public ISqlProc SqlProc { get; private set; }
        protected IDictionary<string, SqlFieldInfo> fields;
        public IDictionary<string, SqlFieldInfo> Fields { get { return this.fields; } }

        public object MoveNextField(string columnName)
        {
            if (Worker.Current == null && !Worker.MoveNext())
                return null;
            return GetField(columnName, Worker.Current);
        }

        public SqlDataReader Reader { get; set; }
        public IEnumerator<object[]> Worker;
        public object[] First
        {
            get
            {
                if (Worker.Current != null)
                    return Worker.Current;
                if (Worker.MoveNext())
                    return Worker.Current;
                return null;
            }
        }

        public new void Dispose()
        {
            base.Dispose();
            if (Reader != null)
                Reader.Dispose();
            Reader = null;
            if (SqlProc != null)
                SqlProc.Dispose();
            SqlProc = null;
        }

        // public virtual int RecordNumber { get; private set; }

        public virtual bool Any()
        {
            if (Current == null && (!Prepare() || !MoveNext())) return false;
            return Current != null;
        }

        public virtual bool Prepare()
        {
            bool success = false;
            try
            {
                Tuple<IEnumerable<object[]>, SqlDataReader> tuple
                    = DbGetHelper.ExecEnumerableReader(this.SqlProc, this, progress: null);
                // , progress); - (tuple.Item1 == null || tuple.Item2.IsClosed || tuple.Item1 == null)

                this.Reader = tuple.Item2;
                if (Reader != null && !Reader.IsClosed && tuple.Item1 != null)
                {
                    this.Worker = tuple.Item1.GetEnumerator();
                    success = (this.Worker != null);
                }
            }
            catch (Exception ex) { LastError = ex; }

            return success;
        }

        public new virtual bool MoveNext()
        {
            if (Worker == null)
                return false;
            RecordNumber++;
            if (RecordNumber == 0 && First != null)
                return true;
            return Worker.MoveNext();
        }

        public virtual void Reset()
        {
            if (Worker == null)
                throw new NotImplementedException();
            if (RecordNumber == 0 && Reader != null && Reader.IsClosed)
                return;   // OK otherwise

            RecordNumber = -1;
            Prepare();
        }

        public new object[] Current
        {
            get
            {
                if (Worker != null)
                    base.Current = Worker.Current;

                return base.Current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            if (!Prepare())
                return null;
            return Worker;
        }

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }
}
