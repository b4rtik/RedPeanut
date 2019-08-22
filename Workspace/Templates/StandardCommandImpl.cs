using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace StandardCommandImpl
{
    public class Program
    {
        public static void GetUid(string [] param)
        {
            try
            {
                Console.WriteLine("[*] You are {0}", WindowsIdentity.GetCurrent().Name);
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing cd");
            }
        }

        public static void GetPwd(string [] param)
        {
            try
            {
                Console.WriteLine("[*] Current directory {0}", Directory.GetCurrentDirectory());
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing cd");
            }
        }

        public static void GetCd(string [] param)
        {
            try
            {
                if (Directory.Exists(param[0]))
                    Directory.SetCurrentDirectory(param[0]);
                Console.WriteLine("[*] Current directory {0}", Directory.GetCurrentDirectory());
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing cd");
            }
        }

        /// <summary>
        /// Impersonate the SYSTEM user. Equates to `ImpersonateUser("NT AUTHORITY\SYSTEM")`. (Requires Admin)
        /// </summary>
        /// <returns>True if impersonation succeeds, false otherwise.</returns>
        public static void GetSystem(string[] param)
        {
            try
            {
                SharpSploit.Credentials.Tokens t = new SharpSploit.Credentials.Tokens();
                if (t.GetSystem())
                {
                    Console.WriteLine("User: " + t.WhoAmI());
                    Console.WriteLine("Successfully executed getsystem");
                }
                else
                {
                    Console.WriteLine("Failed to execute getsystem");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetSystem error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }   
}
