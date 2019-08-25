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
    class RubeusKlistManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set luid", "Set logonid" },
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
        string modulename = "klist";
        string luid;

        public RubeusKlistManager()
        {

        }

        public RubeusKlistManager(IAgentInstance agent)
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
                RubeusAskTgtMenu(input);
            } while (input != "back");
        }

        private void RubeusAskTgtMenu(string input)
        {
            string[] a_input = input.Split(' ');
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set luid":
                            luid = GetParsedSetString(input);
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
            args.Add("luid");

            if (!string.IsNullOrEmpty(luid))
            {
                args.Add("/luid:" + luid);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }

        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();
            Console.WriteLine("{0}: {1}", "luid", luid);

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "luid", luid }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

