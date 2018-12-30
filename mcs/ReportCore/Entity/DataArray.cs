using Mono.Report;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Mono.Entity
{
    public interface IDataArray : IData, IList<object[]>, IEnumerable<object[]>, IEnumerator<object[]>, IDisposable
    {
        // IEnumerable<object[]> Array { get; }
        void ReadSource(IEnumerable<object[]> source);
        XDocument GetXml(string[] names);

        object[] GetItem(int index);
    }

    public static class DataArrayExt
    {
        // public 
        internal static IEnumerable<T> Column<T>(this DataArray source, int col = 0) where T : IConvertible
        {
            return ReadColumnObj<T>(source, col);
        }

        public static IEnumerable<T> ReadColumnObj<T>(IEnumerable<object[]> source, int col = 0) where T : IConvertible
        {
            foreach (object[] item in source)
            {
                if (item != null && item.Length > col)
                    yield return (T)Convert.ChangeType(item[col], typeof(T));
            }
        
        }
    }

    public static class DataArrayMethods
    {
        public static IEnumerator<object[]> 
            NotEmpty(this IFirstRecord<object[]> source, int skip = 0, int minEmptyRows = 5)
        {
            if (!source.Any() && !source.Prepare())
                yield break;

            var numerator = source; // .GetEnumerator();
            bool isNext = numerator.MoveNext();
            int index = 0;
            if (!isNext)
                yield break;

            if (skip > 0)
            {
                skip--;
                while (numerator.MoveNext() && skip > 0)
                {
                    index++;
                    skip--;
                }
                if (skip > 0)
                    yield break;

                isNext = numerator.MoveNext();
            }

            if (minEmptyRows == 0)
            {
                while (isNext)
                {
                    yield return numerator.Current;
                    isNext = numerator.MoveNext();
                }

                yield break;
            }

            int pushIndex = 0;
            int popIndex = -1;
            bool isEmpty = false;
            int foundEmpty = 0;
            Dictionary<int, object[]> cache = new Dictionary<int, object[]>();

            while (isNext)
            {
                isEmpty = numerator.Current == null || numerator.Current.Length == 0;
                if (!isEmpty)
                {
                    var item = numerator.Current;
                    cache.Add(pushIndex, item);
                    pushIndex++;
                }
                else
                    foundEmpty++;
            
                isNext = numerator.MoveNext();
                index++;

                // popIndex
                if (pushIndex - minEmptyRows > 0)
                {
                    popIndex++;
                    if (cache.ContainsKey(popIndex))
                    {
                        yield return cache[popIndex];
                        cache.Remove(key: popIndex);
                    }
                }
                if (foundEmpty >= minEmptyRows)
                    break;
            }

            // rest items
            while (popIndex < pushIndex)
            { 
                if (cache.ContainsKey(popIndex))
                    yield return cache[popIndex];
                popIndex++;            
            }

        }
    }

    // non internal
    public class XDataArray : DataArray, IDataArray
    { }

    // internal 
    public class DataArray : IDataArray, IData, IFirstRecord<object[]>, IDisposable
    {
        #region ctor
        protected List<object[]> array;

        static DataArray()
        {
            Empty = new DataArray();
            EmptyRecord = new object[] { null };
        }
        public static DataArray Empty;
        public static object[] EmptyRecord;

        public DataArray()
        {
            array = new List<object[]>();
        }
        
        void IDisposable.Dispose() {
            array = null;
        }
        #endregion

        public virtual void ReadSource(IEnumerable<object[]> source)
        {
            foreach(var item in source)
               array.Add(item);
        }
      
        public IList<object[]> Array { get { return array; } }
        
        IEnumerable<object[]> IData.Array { get { return array; } }

        #region IList

        public int IndexOf(object[] item)
        {
            return array.IndexOf(item);
        }

        public void Insert(int index, object[] item)
        {
            array.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            array.RemoveAt(index);
        }

        public object ItemCol(int row, int col) => this[row][col];
        public object[] GetItem(int index) => this[index];
        public object[] this[int index]
        {
            get
            {
                if (array == null || array.Count <= index)
                    return EmptyRecord;
                return array[index];
            }
            set
            {
                array[index] = value;
            }
        }

        public void Add(object[] item)
        {
            array.Add(item);
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(object[] item)
        {
            return array.Contains(item);
        }

        public void CopyTo(object[][] arrayResult, int arrayIndex)
        {
            array.CopyTo(arrayResult, arrayIndex);
        }

        public int Count
        {
            get { return array.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(object[] item)
        {
            return array.Remove(item);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return array.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IEnumerator 
        public virtual bool Prepare()
        {
            if (numerator != null && First != null)
                return true;
            
            Reset();
            if (First == null)
                MoveNext();

            return numerator != null && First != null;
        }

        object IEnumerator.Current { get { return Current; } }
        public object[] Current { get; private set; }

        public int RecordNumber { get; private set; }
        public object[] First { get; private set; }
        public bool Any() {
            if (First != null) return true;
            if (numerator == null && !Prepare())
                return false;
            return numerator != null && numerator.Current != null; 
        }

        protected IEnumerator<object[]> numerator;
        public virtual void Reset()
        {
            First = null;
            Current = null;
            RecordNumber = -1;
            numerator = Array.GetEnumerator();
        }
        public bool MoveNext()
        {
            if (numerator == null)
                return false;
            if (!numerator.MoveNext())
                return false;
            Current = numerator.Current;
            RecordNumber++;
            if (RecordNumber == 0) // First == null || First.Length == 0)
                First = Current;
            return true;
        }

        #endregion

        public virtual XDocument GetXml(string[] names)
        {
            var doc = new XDocument(new XElement(names[(int)IdxNames.Root]));
            foreach (object[] row in Array)
            {
                if (row == null || row.Length == 0)
                    continue;

                XElement rowEl = new XElement(names[(int)IdxNames.Row]);
                for (int i = 0; i < row.Length; i++)
                {
                    var el = new XElement(names[(int)IdxNames.FirstField + i]);

                    if (row[i] is string 
                        && (row[i].ToString().Contains("</") || row[i].ToString().Contains("/>")))
                        el.Add(XElement.Parse(row[i] as string));
                    else 
                        el.SetValue(row[i]);
                    
                    rowEl.Add(el);
                }
                doc.Root.Add(rowEl);
            }
            return doc;
        }

        public enum IdxNames : int { 
            Root = 0,
            Row = 1,
            FirstField = 2
        }

        public Exception LastError { get; set; }

        public static DataArray Read(IEnumerable<object[]> array)
        {
            var data = new DataArray();
            data.ReadSource(array);
            return data;
        }

        public virtual void Dispose()
        {
            array = null;
        }

    }

    public class DataArrayFields : IEnumerable<object[]>, ILastError // IDataArray, 
    {
        public DataArrayFields(object array = null)
        {
            Array = array as DataArray ?? new DataArray();
        }

        public static DataArray Data(DataArrayFields fields) => fields?.Array;

        public Exception LastError { get; set; }
        internal DataArray Array { get; set; }
        public SqlField[] Fields { get; set; }

        public void Dispose() => Array?.Dispose();
        public void Add(object[] item) => Array.Add(item);
        public void Clear() => Array.Clear();
        public bool Contains(object[] item) => Array.Contains(item);

        public void CopyTo(object[][] arrayResult, int arrayIndex)
        {
            Array.CopyTo(arrayResult, arrayIndex);
        }

        public int Count { get => (int?)Array?.Count ?? 0; }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(object[] item) => Array?.Remove(item) ?? false;

        public IEnumerator<object[]> GetEnumerator() => Array?.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Array?.GetEnumerator();

        public object[] Current { get => Array?.Current; }
        public bool MoveNext() => Array?.MoveNext() ?? false;
        public void Reset() => Array?.Reset();

        public XDocument GetXml(string rootName = "Root")
        {
            string[] names = new string[] { rootName };
            System.Array.Resize<string>(ref names, 1 + Fields.Length);
            for (int i = 0; i < Fields.Length; i++)
                names[1 + i] = Fields[i].Caption;

            return Array?.GetXml(names);
        }
    }

}
