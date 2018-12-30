using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.DataAnnotations.Schema
{
    public class TableAttribute : Attribute 
    {
        public TableAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
