using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Cryptonite
{
    class ProcessManager
    {
        internal static Boolean amIRunning()
        {
            UInt16 instances = (UInt16)Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(
                System.Reflection.Assembly.GetEntryAssembly().Location)).Count();

            return (instances == 1) ? false : true;
        
        }
    }
}
