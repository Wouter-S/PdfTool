using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PfdTool
{
    public class SerialSettings
    {
        public bool Enabled { get; set; }
        public string ReaderPort { get; set; }
        public string WriterPort { get; set; }
    }
}
