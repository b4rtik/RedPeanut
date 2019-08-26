//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Linq;
using static RedPeanut.Utility;

namespace RedPeanut
{
    class SharpDPAPICredentialsManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set pvk", "Set target DC" },
            { "set guid:sha1", "Set context domain" },
            { "set target", "Set context domain" },
            { "set server", "Set context domain" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "credentials";
        string pvk;
        string target;
        string guid_sha1;
        string server;

        public SharpDPAPICredentialsManager()
        {

        }

        public SharpDPAPICredentialsManager(IAgentInstance agent)
        {
            this.agent = agent;
        }

        public void Execute()
        {
            string input;
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(agent, modulename);
                DPAPIMenu(input);
            } while (input != "back");
        }

        private void DPAPIMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set pvk":
                            pvk = GetParsedSetString(input);
                            break;
                        case "set guid:sha1":
                            guid_sha1 = GetParsedSetString(input);
                            break;
                        case "set target":
                            target = GetParsedSetString(input);
                            break;
                        case "set server":
                            server = GetParsedSetString(input);
                            break;
                        case "run":
                            Run();
                            break;
                        case "options":
                            PrintCurrentConfig();
                            break;
                        case "info":
                            PrintOptions("info", mainmenu);
                            break;
                        case "back":
                            Program.GetMenuStack().Pop();
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
            List<string> args = new List<string>();
            args.Add("credentials");

            if (!string.IsNullOrEmpty(target))
            {
                args.Add("/target:" + target);

                if (!string.IsNullOrEmpty(pvk))
                {
                    args.Add("/pvk:" + pvk);
                }

                if (!string.IsNullOrEmpty(guid_sha1))
                {
                    args.Add(guid_sha1);
                }

                RunAssembly(PL_MODULE_SHARPDPAPI, "SharpDPAPI.Program", args.ToArray<string>(), agent);
            }
            else if (!string.IsNullOrEmpty(pvk))
            {
                args.Add("/pvk:" + pvk);

                if (!string.IsNullOrEmpty(server))
                {
                    args.Add("/server:" + server);
                }

                if (!string.IsNullOrEmpty(guid_sha1))
                {
                    args.Add(guid_sha1);
                }

                RunAssembly(PL_MODULE_SHARPDPAPI, "SharpDPAPI.Program", args.ToArray<string>(), agent);
            }
            else if (!string.IsNullOrEmpty(guid_sha1))
            {
                args.Add(guid_sha1);
                RunAssembly(PL_MODULE_SHARPDPAPI, "SharpDPAPI.Program", args.ToArray<string>(), agent);
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "pvk", pvk },
                { "guid:sha1", guid_sha1 },
                { "target", target },
                { "server", server }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

