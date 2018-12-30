using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.DataAnnotations.Schema
{
    public class ColumnAttribute : Attribute 
    {
        public ColumnAttribute(int Order = -1, string columnName = null)
        {
            if (Order >= 0)
                this.Order = Order;
            Name = columnName;
        }

        public string Name { get; protected set; }
        public int? Order { get; protected set; }
    }

    public class DatabaseGeneratedAttribue : Attribute 
    {
        public DatabaseGeneratedAttribue (object identity = null)
	    {
            Identity = identity;
        	}

        public object Identity {get; protected set; }
            
        // (DatabaseGeneratedOption.Identity)]
 
    }

}
