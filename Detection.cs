using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonite
{
    class DetectionEvent
    {
        DetectionEventsArgs Args;
        internal DetectionEvent(FilesSensor Sensor)
        {
            Sensor.Detection += new FilesSensor.OnDetection(OpenDetection);
        }

        internal void OpenDetection(object sender, DetectionEventsArgs args)
        {
            Args = args;
            Helper.Print("DETECTION: file {0} has been changed", args.fileName);
        }


        void SendAlert() { }
        void RetriveAttackingProcess() { }

    }

    internal class DetectionEventsArgs : EventArgs
    {
        internal String fileName;
        internal String oldKey;
        internal String newKey;
        internal DateTime eventTime;

        internal DetectionEventsArgs(string file, string old, string @new)
            : base()
        {
            fileName = file;
            oldKey = old;
            newKey = @new;
            eventTime = DateTime.Now;
        }
    }

}
