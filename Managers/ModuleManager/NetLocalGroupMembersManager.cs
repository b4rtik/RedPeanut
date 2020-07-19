//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static RedPeanut.Models;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class NetLocalGroupMembersManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set computername", "Target computer" },
            { "set group", "Target group, default Administrators" },
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
        string modulename = "netlocalgroupmembers";
        string computername = "";
        string group = "Administrators";

        bool exit = false;

        public NetLocalGroupMembersManager()
        {

        }

        public NetLocalGroupMembersManager(IAgentInstance agent)
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
                WmiMenu(input);
            } while (!exit);
        }

        private void WmiMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set computername":
                            computername = GetParsedSetString(input);
                            break;
                        case "set group":
                            group = GetParsedSetString(input);
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
            string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);

            if (string.IsNullOrEmpty(computername))
            {
                Console.WriteLine("Must provide computer name or ip address of the target");
                return;
            }

            try
            {
                string source = File.ReadAllText(Path.Combine(folderrpath, DOMAIN_RECON));

                string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.SSploitEnumerationDomain)));

                RunStandardBase64(commandstr, "GetNetLocalGroupMembers", "SharpSploitDomainReconImpl.Program", new string[] { computername.Trim(), group }, agent);
            }
            catch (Exception)
            {
                Console.WriteLine("[*] Errore generating task");
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "computername", computername }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
