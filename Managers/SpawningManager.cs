//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class SpawningManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "ppidagent", "SharpWmi" },
            { "spawnasagent", "SharpPsExec" },
            { "spawnshellcode", "Run task on remote host via wmi and msbuild" },
            { "spawnasshellcode", "Run task on remote host via wmi and msbuild" },
            { "list", "List module" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        SpawnAsAgentManager spawnAsAgentManager;
        SpawnAsShellcodeManager spawnAsShellcode;
        SpawnPPIDAgentManager spawnPPIDAgent;
        SpawnShellcodeManager spawnShellcode;
        IAgentInstance agent = null;
        string modulename = "process";

        bool exit = false;

        public SpawningManager()
        {

        }

        public SpawningManager(IAgentInstance agent)
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
                SpawningMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void SpawningMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "ppidagent":
                            spawnPPIDAgent = new SpawnPPIDAgentManager(agent);
                            Program.GetMenuStack().Push(spawnPPIDAgent);
                            exit = true;
                            break;
                        case "spawnasagent":
                            spawnAsAgentManager = new SpawnAsAgentManager(agent);
                            Program.GetMenuStack().Push(spawnAsAgentManager);
                            exit = true;
                            break;
                        case "spawnshellcode":
                            spawnShellcode = new SpawnShellcodeManager(agent);
                            Program.GetMenuStack().Push(spawnShellcode);
                            exit = true;
                            break;
                        case "spawnasshellcode":
                            spawnAsShellcode = new SpawnAsShellcodeManager(agent);
                            Program.GetMenuStack().Push(spawnAsShellcode);
                            exit = true;
                            break;
                        case "list":
                            PrintOptions("List", mainmenu);
                            break;
                        case "back":
                            Program.GetMenuStack().Pop();
                            exit = true;
                            return;
                        default:
                            break;
                    }
                }
                else
                {
                    PrintOptions("Command not found", mainmenu);
                }
            }
        }
    }
}
