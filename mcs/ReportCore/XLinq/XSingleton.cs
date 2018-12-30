using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.XLinq
{

#if NET40CL || UNIX
    internal interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        int Count { get; }
    }

#elif NET45 || NETCORE
    internal interface IReadOnlyCollection<T> : System.Collections.Generic.IReadOnlyCollection<T>
    {
    }
#endif

    // http://source.roslyn.io/#Microsoft.CodeAnalysis/InternalUtilities/SpecializedCollections.Singleton.Collection%25601.cs,447e422fc2f5a817

    public static class XSingleton
    {
        public sealed class Collection<T> : ICollection<T>, IReadOnlyCollection<T>
        {
            private readonly T _loneValue;

            public Collection(T value)
            {
                _loneValue = value;
            }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                return EqualityComparer<T>.Default.Equals(_loneValue, item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                array[arrayIndex] = _loneValue;
            }

            public int Count
            {
                get { return 1; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator<T>(_loneValue);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class Enumerator<T> : IEnumerator<T>
        {
            public Enumerator(T loneValue)
            {
                First = loneValue;
                Reset();
            }

            T First;
            public T Current { get; protected set; }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (!EqualityComparer<T>.Default.Equals(Current, First))
                {
                    Current = First;
                    return true;
                }
                return false;
            }

            public void Reset() { Current = default(T); }
            public void Dispose() { Reset(); }
        }
    }
}