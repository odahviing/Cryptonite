using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonite
{
    internal class Settings
    {
        internal static void Load() { }
        internal static void Save() { }
    }

    internal static class Debug
    {
        // All True == Production
        internal static readonly Boolean LoadSettings = true;
        internal static readonly Boolean CreateFiles = true;
        

        // Limitation Flags (most of the time - all false == production)
        internal static readonly Boolean OneFileLimit = false;
        internal static readonly Boolean DeleteSettingsFile = false;

        // For Debug Only
        internal static readonly Boolean StayRunning = true;
    }
}
