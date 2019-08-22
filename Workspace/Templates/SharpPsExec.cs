using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Threading;

namespace SharpPsExec
{
    public class Program
    {
        public static string domain = "#DOMAIN#";
        public static string username = "#USERNAME#";
        public static string password = "#PASSWORD#";
        public static string hostname = "#HOSTANME#";
        public static string assembly = "#ASSEMBLY#";
        public static string exename = "#EXENAME#";
        public static string ServiceDisplayName = "#SERVICEDISPLAYNAME#";
        public static string ServiceDescription = "#SERVICEDESCRIPTION#";
        public static string ServiceName = "#SERVICENAME#";

        private static Random random = new Random();

        public static void Execute(string[] args)
        {
            try
            {

                if (string.IsNullOrEmpty(exename))
                    exename = RandomString(10) + ".exe";

                //Login
                IntPtr hToken = IntPtr.Zero;
                IntPtr hTokenDuplicate = IntPtr.Zero;

                if (Natives.LogonUser(username, domain, password, Natives.LOGON32_LOGON_NEW_CREDENTIALS, Natives.LOGON32_PROVIDER_DEFAULT, ref hToken))
                {
                    Console.WriteLine("[*] Login successfully");

                    Natives.SECURITY_ATTRIBUTES secAttribs = new Natives.SECURITY_ATTRIBUTES();

                    if (Natives.DuplicateTokenEx(hToken, 0xf01ff, ref secAttribs, 2, 1, ref hTokenDuplicate))
                    {

                        if (!Natives.ImpersonateLoggedOnUser(hTokenDuplicate))
                            throw new SystemException("[x] Failed to impersonate context");

                        try
                        {
                            CopyServiceExe(DecompressFile(Convert.FromBase64String(assembly)), @"\\" + hostname, exename);
                            InstallService(@"\\" + hostname, exename);
                            Thread.Sleep(5000);
                            UninstallService(@"\\" + hostname);
                            DeleteServiceExe(@"\\" + hostname, exename);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[x] Something went wrong!!" + e.Message);
                            Console.WriteLine("[x] " + e.StackTrace);
                        }

                        Natives.RevertToSelf();
                    }
                    else
                    {
                        Console.WriteLine("[x] Faliled duplicate Token");
                    }
                }
                else
                {
                    Console.WriteLine("[x] Logon Faliled {0} - {1} - {2}", username, domain, password);
                }

                if (hToken != IntPtr.Zero) Natives.CloseHandle(hToken);
                if (hTokenDuplicate != IntPtr.Zero) Natives.CloseHandle(hTokenDuplicate);
            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Something went wrong!!" + e.Message);
            }
        }

        private static byte[] DecompressFile(byte[] gzip)
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

        private static void CopyServiceExe(byte[] assembly, string hostname, string exename)
        {
            byte[] svcexe = assembly;

            var path = hostname + @"\admin$\system32\" + exename;
            try
            {
                File.WriteAllBytes(path, svcexe);

                Console.WriteLine("[*] Copied {0} -> {1} service executable to {2}", exename, hostname, path);

            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine(uae.Message);
                return;
            }
        }

