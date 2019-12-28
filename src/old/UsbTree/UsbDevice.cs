using System;
using System.Collections.Generic;
using static System.Globalization.CultureInfo;

namespace lunOptics.LibUsbTree
{
    public class UsbDevice 
    {
        #region public properties -------------------------------------- 

        public string DeviceInstanceId { get;  }
        public string Description { get; protected set; }
        public string SerialnumberString { get; private set; }
        public bool IsUsbInterface { get;  }       
        public Guid ClassGuid { get; private set; }                
        public UsbDevice Parent { get; internal set; }
        public List<UsbDevice> children { get; } = new List<UsbDevice>();
        public int vid { get; internal set; }
        public int pid { get; internal set; }

        #endregion


        #region construction -------------------------------------------

        public UsbDevice(DeviceInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            ClassGuid        = info.deviceClassGuid;
            DeviceInstanceId = info.deviceInstanceID;
            vid              = info.vid;
            pid              = info.pid;
            IsUsbInterface   = info.isInterface;
            Description      = info.getProperty<string>(DevicePropertyKeys.Name);            
            ParentStr        = info.getProperty<string>(DevicePropertyKeys.Parent).ToUpper (InvariantCulture);
            
            SerialnumberString = DeviceInstanceId.Split('\\')[2];
        }

        #endregion
        
        #region internal fields and methods ----------------------------
        internal protected string ParentStr { get; set; }

        #endregion

        public override string ToString()
        {
            return $"{Description} ({vid:X4}/{pid:X4}) #{SerialnumberString}";
        }
    }
}
