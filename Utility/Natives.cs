//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RedPeanut
{
    public class Natives
    {

        public const string UNTRUSTED_MANDATORY_LEVEL = "S-1-16-0";
        public const string LOW_MANDATORY_LEVEL = "S-1-16-4096";
        public const string MEDIUM_MANDATORY_LEVEL = "S-1-16-8192";
        public const string MEDIUM_PLUS_MANDATORY_LEVEL = "S-1-16-8448";
        public const string HIGH_MANDATORY_LEVEL = "S-1-16-12288";
        public const string SYSTEM_MANDATORY_LEVEL = "S-1-16-16384";
        public const string PROTECTED_PROCESS_MANDATORY_LEVEL = "S-1-16-20480";
        public const string SECURE_PROCESS_MANDATORY_LEVEL = "S-1-16-28672";

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public uint dwOem;
            public uint dwPageSize;
            public IntPtr lpMinAppAddress;
            public IntPtr lpMaxAppAddress;
            public IntPtr dwActiveProcMask;
            public uint dwNumProcs;
            public uint dwProcType;
            public uint dwAllocGranularity;
            public ushort wProcLevel;
            public ushort wProcRevision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LARGE_INTEGER
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebAddress;
            public IntPtr Reserved2;
            public IntPtr Reserved3;
            public IntPtr UniquePid;
            public IntPtr MoreReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            uint cb;
            IntPtr lpReserved;
            IntPtr lpDesktop;
            IntPtr lpTitle;
            uint dwX;
            uint dwY;
            uint dwXSize;
            uint dwYSize;
            uint dwXCountChars;
            uint dwYCountChars;
            uint dwFillAttributes;
            uint dwFlags;
            ushort wShowWindow;
            ushort cbReserved;
            IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdErr;
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200),
            THREAD_HIJACK = SUSPEND_RESUME | GET_CONTEXT | SET_CONTEXT,
            THREAD_ALL = (0x001f0ffb)
        }

        public enum ThreadInfoClass : int
        {
            ThreadBasicInformation = 0,
            ThreadQuerySetWin32StartAddress = 9
        }

        public const uint CreateSuspended = 0x00000004;
        public const uint SecCommit = 0x08000000;
        public const uint GenericAll = 0x10000000;

        // Process privileges
        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_READ = 0x0010;

        [Flags]
        public enum PROCESS_RIGHTS : uint
        {
            PROCESS_TERMINATE = 0x0001,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_SET_SESSIONID = 0x0004,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_DUP_HANDLE = 0x0040,
            PROCESS_CREATE_PROCESS = 0x0080,
            PROCESS_SET_QUOTA = 0x0100,
            PROCESS_SET_INFORMATION = 0x0200,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_SUSPEND_RESUME = 0x0800,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_ALL_ACCESS = STANDARD_RIGHTS.STANDARD_RIGHTS_ALL |
                STANDARD_RIGHTS.SYNCHRONIZE | 0xffff
        }

        [Flags]
        public enum STANDARD_RIGHTS : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = READ_CONTROL,
            STANDARD_RIGHTS_WRITE = READ_CONTROL,
            STANDARD_RIGHTS_EXECUTE = READ_CONTROL,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,
            ACCESS_SYSTEM_SECURITY = 0x01000000,
            MAXIMUM_ALLOWED = 0x02000000,
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000
        }


        // Memory permissions
        public const uint MEM_COMMIT = 0x00001000;
        public const uint MEM_RESERVE = 0x00002000;
        public const uint PAGE_READWRITE = 4;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_EXECUTE_READ = 0x20;

        [Flags]
        public enum CONTEXT_FLAGS : uint
        {
            CONTEXT_i386 = 0x10000,
            CONTEXT_i486 = 0x10000,   //  same as i386
            CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
            CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
            CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
            CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
            CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
            CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
            CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
            CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
        }

        [Flags]
        public enum MEMORY_PROTECTION : int
        {
            PAGE_ACCESS_DENIED = 0x0,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08
        }

        [Flags]
        public enum MEMORY_STATE : int
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,

            /// <summary>
            /// Decommits memory, putting them into the reserved state.
            /// </summary>
            MEM_DECOMMIT = 0x4000,

            /// <summary>
            /// Frees memory, putting them into the freed state.
            /// </summary>
            MEM_RELEASE = 0x8000,
            MEM_FREE = 0x10000,
            MEM_RESET = 0x80000,
            MEM_TOP_DOWN = 0x100000,
            MEM_PHYSICAL = 0x400000,
            MEM_LARGE_PAGES = 0x20000000
        }

        public enum MEMORY_TYPE : int
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }


        // x86 float save
        [StructLayout(LayoutKind.Sequential)]
        public struct FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] RegisterArea;
            public uint Cr0NpxState;
        }

        // x86 context structure (not used in this example)
        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT
        {
            public CONTEXT_FLAGS ContextFlags; //set this to an appropriate value 
                                               // Retrieved by CONTEXT_DEBUG_REGISTERS 
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            // Retrieved by CONTEXT_FLOATING_POINT 
            public FLOATING_SAVE_AREA FloatSave;
            // Retrieved by CONTEXT_SEGMENTS 
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            // Retrieved by CONTEXT_INTEGER 
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            // Retrieved by CONTEXT_CONTROL 
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            // Retrieved by CONTEXT_EXTENDED_REGISTERS 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;
        }

        // x64 m128a
        [StructLayout(LayoutKind.Sequential)]
        public struct M128A
        {
            public ulong High;
            public long Low;

            public override string ToString()
            {
                return string.Format("High:{0}, Low:{1}", this.High, this.Low);
            }
        }

        // x64 save format
        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct XSAVE_FORMAT64
        {
            public ushort ControlWord;
            public ushort StatusWord;
            public byte TagWord;
            public byte Reserved1;
            public ushort ErrorOpcode;
            public uint ErrorOffset;
            public ushort ErrorSelector;
            public ushort Reserved2;
            public uint DataOffset;
            public ushort DataSelector;
            public ushort Reserved3;
            public uint MxCsr;
            public uint MxCsr_Mask;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public M128A[] FloatRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public M128A[] XmmRegisters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved4;
        }

        // x64 context structure
        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct CONTEXT64
        {
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;

            public CONTEXT_FLAGS ContextFlags;
            public uint MxCsr;

            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;

            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;

            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;
            public ulong Rip;

            public XSAVE_FORMAT64 DUMMYUNIONNAME;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public M128A[] VectorRegister;
            public ulong VectorControl;

            public ulong DebugControl;
            public ulong LastBranchToRip;
            public ulong LastBranchFromRip;
            public ulong LastExceptionToRip;
            public ulong LastExceptionFromRip;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct THREAD_BASIC_INFORMATION
        {
            public uint ExitStatus; // original: LONG NTSTATUS
            public uint TebBaseAddress; // original: PVOID
            public CLIENT_ID ClientId;
            public uint AffinityMask; // original: ULONG_PTR
            public uint Priority; // original: DWORD
            public uint BasePriority; // original: DWORD
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CLIENT_ID
        {
            public uint UniqueProcess; // original: PVOID
            public uint UniqueThread; // original: PVOID
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public MemoryProtection AllocationProtect;
            private int _alignment1;
            public ulong RegionSize;
            public MemoryState State;
            public MemoryProtection Protect;
            public MemoryType Type;
            private int _alignment2;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct MemoryBasicInformation64
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public MemoryProtection AllocationProtect;
            //_private int _alignment1;
            public ulong RegionSize;
            public MemoryState State;
            public MemoryProtection Protect;
            public MemoryType Type;
            //private int _alignment2;
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            AccessDenied = 0x0,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08
        }

        [Flags]
        public enum MemoryState : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,

            /// <summary>
            /// Decommits memory, putting it into the reserved state.
            /// </summary>
            Decommit = 0x4000,

            /// <summary>
            /// Frees memory, putting it into the freed state.
            /// </summary>
            Release = 0x8000,
            Free = 0x10000,
            Reset = 0x80000,
            Physical = 0x400000,
            LargePages = 0x20000000
        }

        public enum MemoryType : int
        {
            Image = 0x1000000,
            Mapped = 0x40000,
            Private = 0x20000
        }

        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }

        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {

            public IntPtr Sid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;

            [MarshalAs(UnmanagedType.ByValArray)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public SE_PRIVILEGE_ATTRIBUTES Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        [Flags]
        public enum SE_PRIVILEGE_ATTRIBUTES : uint
        {
            SE_PRIVILEGE_DISABLED = 0x00000000,
            SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
            SE_PRIVILEGE_ENABLED = 0x00000002,
            SE_PRIVILEGE_REMOVED = 0x00000004,
            SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_MANDATORY_LABEL
        {

            public SID_AND_ATTRIBUTES Label;

        }

        public enum AddressMode
        {
            _1616,
            _1632,
            Real,
            Flat,
        }

        public enum SymType
        {
            SymNone = 0,
            SymCoff,
            SymCv,
            SymPdb,
            SymExport,
            SymDeferred,
            SymSym, // .sym file
            SymDia,
            SymVirtual,
            NumSymTypes
        }

        [Flags]
        public enum ListModules : uint
        {
            Default = 0x0,
            _32Bit = 0x01,
            _64Bit = 0x02,
            All = 0x03,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            uint Data1;
            ushort Data2;
            ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            byte[] Data4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEHLP_MODULE64
        {
            public uint SizeOfStruct;
            public ulong BaseOfImage;
            public uint ImageSize;
            public uint TimeDateStamp;
            public uint CheckSum;
            public uint NumSyms;
            SymType SymType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] ModuleName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] ImageName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] LoadedImageName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] LoadedPdbName;
            public uint CVSig;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260 * 3)]
            public char[] CVData;
            public uint PdbSig;
            GUID PdbSig70;
            public uint PdbAge;
            public bool PdbUnmatched;
            public bool DbgUnmatched;
            public bool LineNumbers;
            public bool GlobalSymbols;
            public bool TypeInfo;
            public bool SourceIndexed;
            public bool Publics;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEHLP_SYMBOL64
        {
            public uint SizeOfStruct;   // set to sizeof(IMAGEHLP_SYMBOLW64)
            public ulong Address;        // virtual address including dll base address
            public uint Size;           // estimated size of symbol, can be zero
            public uint Flags;          // info about the symbols, see the SYMF defines
            public uint MaxNameLength;  // maximum size of symbol name in 'Name'
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
            public char[] Name;           // symbol name (null terminated string)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULE_INFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KDHELP
        {
            public ulong Thread;
            public uint ThCallbackStack;
            public uint ThCallbackBStore;
            public uint NextCallback;
            public uint FramePointer;
            public uint KiCallUserMode;
            public uint KeUserCallbackDispatcher;
            public uint SystemRangeStart;
            public uint KiUserExceptionDispatcher;
            public uint StackBase;
            public uint StackLimit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ulong[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ADDRESS64
        {
            public ulong Offset;
            public ushort Segment;
            public AddressMode Mode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STACKFRAME64
        {
            public ADDRESS64 AddrPC; //Program Counter EIP, RIP
            public ADDRESS64 AddrReturn; //Return Address
            public ADDRESS64 AddrFrame; //Frame Pointer EBP, RBP or RDI
            public ADDRESS64 AddrStack; //Stack Pointer ESP, RSP
            public ADDRESS64 AddrBStore; //IA64 Backing Store RsBSP
            public IntPtr FuncTableEntry; //x86 = FPO_DATA struct, if none = NULL
            public ulong Offset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ulong[] Params; //possible arguments to the function
            public bool Far; //TRUE if this is a WOW far call
            public bool Virtual; //TRUE if this is a virtual frame
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ulong[] Reserved; //used internally by StackWalk64
            public KDHELP KdHelp; //specifies helper data for walking kernel callback frames
        }

        public enum ImageFileMachine : int
        {
            I386 = 0x014c,
            IA64 = 0x0200,
            AMD64 = 0x8664,
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcess(IntPtr lpApplicationName, string lpCommandLine, IntPtr lpProcAttribs, IntPtr lpThreadAttribs, bool bInheritHandles, uint dwCreateFlags, IntPtr lpEnvironment, IntPtr lpCurrentDir, [In] ref STARTUPINFO lpStartinfo, out PROCESS_INFORMATION lpProcInformation);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, IntPtr nSize, out IntPtr lpNumWritten);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int NtResumeThread(IntPtr hThread);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern uint GetLastError();

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int NtCreateSection(ref IntPtr section, uint desiredAccess, IntPtr pAttrs, ref LARGE_INTEGER pMaxSize, uint pageProt, uint allocationAttribs, IntPtr hFile);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int NtMapViewOfSection(IntPtr section, IntPtr process, ref IntPtr baseAddr, IntPtr zeroBits, IntPtr commitSize, IntPtr stuff, ref IntPtr viewSize, int inheritDispo, uint alloctype, uint prot);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int NtUnmapViewOfSection(IntPtr hSection, IntPtr address);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSysInfo);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, int procInformationClass, ref PROCESS_BASIC_INFORMATION procInformation, uint ProcInfoLen, ref uint retlen);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int NtQueryInformationThread(IntPtr threadHandle, int threadInformationClass, out IntPtr threadInformation, uint threadInformationLength, out uint returnLengthPtr);
      
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, int dwThreadId);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr NtQueueApcThread(IntPtr hThread, IntPtr pfnAPC, IntPtr dwData);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr NtSetInformationThread(IntPtr hThread, uint ThreadInformationClass, IntPtr ThreadInformation, ulong ThreadInformationLength);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 processAccess, bool bInheritHandle, int processId);

        [DllImport("advapi32.dll")]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualQueryEx(
            [In] IntPtr Process,
            [In] [Optional] IntPtr Address,
            [Out] /*[MarshalAs(UnmanagedType.Struct)]*/ out MemoryBasicInformation64 Buffer,
            [In] int Size
            );


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenThreadToken(IntPtr ThreadHandle, uint DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ConvertSidToStringSid(IntPtr pSid, ref IntPtr strSid);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        //SuspendThread
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int SuspendThread(IntPtr hThread);

        //TerminateThread
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateThread(IntPtr hThread, int ExitCode);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        //EnumProcessModulesEx
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, IntPtr ModuleArray, int cb, ref int cbNeeded, ListModules FilterFlags);

        //GetModuleInformation
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, ref MODULE_INFO lpModInfo, int cb);

        //GetModuleBaseNameW
        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetModuleBaseNameW(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

        //GetModuleFileNameExW
        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetModuleFileNameExW(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

        ////GetMappedFileNameW
        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetMappedFileNameW(IntPtr hProcess, IntPtr lpAddress, StringBuilder lpFilename, int nSize);

        //SymInitialize
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, [MarshalAs(UnmanagedType.Bool)] bool InvadeProcess);

        //SymCleanup
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymCleanup(IntPtr hProcess);

        //SymFunctionTableAccess64
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SymFunctionTableAccess64(IntPtr hProcess, ulong AddrBase);

        //SymGetModuleBase64
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ulong SymGetModuleBase64(IntPtr hProcess, ulong dwAddr);

        //SymGetModuleInfo64
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymGetModuleInfo64(IntPtr hProcess, int dwAddr, /*[Out]*/ IMAGEHLP_MODULE64 ModuleInfo);

        //SymGetSymFromAddr64
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymGetSymFromAddr64(IntPtr hProcess, uint Address, int OffestFromSymbol, IntPtr Symbol);

        //SymFromAddr
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymFromAddr(IntPtr hProcess, int Address, int OffestFromSymbol, IntPtr Symbol);

        //SymLoadModuleEx
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName, IntPtr BaseOfDll, int DllSize, IntPtr Data, int Flags);

        //StackWalk64
        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StackWalk64
        (
        int MachineType, //In
        IntPtr hProcess, //In
        IntPtr hThread, //In
        IntPtr StackFrame, //In_Out
        IntPtr ContextRecord, //In_Out
        ReadProcessMemoryDelegate ReadMemoryRoutine, //_In_opt_
        SymFunctionTableAccess64Delegate FunctionTableAccessRoutine, //_In_opt_
        SymGetModuleBase64Delegate GetModuleBaseRoutine, //_In_opt_
        TranslateAddressProc64Delegate TranslateAddress //_In_opt_
        );

        //StackWalk64 Callback Delegates
        public delegate bool ReadProcessMemoryDelegate(IntPtr hProcess, int lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);
        public delegate IntPtr SymFunctionTableAccess64Delegate(IntPtr hProcess, ulong AddrBase);
        public delegate ulong SymGetModuleBase64Delegate(IntPtr hProcess, ulong Address);
        public delegate ulong TranslateAddressProc64Delegate(IntPtr hProcess, IntPtr hThread, IntPtr lpAddress64);

    }

}

