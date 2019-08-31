//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class LateralManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "sharpwmi", "SharpWmi" },
            { "sharppsexec", "SharpPsExec" },
            { "sharpcom", "SharpCom" },
            { "lateralmsbuild", "Run task on remote host via wmi and msbuild" },
            { "list", "List module" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        SharpWmiManager wmimanager;
        SharpPsExecManager psexecmanager;
        LateralMSBuildManager msbuildm;
        SharpCOMManager sharpcomm;
        IAgentInstance agent = null;
        string modulename = "lateral";

        bool exit = false;

        public LateralManager()
        {

        }

        public LateralManager(IAgentInstance agent)
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
                LateralMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void LateralMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "sharpwmi":                      
                            wmimanager = new SharpWmiManager(agent);
                            Program.GetMenuStack().Push(wmimanager);
                            exit = true;
                            break;
                        case "sharppsexec":
                            psexecmanager = new SharpPsExecManager(agent);
                            Program.GetMenuStack().Push(psexecmanager);
                            exit = true;
                            break;
                        case "sharpcom":
                            sharpcomm = new SharpCOMManager(agent);
                            Program.GetMenuStack().Push(sharpcomm);
                            exit = true;
                            break;
                        case "lateralmsbuild":
                            msbuildm = new LateralMSBuildManager(agent);
                            Program.GetMenuStack().Push(msbuildm);
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
