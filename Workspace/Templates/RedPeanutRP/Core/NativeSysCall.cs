//
// Author: B4rtik (@b4rtik)
// Project: SharpMiniDump (https://github.com/b4rtik/SharpMiniDump)
// License: BSD 3-Clause
//

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using static RedPeanutAgent.Core.Natives;

namespace RedPeanutAgent.Core
{
    class NativeSysCall
    {
        public struct Delegates
        {
            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate int LdrLoadDll(IntPtr PathToFile,
                UInt32 dwFlags,
                ref Natives.UNICODE_STRING ModuleFileName,
                ref IntPtr ModuleHandle);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool RtlInitUnicodeString(ref UNICODE_STRING DestinationString, [MarshalAs(UnmanagedType.LPWStr)] string SourceString);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
            
        }
    }
}
