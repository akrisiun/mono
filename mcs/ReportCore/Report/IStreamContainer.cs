using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mono.Report
{
    public interface IStreamContainer
    {
        void ReadStream(Stream stream, string fileName);
    }
}
