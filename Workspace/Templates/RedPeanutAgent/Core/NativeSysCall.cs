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
        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x0f
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bZwClose10 = { 0x49, 0x89, 0xCA, 0xB8, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x3A
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bZwWriteVirtualMemory10 = { 0x49, 0x89, 0xCA, 0xB8, 0x3A, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x50
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bZwProtectVirtualMemory10 = { 0x49, 0x89, 0xCA, 0xB8, 0x50, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x36
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bZwQuerySystemInformation10 = { 0x49, 0x89, 0xCA, 0xB8, 0x36, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x18
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bNtAllocateVirtualMemory10 = { 0x49, 0x89, 0xCA, 0xB8, 0x18, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x1E
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bNtFreeVirtualMemory10 = { 0x49, 0x89, 0xCA, 0xB8, 0x1E, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        /// 0:  49 89 ca                mov r10,rcx
        /// 3:  b8 0f 00 00 00          mov eax,0x55
        /// 8:  0f 05                   syscall
        /// a:  c3                      ret

        static byte[] bNtCreateFile10 = { 0x49, 0x89, 0xCA, 0xB8, 0x55, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 26 00 00 00          mov eax,0x26
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwOpenProcess10 = { 0x49, 0x89, 0xCA, 0xB8, 0x26, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 4a 00 00 00          mov eax,0x4a
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwCreateSection10 = { 0x49, 0x89, 0xCA, 0xB8, 0x4a, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 28 00 00 00          mov eax,0x28
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwMapViewOfSection10 = { 0x49, 0x89, 0xCA, 0xB8, 0x28, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };
        
        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 2a 00 00 00          mov eax,0x2a
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwUnmapViewOfSection10 = { 0x49, 0x89, 0xCA, 0xB8, 0x2a, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 45 00 00 00          mov eax,0x45
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwQueueApcThread10 = { 0x49, 0x89, 0xCA, 0xB8, 0x45, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  4c 8b d1                mov r10,rcx
        ///3:  b8 0d 00 00 00          mov eax,0x0d
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwSetInformationThread10 = { 0x49, 0x89, 0xCA, 0xB8, 0x0d, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };
        //static byte[] bZwSetInformationThread10 = { 0x4c, 0x8b, 0xd1, 0xB8, 0x0d, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        ///0:  49 89 ca                mov r10,rcx
        ///3:  b8 52 00 00 00          mov eax,0x52
        ///8:  0f 05                   syscall
        ///a:  c3                      ret

        static byte[] bZwResumeThread10 = { 0x49, 0x89, 0xCA, 0xB8, 0x52, 0x00, 0x00, 0x00, 0x0F, 0x05, 0xC3 };

        public static NTSTATUS ZwResumeThread10(IntPtr hThread, out ulong PreviousSuspendCount)
        {
            byte[] syscall = bZwResumeThread10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwResumeThread myAssemblyFunction = (Delegates.ZwResumeThread)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwResumeThread));

                    return (NTSTATUS)myAssemblyFunction( hThread, out  PreviousSuspendCount);
                }
            }
        }

        public static NTSTATUS ZwSetInformationThread10(IntPtr hThread, uint ThreadInformationClass, IntPtr ThreadInformation, ulong ThreadInformationLength)
        {
            byte[] syscall = bZwSetInformationThread10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwSetInformationThread myAssemblyFunction = (Delegates.ZwSetInformationThread)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwSetInformationThread));

                    return (NTSTATUS)myAssemblyFunction( hThread,  ThreadInformationClass,  ThreadInformation,  ThreadInformationLength);
                }
            }
        }

        public static NTSTATUS ZwQueueApcThread10(IntPtr hThread, IntPtr pfnAPC, IntPtr dwData)
        {
            byte[] syscall = bZwQueueApcThread10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwQueueApcThread myAssemblyFunction = (Delegates.ZwQueueApcThread)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwQueueApcThread));

                    return (NTSTATUS)myAssemblyFunction( hThread,  pfnAPC,  dwData);
                }
            }
        }

        public static NTSTATUS ZwUnmapViewOfSection10(IntPtr hSection, IntPtr address)
        {
            byte[] syscall = bZwUnmapViewOfSection10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwUnMapViewOfSection myAssemblyFunction = (Delegates.ZwUnMapViewOfSection)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwUnMapViewOfSection));

                    return (NTSTATUS)myAssemblyFunction( hSection,  address);
                }
            }
        }

        public static NTSTATUS ZwMapViewOfSection10(IntPtr section, IntPtr process, ref IntPtr baseAddr, IntPtr zeroBits, IntPtr commitSize, IntPtr stuff, ref IntPtr viewSize, int inheritDispo, uint alloctype, uint protect)
        {
            byte[] syscall = bZwMapViewOfSection10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwMapViewOfSection myAssemblyFunction = (Delegates.ZwMapViewOfSection)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwMapViewOfSection));

                    return (NTSTATUS)myAssemblyFunction( section,  process, ref  baseAddr,  zeroBits,  commitSize,  stuff, ref  viewSize,  inheritDispo,  alloctype,  protect);
                }
            }
        }

        public static NTSTATUS ZwCreateSection10(ref IntPtr section, uint desiredAccess, IntPtr pAttrs, ref LARGE_INTEGER pMaxSize, uint pageProt, uint allocationAttribs, IntPtr hFile)
        {
            byte[] syscall = bZwCreateSection10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwCreateSection myAssemblyFunction = (Delegates.ZwCreateSection)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwCreateSection));

                    return (NTSTATUS)myAssemblyFunction(out section, desiredAccess,  pAttrs,  ref pMaxSize,  pageProt,  allocationAttribs, hFile);
                }
            }
        }

        public static NTSTATUS ZwOpenProcess10(ref IntPtr hProcess, ProcessAccessFlags processAccess, OBJECT_ATTRIBUTES objAttribute, ref CLIENT_ID clientid)
        {
            byte[] syscall = bZwOpenProcess10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwOpenProcess myAssemblyFunction = (Delegates.ZwOpenProcess)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwOpenProcess));

                    return (NTSTATUS)myAssemblyFunction(out hProcess, processAccess, objAttribute, ref clientid);
                }
            }
        }

        public static NTSTATUS ZwClose10(IntPtr handle)
        {
            byte[] syscall = bZwClose10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwClose myAssemblyFunction = (Delegates.ZwClose)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwClose));

                    return (NTSTATUS)myAssemblyFunction(handle);
                }
            }
        }

        public static NTSTATUS ZwWriteVirtualMemory10(IntPtr hProcess, ref IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, ref IntPtr lpNumberOfBytesWritten)
        {
            byte[] syscall = bZwWriteVirtualMemory10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwWriteVirtualMemory myAssemblyFunction = (Delegates.ZwWriteVirtualMemory)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwWriteVirtualMemory));

                    return (NTSTATUS)myAssemblyFunction(hProcess, lpBaseAddress, lpBuffer, nSize, ref lpNumberOfBytesWritten);
                }
            }
        }

        public static NTSTATUS ZwProtectVirtualMemory10(IntPtr hProcess, ref IntPtr lpBaseAddress, ref uint NumberOfBytesToProtect, uint NewAccessProtection, ref uint lpNumberOfBytesWritten)
        {
            byte[] syscall = bZwProtectVirtualMemory10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwProtectVirtualMemory myAssemblyFunction = (Delegates.ZwProtectVirtualMemory)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwProtectVirtualMemory));

                    return (NTSTATUS)myAssemblyFunction(hProcess, ref lpBaseAddress, ref NumberOfBytesToProtect, NewAccessProtection, ref lpNumberOfBytesWritten);
                }
            }
        }

        public static NTSTATUS ZwQuerySystemInformation10(SYSTEM_INFORMATION_CLASS SystemInformationClass, IntPtr SystemInformation, uint SystemInformationLength, ref uint ReturnLength)
        {
            byte[] syscall = bZwQuerySystemInformation10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.ZwQuerySystemInformation myAssemblyFunction = (Delegates.ZwQuerySystemInformation)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.ZwQuerySystemInformation));

                    return (NTSTATUS)myAssemblyFunction(SystemInformationClass, SystemInformation, SystemInformationLength, ref ReturnLength);
                }
            }
        }

        public static NTSTATUS NtAllocateVirtualMemory10(IntPtr hProcess, ref IntPtr BaseAddress, IntPtr ZeroBits, ref IntPtr RegionSize, ulong AllocationType, ulong Protect)
        {
            byte[] syscall = bNtAllocateVirtualMemory10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.NtAllocateVirtualMemory myAssemblyFunction = (Delegates.NtAllocateVirtualMemory)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.NtAllocateVirtualMemory));

                    return (NTSTATUS)myAssemblyFunction(hProcess, ref BaseAddress, ZeroBits, ref RegionSize, AllocationType, Protect);
                }
            }
        }

        public static NTSTATUS NtFreeVirtualMemory10(IntPtr hProcess, ref IntPtr BaseAddress, ref uint RegionSize, ulong FreeType)
        {
            byte[] syscall = bNtFreeVirtualMemory10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.NtFreeVirtualMemory myAssemblyFunction = (Delegates.NtFreeVirtualMemory)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.NtFreeVirtualMemory));

                    return (NTSTATUS)myAssemblyFunction(hProcess, ref BaseAddress, ref RegionSize, FreeType);
                }
            }
        }

        public static NTSTATUS NtCreateFile10(out Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle,
                Int32 desiredAccess,
                ref OBJECT_ATTRIBUTES objectAttributes,
                out IO_STATUS_BLOCK ioStatusBlock,
                ref Int64 allocationSize,
                UInt32 fileAttributes,
                System.IO.FileShare shareAccess,
                UInt32 createDisposition,
                UInt32 createOptions,
                IntPtr eaBuffer,
                UInt32 eaLength)
        {
            byte[] syscall = bNtCreateFile10;

            unsafe
            {
                fixed (byte* ptr = syscall)
                {

                    IntPtr memoryAddress = (IntPtr)ptr;

                    if (!VirtualProtect( memoryAddress,
                        (UIntPtr)syscall.Length, 0x40, out uint oldprotect))
                    {
                        throw new Win32Exception();
                    }

                    Delegates.NtCreateFile myAssemblyFunction = (Delegates.NtCreateFile)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(Delegates.NtCreateFile));

                    return (NTSTATUS)myAssemblyFunction(out fileHandle,
                 desiredAccess,
                ref objectAttributes,
                out ioStatusBlock,
                ref allocationSize,
                 fileAttributes,
                 shareAccess,
                 createDisposition,
                 createOptions,
                 eaBuffer,
                 eaLength);
                }
            }
        }

        public struct Delegates
        {
            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwResumeThread(IntPtr hThread, out ulong PreviousSuspendCount);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwSetInformationThread(IntPtr hThread, uint ThreadInformationClass, IntPtr ThreadInformation, ulong ThreadInformationLength);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwQueueApcThread(IntPtr hThread, IntPtr pfnAPC, IntPtr dwData);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwUnMapViewOfSection(IntPtr hSection, IntPtr address);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwMapViewOfSection(IntPtr section, IntPtr process, ref IntPtr baseAddr, IntPtr zeroBits, IntPtr commitSize, IntPtr stuff, ref IntPtr viewSize, int inheritDispo, uint alloctype, uint protect);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwCreateSection(out IntPtr section, uint desiredAccess, IntPtr pAttrs, ref LARGE_INTEGER pMaxSize, uint pageProt, uint allocationAttribs, IntPtr hFile);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwOpenProcess(out IntPtr hProcess, ProcessAccessFlags processAccess, OBJECT_ATTRIBUTES objAttribute, ref CLIENT_ID clientid);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwClose(IntPtr handle);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwWriteVirtualMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, ref IntPtr lpNumberOfBytesWritten);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwProtectVirtualMemory(IntPtr hProcess, ref IntPtr lpBaseAddress, ref uint NumberOfBytesToProtect, uint NewAccessProtection, ref uint lpNumberOfBytesWritten);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass, IntPtr SystemInformation, uint SystemInformationLength, ref uint ReturnLength);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int NtAllocateVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, IntPtr ZeroBits, ref IntPtr RegionSize, ulong AllocationType, ulong Protect);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int NtFreeVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref uint RegionSize, ulong FreeType);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int NtCreateFile(out Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle,
                Int32 desiredAccess,
                ref OBJECT_ATTRIBUTES objectAttributes,
                out IO_STATUS_BLOCK ioStatusBlock,
                ref Int64 allocationSize,
                UInt32 fileAttributes,
                System.IO.FileShare shareAccess,
                UInt32 createDisposition,
                UInt32 createOptions,
                IntPtr eaBuffer,
                UInt32 eaLength);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool RtlEqualUnicodeString(UNICODE_STRING String1, UNICODE_STRING String2, bool CaseInSensitive);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool RtlGetVersion(ref OSVERSIONINFOEXW lpVersionInformation);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool RtlInitUnicodeString(ref UNICODE_STRING DestinationString, [MarshalAs(UnmanagedType.LPWStr)] string SourceString);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool MiniDumpWriteDump(IntPtr hProcess, uint ProcessId, Microsoft.Win32.SafeHandles.SafeFileHandle hFile, int DumpType, IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallbackParam);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int LdrLoadDll(IntPtr PathToFile,
                UInt32 dwFlags,
                ref Natives.UNICODE_STRING ModuleFileName,
                ref IntPtr ModuleHandle);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Int32 NtSetInformationToken(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref TOKEN_MANDATORY_LABEL TokenInformation, int TokenInformationLength);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwQueryWnfStateData(ref ulong StateId, [In, Optional] WnfType TypeId, [Optional] IntPtr Scope, out int Changestamp, SafeBuffer DataBuffer, ref int DataBufferSize);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int ZwUpdateWnfStateData(ref ulong StateId, SafeBuffer DataBuffer, int DataBufferSize, [In, Optional] WnfType TypeId, [Optional] IntPtr Scope, int MatchingChangestamp, [MarshalAs(UnmanagedType.Bool)] bool CheckChangestamp);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int NtFilterToken(IntPtr TokenHandle, uint Flags, IntPtr SidsToDisable, IntPtr PrivilegesToDelete, IntPtr RestrictedSids, ref IntPtr hToken);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes, int ImpersonationLevel, int TokenType, ref IntPtr phNewToken);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool RevertToSelf();

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Boolean ImpersonateLoggedOnUser(IntPtr hToken);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Boolean AllocateAndInitializeSid(
                        ref SID_IDENTIFIER_AUTHORITY pIdentifierAuthority,
                        byte nSubAuthorityCount,
                        int dwSubAuthority0,
                        int dwSubAuthority1,
                        int dwSubAuthority2,
                        int dwSubAuthority3,
                        int dwSubAuthority4,
                        int dwSubAuthority5,
                        int dwSubAuthority6,
                        int dwSubAuthority7,
                        out IntPtr pSid
                    );

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr GetCurrentProcess();

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool CreateProcess(IntPtr lpApplicationName, string lpCommandLine, IntPtr lpProcAttribs, IntPtr lpThreadAttribs, bool bInheritHandles, uint dwCreateFlags, IntPtr lpEnvironment, IntPtr lpCurrentDir, [In] ref STARTUPINFO lpStartinfo, out PROCESS_INFORMATION lpProcInformation);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool CreateProcessEx(IntPtr lpApplicationName, string lpCommandLine, IntPtr lpProcAttribs, IntPtr lpThreadAttribs, bool bInheritHandles, uint dwCreateFlags, IntPtr lpEnvironment, IntPtr lpCurrentDir, [In] ref STARTUPINFOEX lpStartinfo, out PROCESS_INFORMATION lpProcInformation);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool CloseHandle(IntPtr handle);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint GetLastError();

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GetSystemInfo(ref SYSTEM_INFO lpSysInfo);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool CreateProcessWithLogonW(
                    string userName,
                    string domain,
                    string password,
                    LogonFlags dwLogonFlags,
                    string applicationName,
                    string commandLine,
                    CreationFlags dwCreationFlags,
                    int environment,
                    string currentDirectory,
                    ref STARTUPINFO startupInfo,
                    out PROCESS_INFORMATION processInformation);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MoveMemory(IntPtr dest, IntPtr src, int size);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

            [SuppressUnmanagedCodeSecurity]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint newprotect, out uint oldprotect);

            
        }
    }
}
