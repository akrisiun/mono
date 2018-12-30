using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class ResultDynFirst : ResultDyn, IFirstRecord<ExpandoObject>
    {
        public ResultDynFirst(ExpandoObject data, SqlField[] fields = null)
        {
            Data = data;
            this.fields = fields;
            base.Reader = null;
            base.Numerator = null;
        }

        private ExpandoObject Data;
        private SqlField[] fields;
        public override SqlField[] Fields { get { return fields; } }
        public override ExpandoObject First { get { return Data; } }

        public override IEnumerator<ExpandoObject> GetEnumerator() { yield return Data; }
        public override bool Prepare() { return true; }
        private int rec = -1;
        public override bool MoveNext() { rec++; return (rec == 0); }
        public override ExpandoObject Current { get { return Data; } }
        public override void Reset() { rec = -1; }

        /*
        public override T CastFirst<T>()
        {
            return DbDataMapHelper.SetExpando<T>(Data);
        }
        public override IEnumerable<T> CastIterator<T>()
        {
            var firstCast = CastFirst<T>();
            yield return firstCast;
        }
        */
    }

    public class ResultDyn : IEnumerable<ExpandoObject>, IEnumerator<ExpandoObject>, IFirstRecord<ExpandoObject>,
        IDisposable, ILastError, IEnumerable
    {
        public static ResultDyn Empty(Exception err)
        {
            return new ResultDyn() { Reader = null, Numerator = null, LastError = err };
        }

        public SqlDataReader Reader { get; set; }
        public SqlConnection Connection { get; set; }
        public DbEnumeratorData Numerator { get; set; }
        public Exception LastError { get; set; }

        public virtual T CastFirst<T>() where T : class
        {
            if (Numerator == null)
                Prepare();
            var numer = Numerator;
            if (numer == null || numer.First == null)
                return null;

            var data = numer.First;
            var mappper = new DbDataMapHelper<T>();
            return mappper.SetValues(data);
        }

        public virtual IList<T> ToList<T>(Func<ExpandoObject, T> convert)
        {
            var list = new List<T>();
            while (MoveNext())
            {
                T value = convert(Current);
                if (value != null)
                    list.Add(value);
            }
            return list;
        }

        /*
        public virtual IEnumerable<T> CastIterator<T>() where T : class
        {
            if (Numerator == null || Reader == null || Reader.IsClosed)
                yield break;
            // return DbDataMapHelper.Empty<T>();

            mappper = new DbDataMapHelper<T>();
            (mappper as DbDataMapHelper<T>).GetProperties(Reader);

            var numer = Numerator.GetEnumerator();
            if (numer != null)
                while (numer.MoveNext() && mappper != null)
            {
                object[] data = numer.Current;
                yield return (mappper as DbDataMapHelper<T>).SetValues(data);
            }
        }
        */

        protected object mappper;
        public int RecordNumber { get { return First != null ? 0 : -1; } }

        public virtual bool Any() { return Numerator != null && Numerator.First != null; }
        public virtual ExpandoObject First
        {
            get
            {
                var first = Numerator == null ? null : Numerator.First as object[];
                var helper = GetHelper();
                return first == null || helper == null ? null : helper.Get(first);
            }
        }

        public virtual bool Prepare()
        {
            if (Numerator == null || Numerator.First == null)
            {
                MoveNext();
                Reset();
            }

            return Numerator != null && Numerator.First != null;
        }

        public virtual SqlField[] Fields
        {
            get
            {
                if (Reader != null && !Reader.IsClosed)
                    return SqlFieldArray.GetArray(Reader);
                return null;
                // helper.GetFields()   // no cache
            }
        }

        public DbMapperDyn GetHelper()
        {
            helper = helper ?? new DbMapperDyn(Reader);
            return helper;
        }
        protected DbMapperDyn helper;

        public virtual void Reset()
        {
            if (Numerator != null && Numerator.Current != null)
            {
                Numerator.Reset();
                Reader = Numerator.Reader as SqlDataReader;
            }
            if (Reader != null)
                helper = helper ?? new DbMapperDyn(Reader);
        }

        public virtual IEnumerator<ExpandoObject> GetEnumerator()
        {
            Reset();
            DbEnumeratorData numerator = Numerator;
            if (numerator == null || numerator.Reader == null || !numerator.MoveNext())
                yield break;

            do
            {
                var rec = numerator.Current as object[];
                if (rec == null || rec.Length == 0)
                    yield break;    // first error

                dynamic obj = helper.Get(rec);
                if (obj != null)
                    yield return obj;
            }
            while (numerator.MoveNext());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Numerator;
        }

        public virtual ExpandoObject Current
        {
            get
            {
                var rec = Numerator == null ? null : Numerator.Current as object[];
                return rec == null || helper == null ? null : helper.Get(rec);
            }
        }

        public void Dispose()
        {
            if (Reader != null)
                Reader.Dispose();
            Reader = null;
            if (Numerator != null)
                Numerator.Dispose();
            Numerator = null;
            helper = null;
        }

        object IEnumerator.Current
        {
            get { return Numerator == null ? null : Numerator.Current; }
        }

        public virtual bool MoveNext()
        {
            if (Numerator == null || !Numerator.MoveNext())
                return false;
            if (helper == null)
            {
                Guard.Check(Reader == Numerator.Reader);
                helper = new DbMapperDyn(Numerator.Reader);
            }
            return helper != null;
        }

    }

}
