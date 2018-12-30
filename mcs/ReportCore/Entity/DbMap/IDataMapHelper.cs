using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Entity
{
    public interface IDataMapHelper<T> : IDataMapHelper, IDisposable
    {
        // IDataMapHelper<T> GetProperties(DbDataReader dataReader);
        IDataMapHelper<T> GetProperties(DbDataReader dataReader, Action<Exception> onDublicateField);
        Type Type { get; }  // { get { return typeof(T); } }
        int RecordNumber { get; }

        DbDataReader Reader { get; }
        // DbDataReader IDataMapHelper<T>.Reader { get { return null; }} // no reader available

        T SetValues(DbDataReader dataReader, object[] objVal);
        // Tuple<int[], PropertyInfo[]> GetMap();
    }

    public interface IDataMapHelper
    {
        SqlField[] GetFields(DbDataReader dataReader);

        object[] DbRecordArray();
        object[] DbRecordArray(int len);
    }

    public interface IDataMapHelperSet<T> : IDataMapHelper<T>
    {
        T SetValues(object[] objVal);

        int? GetOrdinal(string columnName);
        object GetField(string columnName, object[] arrayItem);
    }

}
