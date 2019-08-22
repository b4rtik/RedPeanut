using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

class FileDownLoader
{
    static string filesrc = "#FILESRC#";

    static string username = "#USERNAME#";
    static string password = "#PASSWORD#";
    static string domain = "#DOMAIN#";

    public static void Execute(string[] args)
    {

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
			if (File.Exists(filesrc))
			{
				ReadFile(filesrc);
			}
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
                    Natives.SECURITY_ATTRIBUTES secAttribs = new Natives.SECURITY_ATTRIBUTES();

                    if (Natives.DuplicateTokenEx(hToken, 0xf01ff, ref secAttribs, 2, 1, ref hTokenDuplicate))
                    {

                        if (!Natives.ImpersonateLoggedOnUser(hTokenDuplicate))
                            return;

                        try
                        {
							if (File.Exists(filesrc))
							{
								ReadFile(filesrc);
							}
                        }
                        catch (Exception)
                        {
                            
                        }

                        Natives.RevertToSelf();
                    }
                }

                if (hToken != IntPtr.Zero) Natives.CloseHandle(hToken);
                if (hTokenDuplicate != IntPtr.Zero) Natives.CloseHandle(hTokenDuplicate);

            }
            catch (Exception)
            {
                return;
            }
        }
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

	public static void ReadFile(string filepath)
    {
        try
        {
            Console.WriteLine(Convert.ToBase64String(CompressGZipAssembly(File.ReadAllBytes(filepath))));
        }
        catch (Exception e) 
		{ 
			Console.WriteLine("[x] Error reading file: " + e.Message + Environment.NewLine + e.StackTrace); 
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
