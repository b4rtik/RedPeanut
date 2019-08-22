//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class ModulesManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "credential", "Spawn and inject a process" },
            { "exploit", "Exploit module" },
            { "privesc", "PrivEsc module" },
            { "recon", "Information gatering" },
            { "lateral", "Lateral movement" },
            { "process", "Process creation" },
            { "persistence", "Persistence manager" },
            { "powershellexecuter", "Run PowerShell command" },
            { "list", "List modules available" },
            { "back", "Back to main" }
        };

        IAgentInstance agent = null;
        string modulename = "module";

        static ReconManager reconm = null;
        static LateralManager lateral = null;
        static PrivEscManager privescm = null;
        static CredentialManager credm = null;
        static PowerShellExecuterManager powerm = null;
        static PersistenceManager persm = null;
        static SpawningManager spawnm = null;

        bool exit = false;

        public ModulesManager(IAgentInstance agent)
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
                input = RedPeanutCLI(agent,modulename);
                ModuleMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void ModuleMenu(string input)
        {

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "credential":
                            credm = new CredentialManager(agent);
                            Program.GetMenuStack().Push(credm);
                            exit = true;
                            break;
                        case "privesc":
                            privescm = new PrivEscManager(agent);
                            Program.GetMenuStack().Push(privescm);
                            exit = true;
                            break;
                        case "recon":
                            reconm = new ReconManager(agent);
                            Program.GetMenuStack().Push(reconm);
                            exit = true;
                            break;
                        case "lateral":
                            lateral = new LateralManager(agent);
                            Program.GetMenuStack().Push(lateral);
                            exit = true;
                            break;
                        case "process":
                            spawnm = new SpawningManager(agent);
                            Program.GetMenuStack().Push(spawnm);
                            exit = true;
                            break;
                        case "powershellexecuter":
                            powerm = new PowerShellExecuterManager(agent);
                            Program.GetMenuStack().Push(powerm);
                            exit = true;
                            break;
                        case "persistence":
                            persm = new PersistenceManager(agent);
                            Program.GetMenuStack().Push(persm);
                            exit = true;
                            break;
                        case "list":
                            PrintOptions("Modules availlable", mainmenu);
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
