using BetterWin32Errors;
using lunOptics.LibUsbTree.Implementation;

using System;
using System.Runtime.InteropServices;
using static lunOptics.LibUsbTree.DevicePropertyKeys;
using static lunOptics.LibUsbTree.Implementation.NativeMethods;
using static System.Globalization.CultureInfo;


namespace lunOptics.LibUsbTree
{
    public sealed class DeviceInfo : IDisposable
    {
        #region public fields -------------------------------------------
        public Guid deviceClassGuid { get; }
        public string deviceInstanceID { get; }
        public int vid { get; }
        public int pid { get; }
        public bool isInterface { get; }
        #endregion

        #region public methods ------------------------------------------
        public T getProperty<T>(DEVPROPKEY key)
        {
            if (!SetupDiGetDevicePropertyW(deviceInfoSet, ref deviceInfoData, ref key, out ulong propertyType, buffer, bufSize, out int requiredSize, 0))
            {
                throw new Win32Exception("getProperty");
            }
             
            if (typeof(T) == typeof(string))    return (T)(object)Marshal.PtrToStringAuto(buffer);
            else if (typeof(T) == typeof(Guid)) return (T)(object)Marshal.PtrToStructure<T>(buffer);

            return default;
        }
        #endregion

        #region construction / deconstruction ---------------------------

        internal DeviceInfo(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData)
        {
            buffer = Marshal.AllocHGlobal(bufSize);
            this.deviceInfoSet = deviceInfoSet;
            this.deviceInfoData = deviceInfoData;

            try
            {
                deviceInstanceID = getProperty<string>(DeviceInstanceID).ToUpper(InvariantCulture); ;
                deviceClassGuid = getProperty<Guid>(DeviceClassGuid);

                var devInstId_parts = deviceInstanceID.Split('\\');
                var device_parts = devInstId_parts[1].Split('&');

                if (device_parts.Length >= 2)
                {
                    vid = Convert.ToInt32(device_parts[0].Substring(4, 4), 16);
                    pid = Convert.ToInt32(device_parts[1].Substring(4, 4), 16);
                }
                isInterface = device_parts.Length == 3;  // interfaces have &MI_NN added to the vid/pid part
            }
            catch (Win32Exception) { throw; }
            catch (Exception innerException)
            {
                throw new UsbTreeException($"Error parsing DeviceInstanceId {deviceInstanceID}", innerException);
            }
        }

        public void Dispose()
        {
            //Debug.WriteLine("Dispose");
            Marshal.FreeHGlobal(buffer);
        }
        #endregion

        #region private fields and methods ------------------------------ 
        private const int bufSize = 1024;
        private readonly IntPtr buffer;

        private readonly IntPtr deviceInfoSet;
        private SP_DEVINFO_DATA deviceInfoData;
        #endregion
    }
}

