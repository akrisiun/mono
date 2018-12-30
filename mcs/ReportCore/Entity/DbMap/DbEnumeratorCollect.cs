using Mono.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class DbEnumeratorCollect : Collection<object>, IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }

    public class DbEnumeratorCollect<T> : DbEnumeratorCollect, ICollection<T>
        where T : class 
    {
        public static DbEnumeratorCollect<T> FromEnum(IEnumerable<T> numerator)
        {
            var collect = new DbEnumeratorCollect<T>();
            foreach (var item in numerator)
            {
                if (item == null)
                    break;
                collect.Add(item);
            }
            return collect;
        }

        public static DbEnumeratorCollect<T> FromNumerator(IEnumerator<T> numerator)
        {
            var collect = new DbEnumeratorCollect<T>();
            while (numerator.MoveNext())
            {
                if (numerator.Current == null)
                    break;
                collect.Add(numerator.Current);
            }
            return collect;
        }

        public static DbEnumeratorCollect<T> FromEach(IEnumerator<object[]> numerator, Func<object[], T> each)
        {
            var collect = new DbEnumeratorCollect<T>();
            while (numerator.MoveNext())
            {
                T value = each(numerator.Current);
                if (value == null)
                    continue;
                collect.Add(value);
            }
            return collect;
        }

        public IEnumerable<ExpandoObject> AsExpando()
        {
            var numerator = base.GetEnumerator();
            while (numerator.MoveNext())
            {
                T objItem = (T)numerator.Current;
                ExpandoObject obj = ExpandoConvert.Muttable(objItem);
                if (obj == null)
                    yield break;

                yield return obj;
            }
        }

        public void Add(T item)
        {
            base.Add(item);
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Guard.Check(arrayIndex <= 0);
            array = Enumerable.ToArray<T>(this);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            // IEnumerable
            var numerable = base.GetEnumerator();
            while (numerable.MoveNext())
            {
                T value = numerable.Current as T;
                if (value != null)
                    yield return value;
            }
            //  return numerable == null ? null : numerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}
