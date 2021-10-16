using System;
using System.Collections.Generic;
using System.Text;

using static lunOptics.libUsbTree.Implementation.NativeMethods;

namespace lunOptics.libUsbTree
{
    public static class NativeWrapper
    {
        public static int cmLocateNode(string DevInstId)
        {
            if(CM_Locate_DevNode(out int node, DevInstId, CM_LOCATE_DEVNODE.NORMAL) != CR.SUCCESS) throw new UsbTreeException(nameof(cmLocateNode));
            return node;
        }

        public static string cmGetDevInstIdFromNode(int node, int bufSize = 1024)
        {
            var buffer = new StringBuilder(bufSize);
            if (CM_Get_Device_ID(node, buffer, buffer.Capacity) != CR.SUCCESS) throw new UsbTreeException(nameof(cmGetDevInstIdFromNode));
            return buffer.ToString();
        }

        public static string[] cmGetDevInstIDs(string enumerator)
        {
            if (CM_Get_Device_ID_List_Size(out int chars, enumerator, CM_GETIDLIST.FILTER_ENUMERATOR | CM_GETIDLIST.FILTER_PRESENT) != CR.SUCCESS) return null;

            var buffer = new byte[chars * sizeof(char)];
            if (CM_Get_Device_ID_List(enumerator, buffer, buffer.Length, CM_GETIDLIST.FILTER_ENUMERATOR | CM_GETIDLIST.FILTER_PRESENT) != CR.SUCCESS) return null;

            return Encoding.Unicode.GetString(buffer).Trim('\0').Split('\0');
        }
        
        public static string[] cmGetDeviceInterfaces(string deviceInstanceId, Guid interfaceGuid, int bufSize = 1024 * 10)
        {
            var buffer = new byte[bufSize];
            CR result = CM_Get_Device_Interface_List(ref interfaceGuid, deviceInstanceId, buffer, bufSize, CM_GET_DEVICE_INTERFACE_LIST.PRESENT);
            if (result != CR.SUCCESS) throw new UsbTreeException(nameof(cmGetDeviceInterfaces));

            return Encoding.Unicode.GetString(buffer, 0, bufSize).Trim('\0').Split(new char[] { '\0' },StringSplitOptions.RemoveEmptyEntries);           
        }

        public static int cmGetParentNode(int node)
        {
            var result = CM_Get_Parent(out int parent, node);
            return result == CR.SUCCESS ? parent : -1;
        }

        public static List<int> cmGetChildNodes(int node)
        {
            List<int> ret = new List<int>();

            if(CM_Get_Child(out int sibling, node) == CR.SUCCESS)            
            {
                ret.Add(sibling);                
                while(CM_Get_Sibling(out int nextSibling, sibling) == CR.SUCCESS)                
                {
                    ret.Add(nextSibling);
                    sibling = nextSibling;
                }
            }         
            return ret;
        }
        
        public static Guid cmGetNodePropGuid(int node, DEVPROPKEY key)
        {
            int bufSize = 16;
            var buffer = new byte[bufSize];
            var result = CM_Get_DevNode_PropertyW(node, ref key, out _, buffer, ref bufSize, 0);

            return result == CR.SUCCESS ? new Guid(buffer) : Guid.Empty;            
        }

        public static string cmGetNodePropStrg(int node, DEVPROPKEY key, int bufSize = 1024)
        {
            var buffer = new byte[bufSize];
            var result = CM_Get_DevNode_PropertyW(node, ref key, out _, buffer, ref bufSize, 0);

            return result == CR.SUCCESS ? Encoding.Unicode.GetString(buffer, 0, bufSize).Trim('\0') : null;
        }

        public static List<string> cmGetNodePropStringList(int node, DEVPROPKEY key, int bufSize = 1024 * 5)
        {
            var buffer = new byte[bufSize];
            var result = CM_Get_DevNode_PropertyW(node, ref key, out _, buffer, ref bufSize, 0);

            return result == CR.SUCCESS ? new List<string>(Encoding.Unicode.GetString(buffer, 0, bufSize).Trim('\0').Split('\0')) : new List<string>();
        }

        //static CR result;
    }
}
