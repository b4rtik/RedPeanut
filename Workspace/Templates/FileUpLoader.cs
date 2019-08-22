using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

class FileUpLoader
{
    static string filesrc = "#FILEBASE64#";
    static string filename = "#FILENAME#";
    static string pathdest = "#PATHDEST#";

    static string username = "#USERNAME#";
    static string password = "#PASSWORD#";
    static string domain = "#DOMAIN#";

    public static void Execute(string[] args)
    {
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            CopyFile();
        }
        else
        {
            try
            {
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
                            CopyFile();
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
            catch (Exception)
            {
                Console.WriteLine("[x] Error writing file");
                return;
            }
        }
    
    }

	public static void CopyFile()
    {
        if (Directory.Exists(pathdest))
        {
            byte[] file = DecompressFile(Convert.FromBase64String(filesrc));
            string filepath = Path.Combine(pathdest, filename);
            if (File.Exists(filepath))
                File.Delete(filepath);
            File.WriteAllBytes(filepath, file);
            Console.WriteLine("[*] File {0} uploaded to  {1}", filename, pathdest);
        }
        else
        {
            Console.WriteLine("[x] destpath does not exists");
            Console.WriteLine("[x] " + pathdest);
            return;
        }
    }

    public static byte[] DecompressFile(byte[] gzip)
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

    [DllImport("kernel32.dll")]
    public static extern int GetLastError();

}
