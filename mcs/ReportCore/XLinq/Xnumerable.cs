using Mono.XHtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Mono.XLinq
{
    // Selected of Linq.Enumerable methods, null safe

    public static class Xnumerable
    {
        public static IEnumerable<T> XWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source == null ? null : System.Linq.Enumerable.Where<T>(source, predicate);
            // static IEnumerable<TSource> Where<TSource> (this IEnumerable<TSource> source);
        }

        public static IEnumerable<T> XWhereN<T>(this IEnumerator<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                yield break;

            while (source.MoveNext() && source.Current != null)
                if (predicate(source.Current))
                    yield return source.Current;
        }

        public static IEnumerable<T> XWhereIN<T>(this IEnumerator source, Func<T, bool> predicate)
        {
            if (source == null)
                yield break;

            while (source.MoveNext() && source.Current != null && source is T)
            {
                T value = (T)source.Current;
                if (value != null && predicate(value))
                    yield return value;
            }

            if (source is IDisposable)
                (source as IDisposable).Dispose();
        }

        public static bool XAny<T>(this IEnumerable<object> source)
        {
            if (source == null)
                return false;
            using (var n = source.GetEnumerator())
            {
                if (n == null)
                    return false;
                if (n.MoveNext() && n.Current != null && n is T)
                    return true;
            }
            return false;
        }

        public static string XValue<T>(this XElement source)
        {
            return source != null ? source.Value : string.Empty;
        }

        public static T XValue<T>(this XNode source)
        {
            T result = default(T);
            if (source == null)
                return result;

            // var conv = new Converter<string, T>((source as XElement).Value);
            result = (T)Convert.ChangeType((source as XElement).Value, typeof(T));
            return result;
        }

        public static IEnumerable<T> XSelect<T>(this IEnumerable source, Func<object, T> select)
        {
            if (source == null)
                yield break;

            var num = source.GetEnumerator();
            while (num.MoveNext() && num.Current != null)
            {
                T value = select(num.Current);
                if (value != null)
                    yield return value;
            };

            if (num is IDisposable)
                (num as IDisposable).Dispose();
        }

        public static IEnumerable<TResult> XSelect<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        {
            return source == null ? null : System.Linq.Enumerable.Select<T, TResult>(source, selector);
        }

        public static IEnumerable<TResult> XSelectWhere<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        {
            if (source == null)
                yield break;

            using (var num = source.GetEnumerator())
            {
                if (num == null)
                    yield break;

                while (num.MoveNext())
                {
                    var filter = selector(num.Current);
                    if (filter != null)
                        yield return filter;
                }
            }
        }


        // IEnumerable<TResult> OfType<TResult>(this IEnumerable source);
        //static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count);
        //static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate);

        public static IEnumerable<TSource> XOrderBy<TSource, TKey>(this IEnumerable<TSource> source,
               Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

    }
}
