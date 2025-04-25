using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadDownloader
{
    [Serializable]
    public class ThreadState
    {
        public long Start {  get; set; }
        public long End { get; set; }
        public long Accumulated { get; set; }
        public long ProgressAbsolute { get; set; }//the one that contains the lenght of bytes to continue
        public string Filename { get; set; }
    }
}
