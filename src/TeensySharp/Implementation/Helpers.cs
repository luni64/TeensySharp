using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lunOptics.libTeensySharp.Implementation
{
    public static class Helpers
    {
        // port needs time after last close before it can be opened again
    //    static public bool SafeOpen(SerialPort p, TimeSpan timeout)
    //    {
    //        if (p == null) return false;

    //       Trace.Write($"SafeOpen {p.PortName} ");
            
    //        bool ok = false;
    //        var endTime = DateTime.Now + timeout;
    //        while (!ok && DateTime.Now < endTime) 
    //        {
    //            try
    //            {
    //                if (p.IsOpen)
    //                {
    //                    ok = false;
    //                    break;
    //                }

    //                else p.Open();
    //                ok = true;
    //            }
    //            catch 
    //            {
    //                Trace.WriteLine("");
    //                Thread.Sleep(10);
    //            }
    //        }
    //        Trace.WriteLine(ok);
    //        return ok;
    //    }
    }
}
