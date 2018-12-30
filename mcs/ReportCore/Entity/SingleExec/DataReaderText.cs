using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Mono.Entity
{
    // KeyValuePair<SqlDataReader, SqlCommand, IEnumerator>
    using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;

    public class DataReaderText<T> : DataReaderEnum, IFirstRecordWrap<T>,
        IDisposable where T : class
    {
        public SqlDataReader Reader { [DebuggerStepThrough] get { return Data.Item1; } }
        public IDictionary<string, object> Header { get; set; }

        public IEnumerator<T> Value
        {
            get
            {
                return Data.Item3 as IEnumerator<T> ??
                    System.Linq.Enumerable.Empty<T>().GetEnumerator();
            }
        }

        public object Tag { get; set; }
        public string Message { get; set; }

        public MultiResult<T> WorkerResult { get; private set; }
        IFirstEnumerable<T> IFirstRecordWrap<T>.Worker { get { return WorkerResult as IFirstEnumerable<T>; } }
        T IEnumerator<T>.Current { get => (T)WorkerResult.Current; }

        public DataReaderText(ISqlProc proc)
            : base(proc, Exec())
        {
            var numer = Data.Item3 as MultiResult<T>;
            this.LastError = numer.LastError;

            if (numer != null && numer.RecordNumber < 0)
            {
                if (numer.Prepare())
                    numer.MoveNext();
            }
            WorkerResult = numer; // Data.Value as IFirstRecord<T>;
        }

        private static Func<ISqlProc, KeyReaderCmdEnum> Exec()
        {
            return new Func<ISqlProc, KeyReaderCmdEnum>((proc) =>
            {
                if (proc == null || string.IsNullOrWhiteSpace(proc.CmdText))
                    return DataReaderEnum<T>.Empty.Data;

                MultiResult<T> result = new MultiResultReader<T>(proc);
                result.Prepare(proc as SqlProcResult, noMoveFirst: false, onReadFields: null,  onFieldError: OnFieldError);

                return new KeyReaderCmdEnum(result.Reader, proc.LastCommand as SqlCommand, result);
            });
        }

        public static Action<Exception> OnFieldError { get; set; }

        public static readonly DataReaderEnum Empty;
        static DataReaderText()
        {
            Empty = new DataReaderEnum(proc: null, exec: null);
        }

        public bool PrepareNext(bool lMoveReader = true)
        {
            var result = WorkerResult as MultiResultReader<T>;
            if (lMoveReader)
                result.NextResult();

            if (result != null)
            {
                if (result.PrepareNext(base.Proc) == null)
                    return false;

                return true;
            }

            result.Reset();
            var idx = result.RecordNumber;
            Data.Item3.Reset();
            return false;
        }

        public IList<T> ToList()
        {
            var list = new List<T>();
            if (WorkerResult.RecordNumber == 0)
                list.Add(WorkerResult.Current);
            while (WorkerResult.MoveNext())
                list.Add(WorkerResult.Current);

            return list;
        }

        public bool ReaderAvailable { get { return Reader != null && Reader.FieldCount > 0; } }
    }
}
