//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using static RedPeanut.Utility;
using System.Collections.Generic;
using static RedPeanut.Models;

namespace RedPeanut
{
    class HttpServerManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set name", "Listener name" },
            { "set lhost", "Host" },
            { "set lport", "Set lport" },
            { "set ssl", "If true (default) use ssl (http listener will serve contents only)" },
            { "set profile", "Profile id" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        string modulename = "http";
        string lhost = "";
        int profileid = 0;
        int lport = 0;
        string name = "";
        bool ssl = true;

        bool exit = false;

        C2Server srv = null;

        public HttpServerManager(C2Server srv)
        {
            this.srv = srv;
        }

        public void Execute()
        {
            exit = false;
            string input = "";
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(null, modulename);
                ServerMenu(input);
            } while (!exit);
        }

        private void ServerMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set name":
                            name = GetParsedSetString(input);
                            break;
                        case "set lhost":
                            lhost = GetParsedSetString(input);
                            break;
                        case "set lport":
                            lport = GetParsedSetInt(input);
                            break;
                        case "set ssl":
                            ssl = GetParsedSetBool(input);
                            break;
                        case "set profile":
                            profileid = GetParsedSetInt(input);
                            break;
                        case "run":
                            Run();
                            break;
                        case "options":
                            PrintOptions("options", mainmenu);
                            break;
                        case "back":
                            Program.GetMenuStack().Pop();
                            exit = true;
                            return;
                        default:
                            Console.WriteLine("We had a woodoo");
                            break;
                    }
                }
                else
                {
                    PrintOptions("Command not found", mainmenu);
                }
            }
        }

        private void Run()
        {
            if(!srv.IsStarted(name))
            {
                if (!string.IsNullOrEmpty(lhost) && lport > 0 && !string.IsNullOrEmpty(name) )
                {
                    HttpProfile profile;
                    if (profileid != 0 && srv.GetProfiles().ContainsKey(profileid))
                    {
                        profile = srv.GetProfile(profileid);                      
                    }
                    else
                    {
                        profile = srv.GetProfile(srv.GetDefaultProfile());
                        profileid = srv.GetDefaultProfile();
                    }

                    ListenerConfig conf = new ListenerConfig(name, lhost, lport, profile, profileid, ssl);
                    srv.RegisterListenerConfig(name, conf);
                    srv.StartServerHttpServer(conf);
                }
                else
                {
                    Console.WriteLine("[-] pipename can't be null");
                }
            }
            else
            {
                Console.WriteLine("[-] Server running");
            }

        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "name", name },
                { "lhost", lhost },
                { "lport", lport.ToString() },
                { "ssl", ssl.ToString() },
                { "profile", profileid.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }

    }
}
