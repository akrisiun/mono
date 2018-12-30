using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;

namespace Mono.Entity
{
    using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;

    public class DataReaderExpando : DataReaderEnum<ExpandoObject>, IDisposable {
        static DataReaderExpando() {
            Empty = new DataReaderExpando(null);
        }
        public new static readonly DataReaderExpando Empty;

        public SqlDataReader SqlReader { [DebuggerStepThrough] get { return Data.Item1; } }

        public DataReaderExpando(ISqlProcReader proc) : base(proc) { }

        /// <summary>
        /// using KeyReaderCmdEnum = System.Tuple<SqlDataReader, SqlCommand, IEnumerator>;
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="async"></param>
        /// <param name="prepare"></param>
        public DataReaderExpando(ISqlProc proc, bool async, Func<ISqlProc, KeyReaderCmdEnum> prepare = null) 
            : base(proc, async: async, prepare: prepare) { 

        }

        public static async Task<DataReaderExpando> CreateTask(ISqlProc proc) {
            var obj = new DataReaderExpando(proc, async: true);

            await obj.PrepareAsync();
            return obj;
        }

        public new IList<ExpandoObject> ToList()
        {
            var list = new List<ExpandoObject>();

            if (Worker.LastError is SqlException)
                throw Worker.LastError;

            if (Worker.RecordNumber == 0)
                list.Add(Worker.Current);
            while (Worker.MoveNext())
                list.Add(Worker.Current);

            return list;
        }
    }

    public class EmptyNumerator : IEnumerator
    {
        public static readonly EmptyNumerator Empty = new EmptyNumerator();
        public object Current { get { return null; } }
        public void Reset() { }
        public bool MoveNext() { return false; }
    }

}