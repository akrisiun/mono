using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Mono.Report
{
    public abstract class DataArrayBase : IData
    {
        public abstract IEnumerator<object[]> GetEnumerator();
        public abstract IEnumerable<object[]> Array { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Dispose();
    }
}
