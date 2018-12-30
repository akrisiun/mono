using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class ResultArray : IEnumerable<object[]>, IFirstRecord<object[]>
    {
        public static ResultArray Empty(DbEnumeratorData worker, Exception error) {
            return new ResultArray() { Worker = worker, LastError = error };
        }
        public static IEnumerator<object[]> EmptyList { get; set; }  = new EmptyArray();
        public class EmptyArray : IEnumerator<object[]>
        {
            public object[] Current => null;
            object IEnumerator.Current => null;
            public void Dispose() { }
            public bool MoveNext() { return false; }
            public void Reset() { }
        }

        public DbEnumeratorData Worker { get; set; }
        public SqlDataReader Reader { get; set; }
        public SqlConnection Connection { get; set; }

        public object[] First => Worker?.First;
        public object[] Current => Worker?.First;

        public int RecordNumber => Worker?.RecordNumber ?? 0;
        object IEnumerator.Current => Worker?.Current;

        public Exception LastError { get { return Worker?.LastError; } set { Worker.LastError = value; } }
        public IEnumerator<object[]> GetEnumerator()
        {
            if (!Worker.Prepare())
                return EmptyList;
            return Worker;
        }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        public bool Prepare() { return Worker?.Prepare() ?? false; }

        public bool Any() { return Worker?.Any() ?? false; }
        public bool MoveNext() { return Worker.MoveNext(); }
        public void Reset()
        {
            Worker?.Reset();
        }

        public void Dispose()
        {
            Worker?.Dispose();
            Worker = null;
        }
    }

}
