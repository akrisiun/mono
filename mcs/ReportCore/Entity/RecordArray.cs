using Mono.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mono.Entity
{
    public class RecordArray<T> : IFirstRecord<T>, IDataObj,
        ICollection<T>, IEnumerator, IDisposable where T : new() // IComparable
    {
        protected List<T> array;
        public Action<double> Progress { [DebuggerStepThrough] get; set; }
        public Exception LastError { [DebuggerStepThrough] get; set; }

        public T First { [DebuggerStepThrough] get; protected set; }
        public int RecordNumber { [DebuggerStepThrough] get; protected set; }

        #region ctor, dispose

        public RecordArray()
        {
            this.array = new List<T>();
            this.RecordNumber = -1;
        }

        public virtual void Dispose()
        {
            array.Clear();
        }

        #endregion

        public virtual bool Read()
        {
            RecordNumber++;
            return true;
        }

        IEnumerable<object[]> IDataObj.Array { get { return null; } }

        #region IFirstRecord

        public IFirstRecord<T> SetFirst()
        {
            RecordNumber = -1;
            return this;
        }

        public bool Any() { return First != null; }
        public bool Prepare() { return Any(); }

        public void Reset() { RecordNumber = -1; } // no sql reset, forward only

        public bool MoveNext()
        {
            bool success = Read();
            if (!success)
            {
                if (Progress != null)
                    Progress(1.0);
                return false;
            }

            RecordNumber++;
            Current = array[RecordNumber];
            return true;
        }

        public T Current { get; protected set; }
        object IEnumerator.Current { get { return this.Current; } }

        #endregion

        public virtual void Add(T item)
        {
            this.array.Add(item);
        }

        public virtual T Get(int index)
        {
            if (index < 0 || index > this.array.Count)
                return default(T); // null

            return this.array[index];
        }

        #region ICollection, IList, IEnumerable

        public int Count { get { return this.array.Count; } }
        public bool IsReadOnly { get { return false; } }

        public void Clear() { this.array.Clear(); }
        public bool Contains(T item) { return this.array.Contains(item); }
        public virtual void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
        public bool Remove(T item) { return this.array.Remove(item); }

        public IEnumerator<T> GetEnumerator() { return this.array.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion
    }

    // Record = KeyValuePair<string, TValue>

    public class RecordDict<TValue> : RecordArray<KeyValuePair<string, TValue>>,
            ICollection<KeyValuePair<string, TValue>>,
            IDictionary<string, TValue> where TValue : new() // IComparable
    {

        public RecordDict() : base()
        {

        }

        public virtual void Add(string Name, TValue data)
        {
            this.Add(new KeyValuePair<string, TValue>(Name, data));
        }

        public ICollection<string> Keys { get { return GetKeys(); } }
        public ICollection<TValue> Values { get { return GetValues(); } }

        private ICollection<string> GetKeys()
        {
            var list = new List<string>();
            foreach (var item in this.array)
                list.Add(item.Key);
            return list;
        }
        private ICollection<TValue> GetValues()
        {
            var list = new List<TValue>();
            foreach (var item in this.array)
                list.Add(item.Value);
            return list;
        }

        public TValue this[string key] { get { return this.Get(key).Value; } set { this.Set(key, value); } }
        public KeyValuePair<string, TValue> Get(string key)
        {
            int index = Index(key);
            TValue value = index >= 0 ? array[index].Value : default(TValue);
            return new KeyValuePair<string, TValue>(key, this[key]);
        }
        public void Set(string key, TValue value)
        {
            int index = Index(key);
            if (index >= 0)
                array[index] = new KeyValuePair<string, TValue>(key, value);
            else
                array.Add(new KeyValuePair<string, TValue>(key, value));
        }

        public bool ContainsKey(string key) { return Index(key) >= 0; }
        public int Index(string key)
        {
            for (int index = 0; index < array.Count; index++)
                if (array[index].Key == key)
                    return index;
            return -1;
        }
        public bool Remove(string key)
        {
            int index = Index(key);
            if (index >= 0)
            {
                array.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out TValue value)
        {
            int index = Index(key);
            if (index >= 0)
            {
                value = array[index].Value;
                return true;
            }

            value = default(TValue);
            return false;
        }
    }

}