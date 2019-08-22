//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RedPeanut.Models;
using static RedPeanut.Utility;

namespace RedPeanut
{
    class Program
    {
        static Stack<IMenu> Menustack = new Stack<IMenu>();

        static C2Manager c2m = null;

        static void Execute(string serverkey)
        {
            PrintBanner();

            c2m = new C2Manager();
            c2m.CreateC2Server(serverkey);
            CheckConfiguredListeners(c2m.GetC2Server());

            int defaulthttpprofile = c2m.GetC2Server().GetDefaultProfile();

            if (defaulthttpprofile != 0)
            {
                do
                {
                    if (Menustack.Count == 0)
                        Menustack.Push(new RedPeanutManager(serverkey));
                    Menustack.Peek().Execute();
                } while (true);
            }
            else
            {
                Console.WriteLine("[x] Error loading profiles");
            }
        }

        static void PrintBanner()
        {
            Console.WriteLine();
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("ooooooo________________oo_ooooooo___________________________________oo____");
            Console.WriteLine("oo____oo___ooooo___oooooo_oo____oo__ooooo___ooooo__oo_ooo__oo____o__oo____");
            Console.WriteLine("oo____oo__oo____o_oo___oo_oo____oo_oo____o_oo___oo_ooo___o_oo____o_oooo___");
            Console.WriteLine("ooooooo___ooooooo_oo___oo_oooooo___ooooooo_oo___oo_oo____o_oo____o__oo____");
            Console.WriteLine("oo____oo__oo______oo___oo_oo_______oo______oo___oo_oo____o_ooo___o__oo__o_");
            Console.WriteLine("oo_____oo__ooooo___oooooo_oo________ooooo___oooo_o_oo____o_oo_ooo____ooo__");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("________________________________________________RedPeanut_v0.2.3___@b4rtik");
            Console.WriteLine("__________________________________________________________________________");
            Console.WriteLine("");
        }

        static private void CheckConfiguredListeners(C2Server srv)
        {
            RedPeanutDBContext context = srv.GetDBContext();
            if (context.Listeners.Count() > 0)
            {
                Console.WriteLine("[*] Starting listeners");
                foreach(Listener l in context.Listeners)
                {
                    int profileid = l.profile;
                    string name = l.name;
                    string lhost = l.lhost;
                    int lport = l.lport;

                    bool ssl = (l.ListenerType == ListenerType.Https) ? true : false;

                    if (profileid != 0 && srv.GetProfiles().ContainsKey(profileid))
                    {
                        HttpProfile profile = srv.GetProfile(profileid);
                        ListenerConfig conf = new ListenerConfig(name, lhost, lport, profile, profileid, ssl);
                        srv.ReloadListenerConfig(name, conf);
                        srv.StartServerHttpServer(conf);                       
                    }
                    else
                    {
                        HttpProfile profile = srv.GetProfile(srv.GetDefaultProfile());
                        ListenerConfig conf = new ListenerConfig(name, lhost, lport, profile, profileid, ssl);
                        srv.ReloadListenerConfig(name, conf);
                        srv.StartServerHttpServer(conf);
                    }
                }
            }
        }

        public static string GetServerKey()
        {
            return c2m.GetServerKey();
        }

        public static C2Manager GetC2Manager()
        {
            return c2m;
        }

        public static Stack<IMenu> GetMenuStack()
        {
            return Menustack;
        }

        static void Main(string[] args)
        {
            // Check if there is a generated serverkey 
            // if not found generate
            string serverkeyfile = WORKSPACE_FOLDER + Path.DirectorySeparatorChar + SERVERKEY_FILE;
            try
            {
                if (File.Exists(serverkeyfile))
                {
                    // File allready generated ask for password to decrypt
                    Console.Write("Enter password to decrypt serverkey: ");
                    string password = ReadLine.ReadPassword();

                    string filecontent = File.ReadAllText(serverkeyfile);

                    filecontent = Crypto.RC4.Decrypt(password, filecontent);

                    ServerKey serverkeyobj = JsonConvert.DeserializeObject<ServerKey>(filecontent);

                    //All is ok file valid check key lenght than use it

                    if (serverkeyobj.serverkey.Length == 16)
                    {

                        Execute(serverkeyobj.serverkey);
                    }
                    else
                    {
                        throw new SystemException("Serverkey corrupted");
                    }
                }
                else
                {
                    // Serverkey file not found ask for password to encrypt the file
                    string password = "";
                    do
                    {
                        Console.Write("Enter password to encrypt serverkey: ");
                        password = ReadLine.ReadPassword();
                    }
                    while (password.Length < 8 || password.StartsWith("        "));

                    // Generate serverkey and store in encrypted file
                    ServerKey serverkeyobj = new ServerKey();
                    Random r = new Random();
                    serverkeyobj.serverkey = RandomString(16, r);
                    string serverkeystr = JsonConvert.SerializeObject(serverkeyobj, Formatting.Indented);
                    File.WriteAllText(serverkeyfile, Crypto.RC4.Encrypt(password, serverkeystr));

                    Execute(serverkeyobj.serverkey);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Serverkey error {0}", e.Message);
                Console.WriteLine("[x] Serverkey error {0}", e.StackTrace);
            }
        }
    }
}
