using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scratcher
{
    class actionBtn : System.Windows.Forms.Button
    {
        public string Command;
        public string args;
        public System.Diagnostics.Process process;
    }
}
