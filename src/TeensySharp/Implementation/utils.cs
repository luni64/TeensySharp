using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace lunOptics.TeensySharp.Implementation
{
    static class Extensions
    {
        public static void ThreadAwareRaise<TEventArgs>(this EventHandler<TEventArgs> customEvent, SynchronizationContext ctx, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            foreach (var d in customEvent.GetInvocationList().OfType<EventHandler<TEventArgs>>())
            {
                if (ctx != null)
                {
                    ctx.Post(s => customEvent.Invoke(sender, e), null);
                }
                else
                {
                    customEvent.Invoke(null, e);
                }
            }
        }
    }
}
