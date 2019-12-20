using System;
using System.Collections.Generic;
using System.Text;

namespace lunOptics.UsbTree.Implementation
{
    internal static class DeviceClassGuids
    {
        public static Guid HidClass = Guid.Parse("745A17A0-74D3-11D0-B6FE-00A0C90F57DA");
        public static Guid Ports { get; } = Guid.Parse("4D36E978-E325-11CE-BFC1-08002BE10318");

        public static readonly Guid PortsC = Guid.Parse("4D36E978-E325-11CE-BFC1-08002BE10318");
    }
}
