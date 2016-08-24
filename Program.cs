using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cryptonite
{

    class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            if (ProcessManager.amIRunning() == true) Helper.Exit(ExitCodes.EXE_ALREADY_RUNNING); else Helper.Print("Start Process Running");
#endif
            
            FilesSensor Sen = new FilesSensor();
            Sen.Start();

            /*
            NetworkCardMitigate nicm = new NetworkCardMitigate();
            nicm.Disable();
            nicm.Enable();
            */
            if (Debug.StayRunning == true) while (true) Thread.Sleep(3000);
        }
    }
}
