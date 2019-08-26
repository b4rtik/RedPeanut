//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    class SharpAdidnsdumpManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set dc", "Execute module" },
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
        string modulename = "sharpadidnsdump";
        string dc;

        bool exit = false;

        public SharpAdidnsdumpManager()
        {

        }

        public SharpAdidnsdumpManager(IAgentInstance agent)
        {
            this.agent = agent;
        }

        public void Execute()
        {
            exit = false;
            string input;
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(agent, modulename);
                SharpAdidnsdumpMenu(input);
            } while (!exit);
        }

        private void SharpAdidnsdumpMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "run":
                            Run();
                            break;
                        case "set dc":
                            dc = GetParsedSetString(input);
                            break;
                        case "options":
                            PrintCurrentConfig();
                            break;
                        case "info":
                            PrintOptions("info", mainmenu);
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
            if(!string.IsNullOrEmpty(dc))
                RunAssembly(PL_MODULE_SHARPADIDNSDUMP, "SharpAdidnsdump.Program", new string[] { dc }, agent);
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "dc", dc }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
