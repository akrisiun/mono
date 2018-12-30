using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Mono.Entity.Expando
{
    public class ResultList : ResultDyn 
    {
        public IList<ExpandoObject> List { get; private set; }


        public override IEnumerator<ExpandoObject> GetEnumerator() 
        {
            return base.GetEnumerator();
        }
    }
}
