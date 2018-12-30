using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mono.Entity
{
    public static class ObservableHelper
    {
        public static ObservableCollection<T>
            IntoCollection<T>(Func<T, bool> whenFilter, IFirstEnumerable<T> worker) where T : class
        {
            if (whenFilter == null)
            {
                var coll = new ObservableCollection<T>(worker as IEnumerable<T>);
                return coll;
            }
            var num = worker.GetEnumerator();
            var collFilt = new ObservableCollection<T>();
            while (num.MoveNext())
            {
                var item = num.Current;
                if (item != null && whenFilter(item))
                    collFilt.Add(item);
            }

            return collFilt;
        }
    }
}
