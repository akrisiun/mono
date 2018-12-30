using Mono.Entity;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Infrastructure
{
    public  static class DbDataSetConvert
    {
#if NET451 || WPF || WEB
        public static IDbSet<T> ToDbSet<T>(this DataReaderEnum<T> reader, Context context) where T : class
        {
            var proc = reader.Proc as SqlProc;
            context = context ?? proc.Context;

            var list = reader.ToList();
            var set = new DbDataSet<T>(list, context);
            return set;
        }
#endif
    }

    public class DbDataSet<T> : IDbSet<T>, IEnumerator<T> where T : class
    {
        public DbDataSet(IList<T> list = null, Context context = null)
        {
            ListData = list ?? new List<T>();
        }

        public IList<T> ListData { get; protected set; }

        public virtual bool Changed { get; set; }
        public virtual void SaveChanges() { }

        protected IEnumerator<T> Worker;
        public T Current { get { return Worker.Current; } }

        public  bool MoveNext() { return Worker.MoveNext();}
        public  void Reset() { Worker = ListData.GetEnumerator(); }
        public  void Dispose() { Worker.Dispose(); ListData = null; }

        public IEnumerator<T> GetEnumerator() { return ListData.GetEnumerator(); }
        object System.Collections.IEnumerator.Current { get { return Current; } }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public int Count { get { return ListData.Count; } }
        public  bool IsReadOnly { get { return ListData.IsReadOnly; } }

        public void Add(T item) { ListData.Add(item); }
        public void Clear() { ListData.Clear(); }
        public bool Contains(T item) { return ListData.Contains(item);  }
        public void CopyTo(T[] array, int arrayIndex) { ListData.CopyTo(array, arrayIndex); }
        public bool Remove(T item) { return ListData.Remove(item); }

        public IList<T> ToList() { return ListData; }
    }

}
