using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonite
{
    class MitigationController
    {
        void BlockNetworkCard() { }
        void UnmountShares() { }
        void ShutdownHost() { }

    }

    abstract class Mitigate
    {
        protected Mitigate(){ Active = false;}
        internal Boolean Active {get;set;}
        internal abstract void Enable();
        internal abstract void Disable();
    }

    class NetworkCardMitigate : Mitigate
    {
        List<String> activeNetworkCards = new List<string>();
        List<String> disabledNetworkCards = new List<string>();
        internal NetworkCardMitigate() : base() { }
        private void getActiveNetworkCards()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                activeNetworkCards.Add(nic.Name);
            }

        }
        internal override void Enable()
        {
            foreach (string interfaceName in disabledNetworkCards)
            {
                ProcessStartInfo psi = new ProcessStartInfo("netsh", String.Format("interface set interface \"{0}\" enable", interfaceName));
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = psi;
#if !DEBUG
                p.Start();
#endif
                Helper.Print("Network card {0} Enabled", interfaceName);
            }

            disabledNetworkCards.Clear();
            Active = false;
        }

        internal override void Disable()
        {
            if (activeNetworkCards.Count() == 0) getActiveNetworkCards();

            foreach (string interfaceName in activeNetworkCards)
            {
                ProcessStartInfo psi = new ProcessStartInfo("netsh", String.Format("interface set interface \"{0}\" disable", interfaceName));
                Process p = new Process();
                p.StartInfo = psi;

#if !DEBUG
                p.Start();
#endif
                Helper.Print("Network card {0} Disabled", interfaceName);

                disabledNetworkCards.Add(interfaceName);
            }

            Active = true;
        }
    }
}
