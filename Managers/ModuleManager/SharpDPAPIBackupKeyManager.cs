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
    class SharpDPAPIBackupKeyManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set server", "Set target DC" },
            { "set outputfile", "Set context domain" },
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
        string modulename = "backupkey";
        string domainController;
        string outputfile;

        public SharpDPAPIBackupKeyManager()
        {

        }

        public SharpDPAPIBackupKeyManager(IAgentInstance agent)
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
            string[] a_input = input.Split(' ');
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set server":
                            domainController = GetParsedSetString(input);
                            break;
                        case "set outputfile":
                            outputfile = GetParsedSetString(input);
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
            args.Add("backupkey");

            if (!string.IsNullOrEmpty(outputfile))
            {
                args.Add("/file:" + outputfile);
            }

            if (!string.IsNullOrEmpty(domainController))
            {
                args.Add("/server:" + domainController);
            }

            RunAssembly(PL_MODULE_SHARPDPAPI, "SharpDPAPI.Program", args.ToArray<string>(), agent);

        }

        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();
            Console.WriteLine("{0}: {1}", "outputfile", outputfile);
            Console.WriteLine("{0}: {1}", "server", domainController);

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "outputfile", outputfile },
                { "server", domainController }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

