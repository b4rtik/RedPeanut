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
    class RubeusTriageManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set user", "Set user" },
            { "set luid", "Set luid logonid" },
            { "set service", "Set service ldap" },
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
        string modulename = "triage";
        string user;
        string luid;
        string service;

        public RubeusTriageManager()
        {

        }

        public RubeusTriageManager(IAgentInstance agent)
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
                        case "set user":
                            user = GetParsedSetString(input);
                            break;
                       case "set luid":
                            luid = GetParsedSetString(input);
                            break;
                        case "set service":
                            service = GetParsedSetString(input);
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
            args.Add("triage");

            if (!string.IsNullOrEmpty(user))
            {
                args.Add("/user:" + user);
            }

            if (!string.IsNullOrEmpty(luid))
            {
                args.Add("/luid:" + luid);
            }
            
            if (!string.IsNullOrEmpty(service))
            {
                args.Add("/service:" + service);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "user", user },
                { "luid", luid },
                { "service", service }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

