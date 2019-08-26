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
    class SeatbeltManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set checkname", "Set command to execute (system, user, all)" },
            { "set fullmode", "Set full mode" },
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
        string modulename = "seatbelt";
        string checkname = "";
        bool fullmode = false;

        bool exit = false;

        public SeatbeltManager()
        {

        }

        public SeatbeltManager(IAgentInstance agent)
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
                SeatbeltMenu(input);
            } while (!exit);
        }

        private void SeatbeltMenu(string input)
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
                        case "set fullmode":
                            fullmode = GetParsedSetBool(input);
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
                if (fullmode)
                {
                    args.Add("full");
                }
            }

            RunAssembly(PL_MODULE_SEATBELT, "Seatbelt.Program", args.ToArray(), agent);
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "checkname", checkname },
                { "fullmode", fullmode.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
