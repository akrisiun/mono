using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mono.Entity
{
    using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;

    public class DataReaderEnum : IDisposable, ILastError, IEnumerator, IFirstRecordWrap
    {
        #region Static 
        public static object[] ParseDbNull(object[] value)
        {
            if (value == null)
                return new object[] { }; // empty

            for (int i = 0; i < value.Length; i++)
                if (DBNull.Value.Equals(value[i]))
                    value[i] = null;
            return value;
        }

        public static bool PreparedReader(ILastError obj, DbDataReader reader, IDbConnection connection)
        {
            if (reader == null && reader.IsClosed)
                return false;

            return connection != null && connection.State == ConnectionState.Open;
        }

        public static bool PreparedReader(ILastError obj, SqlDataReader reader, ISqlProc proc, Context db)
        {
            if (reader == null && reader.IsClosed)
                return false;
            if (reader.FieldCount > 0)
                return true;

            return false;
        }

        #endregion

        // KeyValuePair<SqlDataReader, SqlCommand, IEnumerator>
        public KeyReaderCmdEnum Data { get; protected set; }
        public SqlCommand SqlCommand { get { return Data.Item2; } }
        DbDataReader IFirstRecordWrap.Reader { get { return Data.Item1; } }

        public ISqlProc Proc { get; protected set; }
        public IDbConnection Connection { get { return Proc == null ? null : Proc.Connection; } }
        public Exception LastError { get; set; }

        public DataReaderEnum(ISqlProc proc, Func<ISqlProc, KeyReaderCmdEnum> exec = null)
        {
            this.Proc = proc;
            if (exec != null)
            {
                this.LastError = null;
                Data = exec(proc);
                if (Data.Item3 is ILastError)
                {
                    this.LastError = (Data.Item3 as ILastError).LastError;
                }
            }

            if (Data == null)
                Data = new KeyReaderCmdEnum(null, null, EmptyNumerator.Empty);
        }

        public DataReaderEnum(SqlDataReader reader, IEnumerator numer, SqlCommand cmd = null)
        {
            cmd = cmd ?? (SqlCommand)this.Proc.LastCommand;
            Data = new KeyReaderCmdEnum(reader, cmd, numer);
        }

        public void DisposeReader(bool fromProc = false)
        {
            if (Data != null && Data.Item1 != null && !Data.Item1.IsClosed)
            {
                Data.Item1.Dispose(); // Close();
                // Data.Item1 = null;
            }
            if (fromProc)
                Proc = null;
        }
        public void Dispose()
        {
            DisposeReader();
            Data = null;
            if (Proc != null)
                Proc.Dispose();
        }

        public IEnumerator GetEnumerator() { return Data.Item3; }
        public object Current
        {
            get
            {
                if (Data.Item3 == null) return null;
                var value = Data.Item3.Current;
                if (value != null && value.GetType().IsArray)
                    return ParseDbNull(value as object[]);
                return value;
            }
        }

        public void Reset() { if (Data.Item3 != null) Data.Item3.Reset(); }
        public bool MoveNext() { return Data.Item3 != null && Data.Item3.MoveNext(); }
    }

    public class DataReaderArray : DataReaderEnum, IDisposable, IFirstRecordWrap<object[]>, IEnumerator<object[]>
    {
        public SqlDataReader Reader { [DebuggerStepThrough] get { return Data.Item1; } }
        public IDictionary<string, object> Header { get; set; }
        public IFirstRecord<object[]> Value
        {
            get
            {
                return Data.Item3 as IFirstRecord<object[]>
                    ?? DbEnumeratorData.Empty as IFirstRecord<object[]>;
                // System.Linq.Enumerable.Empty<object[]>().GetEnumerator();
            }
        }

        public MultiResultReader<object[]> MultiResultReader { get { return Data.Item3 as MultiResultReader<object[]>; } }
        object[] IEnumerator<object[]>.Current { get => ((object)Data.Item3?.Current) as object[]; }

        public bool MoveNextResult()
        {
            var reader = Reader;
            if (reader == null || !DbCheck.IsReaderAvailable(reader))
                return false;

            var multiReader = MultiResultReader as MultiResultReader<object[]>;

            // bool NextResult() IReaderNextResult
            if (!multiReader.NextResult())
                return false;

            // KeyValuePair<SqlDataReader, SqlCommand, IEnumerator>
            var cmd = this.Data.Item2;
            var multiReader2 = multiReader.PrepareNext(this.Proc);
            if (multiReader2 == null)
                return false;

            Data = new KeyReaderCmdEnum(reader, cmd, multiReader2);
            if (multiReader2.RecordNumber == 0 && multiReader2.Current != null)
                return true; // prepared

            return multiReader2.MoveNext();
        }

        public IFirstEnumerable<object[]> Worker { get; private set; }

        #region ctor, static

        public DataReaderArray(ISqlProcReader proc) : base(proc, proc == null ? null : ExecArray())
        {
            if (this.Data.Item3 is MultiResultReader<object[]> numer) {
                if (numer.RecordNumber < 0 && numer.Prepare())
                    numer.MoveNext();
                if (Data.Item1 == null)
                    Data = new KeyReaderCmdEnum(numer.Reader, proc.LastCommand as SqlCommand, numer);
            }

            Worker = Data.Item3 as IFirstEnumerable<object[]>;
        }

        private static Func<ISqlProc, KeyReaderCmdEnum> ExecArray()
        {
            return new Func<ISqlProc, KeyReaderCmdEnum>((proc) =>
            {
                if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText))
                    return DataReaderArray.Empty.Data;

                MultiResult<object[]> result = new MultiResultReader<object[]>(proc);
                if (proc.Connection != null && proc.Connection.State == ConnectionState.Closed)
                    proc.OpenConnection();

                result.Prepare(proc as ISqlProcReader, noMoveFirst: false);
                return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
            });
        }
        public static DataReaderArray Empty;
        static DataReaderArray()
        {
            Empty = new DataReaderArray(null);
            if (Empty.Data.Item3 == null)
                Empty.Data = new KeyReaderCmdEnum(null, null, EmptyNumerator.Empty);
        }

        #endregion
    }

    public class DataReaderEnum<T> : DataReaderEnum, IFirstRecordWrap<T>, IDisposable where T : class
    {
        public SqlDataReader Reader { [DebuggerStepThrough] get { return Data.Item1; } }
        public IDictionary<string, object> Header { get; set; }

        public IEnumerator<T> Value
        {
            get { return Data.Item3 as IEnumerator<T> ?? System.Linq.Enumerable.Empty<T>().GetEnumerator(); }
        }

        public IFirstEnumerable<T> Worker { get; private set; }
        public object[] CurrentArray { [DebuggerStepThrough] get { return base.Current as object[]; } }
        public new T Current { [DebuggerStepThrough] get { return Worker == null ? null : Worker.Current; } }

        public static Task<DataReaderEnum<T>> FromTask(ISqlProc proc, Func<ISqlProc, KeyReaderCmdEnum> prepare = null)
        {
            var obj = new DataReaderEnum<T>(proc, prepare ?? ExecAsync(), next: false);
            if (obj.Data.Item3 is MultiResultReader<T> numer && numer.RecordNumber < 0 && numer.Prepare()) {
                var task = numer.MoveNextAsync();
                task.GetAwaiter().GetResult();
                if (proc != null && numer.LastError != null)
                    proc.LastError = numer.LastError;
            }
            obj.Worker = obj.Data.Item3 as IFirstEnumerable<T>;
            return Task.FromResult(obj);
        }

        private static Func<ISqlProc, KeyReaderCmdEnum> ExecAsync()
        {
            return new Func<ISqlProc, KeyReaderCmdEnum>((proc) =>
            {
                if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText))
                    return DataReaderEnum<T>.Empty.Data;

                MultiResult<T> result = new MultiResultReader<T>(proc);
                var task = result.PrepareAsync(proc, noMoveFirst: false);
                task.GetAwaiter().GetResult();

                return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
            });
        }
        private static async Task<KeyReaderCmdEnum> ExecAwait(ISqlProc proc)
        {
            if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText)) {
                return await Task.FromResult(Empty.Data); // DataReaderEnum<T>.
            }

            MultiResult<T> result = new MultiResultReader<T>(proc);
            var task = result.PrepareAsync(proc, noMoveFirst: false);

            await task;
            if (task.IsFaulted) {
                proc.LastError = task.Exception;
            }

            return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
        }


        public DataReaderEnum(ISqlProc proc, bool async, Func<ISqlProc, KeyReaderCmdEnum> prepare = null)
            : this(proc, prepare, false)
        { }

        public async Task OpenAsync() 
        {
            var conn = Proc.OpenConnection() as SqlConnection;
            var taskOpen = conn?.OpenAsync();
            if (taskOpen.IsFaulted) {
                LastError = taskOpen.Exception;
            } else  { 
                await taskOpen;
            }
        }

        public async Task<bool> PrepareAsync()
        {
            LastError = null;
            bool ok = false;
            var proc = this.Proc;
            var exec = ExecAwait(proc);
            
            // async
            var r = await exec;

            this.LastError = proc.LastError;
            this.Data = r;
            var worker = Data.Item3 as IFirstEnumerable<T>;
            if (worker != null) {
                this.Worker = worker;
                if (this.Worker?.First == null && worker.LastError == null) {
                    worker.Reset();
                    ok = worker.Prepare();
                }
            }

            ok = LastError == null && this.Worker.First != null;
            return ok;
        }


        public DataReaderEnum(ISqlProc proc, Func<ISqlProc, KeyReaderCmdEnum> prepare = null)
            : this(proc, prepare, false)
        { }

        public DataReaderEnum(ISqlProc proc, Func<ISqlProc, KeyReaderCmdEnum> prepare, bool next)
            : base(proc, prepare ?? Exec())
        {
            if (proc != null && next) {
                proc.LastError = null;
            }
            if (next && this.Data.Item3 is MultiResultReader<T> numer && numer.RecordNumber < 0 && numer.Prepare()) {
                numer.MoveNext();
                if (proc != null && numer.LastError != null)
                    proc.LastError = numer.LastError;
            }
            Worker = Data.Item3 as IFirstEnumerable<T>;
        }

        private static Func<ISqlProc, KeyReaderCmdEnum> Exec()
        {
            return new Func<ISqlProc, KeyReaderCmdEnum>((proc) =>
            {
                if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText))
                    return DataReaderEnum<T>.Empty.Data;

                MultiResult<T> result = new MultiResultReader<T>(proc);
                result.Prepare(proc, noMoveFirst: false);
                return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
            });
        }
        public static readonly DataReaderEnum Empty;
        static DataReaderEnum()
        {
            Empty = new DataReaderEnum(proc: null, exec: null);
        }

        public IList<T> ToList()
        {
            var list = new List<T>();
            if (Worker.RecordNumber == 0)
                list.Add(Worker.Current);

            while (Worker.MoveNext())
                list.Add(Worker.Current);

            return list;
        }
    }

}