using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Mono.Entity
{
    // IObserver<T> observer)
    // public interface IObj<T> : System.IObservable<T>

    public static class DbGet
    {
        public static DbEnumeratorDataExec<T> Execute<T>(this ISqlProc proc, Context db) where T : class, new()
        {
            SqlCommand cmdSql = proc.CreateCommand() as SqlCommand;

            Func<Tuple<DbDataReader, IDbConnection>> get =
                () =>
                new Tuple<DbDataReader, IDbConnection>(
                    (DbDataReader)proc.ExecuteReader(cmdSql), cmdSql.Connection);

            DbEnumeratorDataExec<T> data = null;
            try
            {
                data = new DbEnumeratorDataExec<T>(get);
                if (data != null && !data.Prepare() && !data.ReaderAvailable)
                    data = null;
            }
            catch (Exception ex) { db.LastError = ex; }

            return data;
        }
    }

    public class DbEnumeratorDataExec<T> : DbEnumeratorData<T>, IFirstRecordWrap<T>, IFirstRecord<T>, IFirstEnumerable<T> where T : class
    {
        public IDictionary<string, object> Header { get; set; }

        public DbEnumeratorDataExec(Func<Tuple<DbDataReader, IDbConnection>> getReader)
            : base(getReader)
        {
            GetHelper = () => new MapItem<T>();
        }

        public override bool Prepare()
        {
            Helper = GetHelper();
            if (!base.Prepare())
                return false;

            if (Reader == null)
                return false;
            if (base.first != null && First == null)
                MapHelper.GetProperties(Reader);
            if (base.first == null)
                MoveNext();

            return First != null;
        }

#if DEBUG
        public override bool MoveNext() { return base.MoveNext(); }
#endif

        public MapItem<T> MapHelper { get { return Helper as MapItem<T>; } }

        public ISqlProc Proc { get; set; }
        public IFirstEnumerable<T> Worker { get { return this; } }
        public Func<T, bool> WhenFilter { get; set; } // allow null

        public virtual ICollection<T> IntoCollection() 
        {
            var filter = WhenFilter;
            return ObservableHelper.IntoCollection<T>(filter, this); 
        }

        public ObservableCollection<T> Filter()
        {
            var filter = WhenFilter;
            return ObservableHelper.IntoCollection<T>(filter, this); 
        }

    }

    public class MapItem<T> : DbDataMapHelper<T>, IDataMapHelper<T>
    {
        public override bool MoveNext() { return false; }
        public override bool Prepare() { return true; }
        public override void Reset() { }

        public override T SetValues(object[] objVal)
        {
            if (this.map == null || this.map.Length == 0)
                return default(T);
            T item = Activator.CreateInstance<T>();
            ObjConverter.FillValues<T>(objVal, item, this.map, this.properties);
            return item;
        }
    }
}
