using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Entity
{

    public class FirstRecordList<T> : IFirstRecord<T>, IFirstEnumerable<T> where T : class
    {
        protected FirstRecordList() { Data = new List<T>(); }
        public IList<T> Data { get; protected set; }

        public static FirstRecordList<T> ReadList(IList<T> source) { return Read<FirstRecordList<T>>(source); }
        public static FirstRecordList<T> ReadNum(IEnumerator<T> source) { return Read<FirstRecordList<T>>(source); }
        public static FirstRecordList<T> ReadArray(T[] source) { return Read<FirstRecordList<T>>(source); }
        public static C Read<C>(object source) where C : FirstRecordList<T>
        {
            C num = null;
            Type t = typeof(C);
            if (t == typeof(FirstRecordList<T>))
                num = (C)(object)(new FirstRecordList<T>());
            else
                num = Activator.CreateInstance<C>();

            var Data = num.Data;
            var numSource = source as IEnumerator<T>;
            if (source is IEnumerable<T>)
                numSource = (source as IEnumerable<T>).GetEnumerator();

            if (source is IList<T>)
                Data = (source as IList<T>);
            else if (source is T[])
            {
                foreach (T item in (source as T[]))
                    num.Add(item, source);
            }
            else if (numSource != null)
            {
                if (source is IFirstRecord<T>)
                    (source as IFirstRecord<T>).Reset();
                if (numSource != null)
                    while (numSource.MoveNext() && numSource.Current != null)
                        num.Add(numSource.Current, numSource);
            }

            num.Prepare();
            return num;
        }

        public virtual void Add(T item, object source)
        {
            Data.Add(item);
        }

        #region IFirstRecord, IEnumerable

        protected IEnumerator<T> Numerator;
        public bool MoveNext()
        {
            RecordNumber++;
            if (Numerator == null || !Numerator.MoveNext())
                return false;

            Current = Numerator.Current;
            if (RecordNumber == 0)
                First = Current;
            return Current != null;
        }

        public void Reset()
        {
            RecordNumber = -1;
            Current = null;
            First = null;
            Numerator = Data.GetEnumerator();
        }

        public bool Prepare()
        {
            Reset();
            return First != null;
        }

        #region Properties

        public IEnumerator<T> GetEnumerator() { return Data.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public int RecordNumber { get; private set; }
        public bool Any() { return First != null; }
        public T Current { get; private set; }
        public T First { get; private set; }

        public void Dispose() { }

        object IEnumerator.Current { get { return Current; } }

        public Exception LastError { get; set; }

        #endregion

        public IFirstEnumerable<T> Worker { get { return this; } set { } }

        public ICollection<T> IntoCollection() { return Data; }

        #endregion
    }

}