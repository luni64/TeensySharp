﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace lunOptics.libTeensySharp.Native
{
    internal static class NativeMethods
    {
        //========================================================================================================================================
        // CfgMgr32.h
        // https://docs.microsoft.com/en-us/windows/win32/devinst/cfgmgr32-h
        //========================================================================================================================================

        #region pinvoke -----------------------------------------------------
        //CM_Get_Device_Interface_List
        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        public static extern CR CM_Get_Device_Interface_List(
        ref Guid interfaceClassGuid,
            string deviceID,
            byte[] buffer,
            int bufferLength,
            CM_GET_DEVICE_INTERFACE_LIST flags
        );

        #endregion

        #region Structs and Flags -------------------------------------------
     

        [Flags]
        public enum CM_LOCATE_DEVNODE : uint
        {
            NORMAL = 0x00000000,
            PHANTOM = 0x00000001,
            CANCELREMOVE = 0x00000002,
            NOVALIDATION = 0x00000004,
            BITS = 0x00000007
        }

        [Flags]
        public enum CM_REENUMERATE : uint
        {
            NORMAL = 0x00000000,
            SYNCHRONOUS = 0x00000001,
            RETRY_INSTALLATION = 0x00000002,
            ASYNCHRONOUS = 0x00000004,
            BITS = 0x00000007
        }

        [Flags]
        public enum CM_GETIDLIST : uint
        {
            FILTER_NONE = 0x00000000,
            FILTER_ENUMERATOR = 0x00000001,
            FILTER_SERVICE = 0x00000002,
            FILTER_EJECTRELATIONS = 0x00000004,
            FILTER_REMOVALRELATIONS = 0x00000008,
            FILTER_POWERRELATIONS = 0x00000010,
            FILTER_BUSRELATIONS = 0x00000020,
            DONOTGENERATE = 0x10000040,
            FILTER_TRANSPORTRELATIONS = 0x00000080,
            FILTER_PRESENT = 0x00000100,
            FILTER_CLASS = 0x00000200,
            FILTER_BITS = 0x100003FF,
        }

        [Flags]
        public enum CM_GET_DEVICE_INTERFACE_LIST : uint
        {
            PRESENT = 0,
            ALL_DEVICES = 1,
        }

        #endregion


        //========================================================================================================================================
        // kernel32.dll
        // https://docs.microsoft.com/en-us/windows/win32/devinst/cfgmgr32-h
        //========================================================================================================================================

        #region pinvoke -----------------------------------------------------

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        #endregion

        #region Structs and Flags -------------------------------------------

        [Flags]
        public enum EFileAccess : uint
        {
            AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
            MaximumAllowed = 0x2000000,     // MaximumAllowed access type

            Delete = 0x10000,
            ReadControl = 0x20000,
            WriteDAC = 0x40000,
            WriteOwner = 0x80000,
            Synchronize = 0x100000,

            StandardRightsRequired = 0xF0000,
            StandardRightsRead = ReadControl,
            StandardRightsWrite = ReadControl,
            StandardRightsExecute = ReadControl,
            StandardRightsAll = 0x1F0000,
            SpecificRightsAll = 0xFFFF,

            FILE_READ_DATA = 0x0001,        // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,       // directory
            FILE_WRITE_DATA = 0x0002,       // file & pipe
            FILE_ADD_FILE = 0x0002,         // directory
            FILE_APPEND_DATA = 0x0004,      // file
            FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
            FILE_READ_EA = 0x0008,          // file & directory
            FILE_WRITE_EA = 0x0010,         // file & directory
            FILE_EXECUTE = 0x0020,          // file
            FILE_TRAVERSE = 0x0020,         // directory
            FILE_DELETE_CHILD = 0x0040,     // directory
            FILE_READ_ATTRIBUTES = 0x0080,      // all
            FILE_WRITE_ATTRIBUTES = 0x0100,     // all

            //
            // Generic Section
            //

            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,

            SPECIFIC_RIGHTS_ALL = 0x00FFFF,
            FILE_ALL_ACCESS =
            StandardRightsRequired |
            Synchronize |
            0x1FF,

            FILE_GENERIC_READ =
            StandardRightsRead |
            FILE_READ_DATA |
            FILE_READ_ATTRIBUTES |
            FILE_READ_EA |
            Synchronize,

            FILE_GENERIC_WRITE =
            StandardRightsWrite |
            FILE_WRITE_DATA |
            FILE_WRITE_ATTRIBUTES |
            FILE_WRITE_EA |
            FILE_APPEND_DATA |
            Synchronize,

            FILE_GENERIC_EXECUTE =
            StandardRightsExecute |
              FILE_READ_ATTRIBUTES |
              FILE_EXECUTE |
              Synchronize
        }

        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            ///
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access.
            /// Otherwise, other processes cannot open the object if they request read access.
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access.
            /// Otherwise, other processes cannot open the object if they request write access.
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access.
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }
        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always.
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always.
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right.
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }
        #endregion


        //========================================================================================================================================
        // Hid.dll
        // https://docs.microsoft.com/de-de/windows-hardware/drivers/hid/introduction-to-hid-concepts
        //========================================================================================================================================

        [DllImport("Hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool HidD_GetSerialNumberString(
            SafeFileHandle h, 
            byte[] buf, 
            int bufLen
        );

        [DllImport("Hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool HidD_SetFeature(
            SafeFileHandle h,
            byte[] buf,
            int bufLen
        );
        
        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool HidD_SetOutputReport(
          SafeFileHandle h,
          byte[] buf,
          int bufLen);



    }


    //internal static class NativeMethodsOld
    //{
    //    //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    //    //public static extern SafeFileHandle CreateFile(
    //    //    string lpFileName,
    //    //    EFileAccess dwDesiredAccess,
    //    //    EFileShare dwShareMode,
    //    //    IntPtr lpSecurityAttributes,
    //    //    ECreationDisposition dwCreationDisposition,
    //    //    EFileAttributes dwFlagsAndAttributes,
    //    //    IntPtr hTemplateFile);


    //    //[Flags]
    //    //public enum EFileAccess : uint
    //    //{
    //    //    AccessSystemSecurity = 0x1000000,   // AccessSystemAcl access type
    //    //    MaximumAllowed = 0x2000000,     // MaximumAllowed access type

    //    //    Delete = 0x10000,
    //    //    ReadControl = 0x20000,
    //    //    WriteDAC = 0x40000,
    //    //    WriteOwner = 0x80000,
    //    //    Synchronize = 0x100000,

    //    //    StandardRightsRequired = 0xF0000,
    //    //    StandardRightsRead = ReadControl,
    //    //    StandardRightsWrite = ReadControl,
    //    //    StandardRightsExecute = ReadControl,
    //    //    StandardRightsAll = 0x1F0000,
    //    //    SpecificRightsAll = 0xFFFF,

    //    //    FILE_READ_DATA = 0x0001,        // file & pipe
    //    //    FILE_LIST_DIRECTORY = 0x0001,       // directory
    //    //    FILE_WRITE_DATA = 0x0002,       // file & pipe
    //    //    FILE_ADD_FILE = 0x0002,         // directory
    //    //    FILE_APPEND_DATA = 0x0004,      // file
    //    //    FILE_ADD_SUBDIRECTORY = 0x0004,     // directory
    //    //    FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
    //    //    FILE_READ_EA = 0x0008,          // file & directory
    //    //    FILE_WRITE_EA = 0x0010,         // file & directory
    //    //    FILE_EXECUTE = 0x0020,          // file
    //    //    FILE_TRAVERSE = 0x0020,         // directory
    //    //    FILE_DELETE_CHILD = 0x0040,     // directory
    //    //    FILE_READ_ATTRIBUTES = 0x0080,      // all
    //    //    FILE_WRITE_ATTRIBUTES = 0x0100,     // all

    //    //    //
    //    //    // Generic Section
    //    //    //

    //    //    GenericRead = 0x80000000,
    //    //    GenericWrite = 0x40000000,
    //    //    GenericExecute = 0x20000000,
    //    //    GenericAll = 0x10000000,

    //    //    SPECIFIC_RIGHTS_ALL = 0x00FFFF,
    //    //    FILE_ALL_ACCESS =
    //    //    StandardRightsRequired |
    //    //    Synchronize |
    //    //    0x1FF,

    //    //    FILE_GENERIC_READ =
    //    //    StandardRightsRead |
    //    //    FILE_READ_DATA |
    //    //    FILE_READ_ATTRIBUTES |
    //    //    FILE_READ_EA |
    //    //    Synchronize,

    //    //    FILE_GENERIC_WRITE =
    //    //    StandardRightsWrite |
    //    //    FILE_WRITE_DATA |
    //    //    FILE_WRITE_ATTRIBUTES |
    //    //    FILE_WRITE_EA |
    //    //    FILE_APPEND_DATA |
    //    //    Synchronize,

    //    //    FILE_GENERIC_EXECUTE =
    //    //    StandardRightsExecute |
    //    //      FILE_READ_ATTRIBUTES |
    //    //      FILE_EXECUTE |
    //    //      Synchronize
    //    //}

    //    //[Flags]
    //    //public enum EFileShare : uint
    //    //{
    //    //    /// <summary>
    //    //    ///
    //    //    /// </summary>
    //    //    None = 0x00000000,
    //    //    /// <summary>
    //    //    /// Enables subsequent open operations on an object to request read access.
    //    //    /// Otherwise, other processes cannot open the object if they request read access.
    //    //    /// If this flag is not specified, but the object has been opened for read access, the function fails.
    //    //    /// </summary>
    //    //    Read = 0x00000001,
    //    //    /// <summary>
    //    //    /// Enables subsequent open operations on an object to request write access.
    //    //    /// Otherwise, other processes cannot open the object if they request write access.
    //    //    /// If this flag is not specified, but the object has been opened for write access, the function fails.
    //    //    /// </summary>
    //    //    Write = 0x00000002,
    //    //    /// <summary>
    //    //    /// Enables subsequent open operations on an object to request delete access.
    //    //    /// Otherwise, other processes cannot open the object if they request delete access.
    //    //    /// If this flag is not specified, but the object has been opened for delete access, the function fails.
    //    //    /// </summary>
    //    //    Delete = 0x00000004
    //    //}
    //    //public enum ECreationDisposition : uint
    //    //{
    //    //    /// <summary>
    //    //    /// Creates a new file. The function fails if a specified file exists.
    //    //    /// </summary>
    //    //    New = 1,
    //    //    /// <summary>
    //    //    /// Creates a new file, always.
    //    //    /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
    //    //    /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
    //    //    /// </summary>
    //    //    CreateAlways = 2,
    //    //    /// <summary>
    //    //    /// Opens a file. The function fails if the file does not exist.
    //    //    /// </summary>
    //    //    OpenExisting = 3,
    //    //    /// <summary>
    //    //    /// Opens a file, always.
    //    //    /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
    //    //    /// </summary>
    //    //    OpenAlways = 4,
    //    //    /// <summary>
    //    //    /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
    //    //    /// The calling process must open the file with the GENERIC_WRITE access right.
    //    //    /// </summary>
    //    //    TruncateExisting = 5
    //    //}

    //    //[Flags]
    //    //public enum EFileAttributes : uint
    //    //{
    //    //    Readonly = 0x00000001,
    //    //    Hidden = 0x00000002,
    //    //    System = 0x00000004,
    //    //    Directory = 0x00000010,
    //    //    Archive = 0x00000020,
    //    //    Device = 0x00000040,
    //    //    Normal = 0x00000080,
    //    //    Temporary = 0x00000100,
    //    //    SparseFile = 0x00000200,
    //    //    ReparsePoint = 0x00000400,
    //    //    Compressed = 0x00000800,
    //    //    Offline = 0x00001000,
    //    //    NotContentIndexed = 0x00002000,
    //    //    Encrypted = 0x00004000,
    //    //    Write_Through = 0x80000000,
    //    //    Overlapped = 0x40000000,
    //    //    NoBuffering = 0x20000000,
    //    //    RandomAccess = 0x10000000,
    //    //    SequentialScan = 0x08000000,
    //    //    DeleteOnClose = 0x04000000,
    //    //    BackupSemantics = 0x02000000,
    //    //    PosixSemantics = 0x01000000,
    //    //    OpenReparsePoint = 0x00200000,
    //    //    OpenNoRecall = 0x00100000,
    //    //    FirstPipeInstance = 0x00080000
    //    //}
    //}


}
