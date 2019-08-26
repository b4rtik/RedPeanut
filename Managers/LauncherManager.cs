//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class LauncherManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "exe", "Build an agent in exe format" },
            { "exepivot", "Build a pivot agent in exe format (Debug)" },
            { "dll", "Build an agent in dll format" },
            { "powershell", "Generate a powershell launcher" },
            { "hta", "Generate a hta launcher" },
            { "vba", "Generate a vba macro launcher" },
            { "msbuild", "Generate a msbuild launcher" },
            { "installutil", "Generate a dll install/unistall launcher" },
            { "evilclippy", "Generate malicious MS Office documents" },
            { "list", "List options" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        LauncherExeManager launcherexemanager;
        LauncherExePipeManager launcherExePipeManager;
        LauncherDllManager launcherdllmanager;
        LauncherHtaManager launcherhtamanager;
        LauncherPowershellManager launcherpsmanager;
        LauncherMSBuildManager launchermsbuildmanager;
        LauncherInstallUtilManager launcherinstallutilmanager;
        EvilClippyManager evilclippym;
        LauncherVBAManager launcherVBAmanager;

        IAgentInstance agent = null;
        string modulename = "launcher";

        bool exit = false;

        public LauncherManager()
        {

        }

        public LauncherManager(IAgentInstance agent)
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
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void LauncherMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "exe":
                            launcherexemanager = new LauncherExeManager(agent);
                            Program.GetMenuStack().Push(launcherexemanager);
                            exit = true;
                            break;
                        case "exepivot":
                            launcherExePipeManager = new LauncherExePipeManager(agent);
                            Program.GetMenuStack().Push(launcherExePipeManager);
                            exit = true;
                            break;
                        case "dll":
                            launcherdllmanager = new LauncherDllManager(agent);
                            Program.GetMenuStack().Push(launcherdllmanager);
                            exit = true;
                            break;
                        case "powershell":
                            launcherpsmanager = new LauncherPowershellManager(agent);
                            Program.GetMenuStack().Push(launcherpsmanager);
                            exit = true;
                            break;
                        case "hta":
                            launcherhtamanager = new LauncherHtaManager(agent);
                            Program.GetMenuStack().Push(launcherhtamanager);
                            exit = true;
                            break;
                        case "vba":
                            launcherVBAmanager = new LauncherVBAManager(agent);
                            Program.GetMenuStack().Push(launcherVBAmanager);
                            exit = true;
                            break;
                        case "msbuild":
                            launchermsbuildmanager = new LauncherMSBuildManager(agent);
                            Program.GetMenuStack().Push(launchermsbuildmanager);
                            exit = true;
                            break;
                        case "installutil":
                            launcherinstallutilmanager = new LauncherInstallUtilManager(agent);
                            Program.GetMenuStack().Push(launcherinstallutilmanager);
                            exit = true;
                            break;
                        case "evilclippy":
                            evilclippym = new EvilClippyManager(agent);
                            Program.GetMenuStack().Push(evilclippym);
                            exit = true;
                            break;
                        case "list":
                            PrintOptionsNoStd("List", mainmenu);
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
                    PrintOptionsNoStd("Command not found", mainmenu);
                }
            }
        }

        
    }
}
