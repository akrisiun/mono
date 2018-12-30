using Mono.Entity;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Mono.Report
{
    public interface IDataObj
    {
        IEnumerable<object[]> Array { get; }
    }

    public interface IData : IEnumerable<object[]>, IDisposable
    {
        IEnumerable<object[]> Array { get; }
    }

    public interface IDataArray : IData, IList<object[]>, IEnumerable<object[]>, IDisposable, ILastError
    {
        void ReadSource(IEnumerable<object[]> source);
        XDocument GetXml(string[] names);
    }
}
