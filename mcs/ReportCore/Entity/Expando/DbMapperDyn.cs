using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Mono.Entity
{
    public class DbMapperDyn : DbDataMapHelper<ExpandoObject>, IDataMapHelper
    {
        public DbMapperDyn(DbDataReader dataReader)
            : base()
        {
            fields = SqlFieldArray.GetFields(dataReader);
        }

        public virtual dynamic Get(object[] objVal)
        {
            IDictionary<string, object> val = new ExpandoObject();

            foreach (KeyValuePair<string, SqlFieldInfo> pair in fields)
            {
                object fieldValue = objVal[pair.Value.Ordinal].Equals(DBNull.Value) ? null :
                              objVal[pair.Value.Ordinal];
                val.Add(new KeyValuePair<string, object>(pair.Key, fieldValue));
            }

            return val as ExpandoObject;
        }
    }

}
