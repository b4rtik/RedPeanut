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
    class SharpWebManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set checkname", "Set command to execute (all, full, chrome, firefox, edge)" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "sharpweb";
        string checkname = "";

        bool exit = false;

        public SharpWebManager()
        {

        }

        public SharpWebManager(IAgentInstance agent)
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
                SharpWebMenu(input);
            } while (!exit);
        }

        private void SharpWebMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set checkname":
                            checkname = GetParsedSetString(input);
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
            List<string> args = new List<string>();

            if (!string.IsNullOrEmpty(checkname))
            {
                args.Add(checkname);
            }

            RunAssembly(PL_MODULE_SHARPWEB, "SharpWeb.Program", args.ToArray(), agent);
        }

        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();
            Console.WriteLine("{0}: {1}", "checkname", checkname);

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "checkname", checkname }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