        private static void DeleteServiceExe(string hostname, string exename)
        {
            var path = hostname + @"\admin$\system32\" + exename;
            var max = 5;
            for (int i = 0; i < max; i++)
            {
                try
                {
                    Thread.Sleep(1000);
                    File.Delete(path);
                    i = max;

                    Console.WriteLine("[*] Deleted service executable {0} from {1}", exename, hostname);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static void InstallService(string hostname, string exename)
        {
            try
            {
                UninstallService(hostname);
            }
            catch (Exception) { }

            using (var scmHandle = Natives.OpenSCManager(hostname, null, Natives.SC_MANAGER_CREATE_SERVICE))
            {
                if (scmHandle.IsInvalid)
                {
                    throw new Win32Exception();
                }

                using (
                    var serviceHandle = Natives.CreateService(
                        scmHandle,
                        ServiceDisplayName,
                        ServiceDescription,
                        Natives.SERVICE_ALL_ACCESS,
                        Natives.SERVICE_WIN32_OWN_PROCESS,
                        Natives.SERVICE_AUTO_START,
                        Natives.SERVICE_ERROR_NORMAL,
                        exename,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null))
                {
                    if (serviceHandle.IsInvalid)
                    {
                        throw new Win32Exception();
                    }
                    Console.WriteLine("[*] Installed {0} Service on {1}", ServiceName, hostname);
                    Natives.StartService(serviceHandle, 0, null);
                    Console.WriteLine("[*] Service Started on {0}", hostname);
                }
            }
        }
        static void UninstallService(string hostname)
        {
            using (var scmHandle = Natives.OpenSCManager(hostname, null, Natives.SC_MANAGER_CREATE_SERVICE))
            {
                if (scmHandle.IsInvalid)
                {
                    throw new Win32Exception();
                }

                using (var serviceHandle = Natives.OpenService(scmHandle, ServiceDisplayName, Natives.SERVICE_ALL_ACCESS))
                {
                    if (serviceHandle.IsInvalid)
                    {
                        throw new Win32Exception();
                    }

                    Natives.DeleteService(serviceHandle);

                    Console.WriteLine("[*] Service Uninstalled from {0}", hostname);
                }
            }
        }

    }

    [RunInstaller(true)]
    public class SharpPsExecSvcInstaller : Installer
    {
        public SharpPsExecSvcInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();
            serviceInstaller.DisplayName = Program.ServiceDisplayName;
            serviceInstaller.Description = Program.ServiceDescription;
            serviceInstaller.ServiceName = Program.ServiceName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            processInstaller.Account = ServiceAccount.LocalSystem;
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
            serviceInstaller.AfterInstall += ServiceInstaller_AfterInstall;
        }

        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            foreach (var svc in ServiceController.GetServices())
            {
                if (svc.ServiceName == Program.ServiceName)
                {
                    Console.WriteLine("[*] Found service installed: {0}", svc.DisplayName);
                    Console.WriteLine("[*] Starting service ...");
                    svc.Start();
                }
            }
        }
    }

    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    class ServiceControlHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Create a SafeHandle, informing the base class 
        // that this SafeHandle instance "owns" the handle,
        // and therefore SafeHandle should call 
        // our ReleaseHandle method when the SafeHandle 
        // is no longer in use. 
        private ServiceControlHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        override protected bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions. 
            return Natives.CloseServiceHandle(this.handle);
            // If ReleaseHandle failed, it can be reported via the 
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger 
            // or during testing to diagnose handle corruption problems. 
            // We do not throw an exception because most code could not recover 
            // from the problem.
        }
    }

    class Natives
    {
        public static uint SC_MANAGER_CREATE_SERVICE = 0x00002;
        public static int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public static int SERVICE_ERROR_NORMAL = 0x00000001;
        public static int SERVICE_AUTO_START = 0x00000002;
        public static uint SERVICE_ALL_ACCESS = 0xF01FF;

        public static int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        public static int LOGON32_PROVIDER_DEFAULT = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern bool LogonUser(string pszUserName, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public extern static bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes, int ImpersonationLevel, int TokenType, ref IntPtr phNewToken);

        [DllImport("advapi32.dll")]
        public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll")]
        public static extern ServiceControlHandle OpenSCManager(string lpMachineName, string lpSCDB, uint scParameter);

        [DllImport("Advapi32.dll")]
        public static extern ServiceControlHandle CreateService(ServiceControlHandle serviceControlManagerHandle, string lpSvcName, string lpDisplayName, uint dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [DllImport("advapi32.dll")]
        public static extern bool CloseServiceHandle(IntPtr serviceHandle);

        [DllImport("advapi32.dll")]
        public static extern int StartService(ServiceControlHandle serviceHandle, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern ServiceControlHandle OpenService(ServiceControlHandle serviceControlManagerHandle, string lpSvcName, uint dwDesiredAccess);

        [DllImport("advapi32.dll")]
        public static extern int DeleteService(ServiceControlHandle serviceHandle);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

    }
}

