//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class GPOAbuseManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "adduserrights", "Add rights to a user" },
            { "addlocaladmin", "Add a user to the local admins group" },
            { "addcomputerscript", "Add a new computer startup script" },
            { "adduserscript", "Configure a user logon script" },
            { "addcomputertask", "Configure a computer immediate task" },
            { "addusertask", "Add an immediate task to a user" },
            { "options", "Print help" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "gpoabuse";
        static SharpGPOAddUserTaskManager usertaskm = null;
        static SharpGPOAddComputerTaskManager computertaskm = null;
        static SharpGPOAddLocalAdminManager locadminm = null;
        static SharpGPOAddUserScriptManager userscrm = null;
        static SharpGPOAddComputerScriptManager computerscrm = null;
        static SharpGPOAddUserRightsManager userrightm = null;

        bool exit = false;

        public GPOAbuseManager()
        {

        }

        public GPOAbuseManager(IAgentInstance agent)
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
                GpoMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void GpoMenu(string input)
        {

            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "adduserrights":
                            userrightm = new SharpGPOAddUserRightsManager(agent);
                            Program.GetMenuStack().Push(userrightm);
                            exit = true;
                            break;
                        case "addlocaladmin":
                            locadminm = new SharpGPOAddLocalAdminManager(agent);
                            Program.GetMenuStack().Push(locadminm);
                            exit = true;
                            break;
                        case "adduserscript":
                            userscrm = new SharpGPOAddUserScriptManager(agent);
                            Program.GetMenuStack().Push(userscrm);
                            exit = true;
                            break;
                        case "addcomputerscript":
                            computerscrm = new SharpGPOAddComputerScriptManager(agent);
                            Program.GetMenuStack().Push(computerscrm);
                            exit = true;
                            break;
                        case "addusertask":
                            usertaskm = new SharpGPOAddUserTaskManager(agent);
                            Program.GetMenuStack().Push(usertaskm);
                            exit = true;
                            break;
                        case "addcomputertask":
                            computertaskm = new SharpGPOAddComputerTaskManager(agent);
                            Program.GetMenuStack().Push(computertaskm);
                            exit = true;
                            break;
                        case "options":
                            PrintOptions("options", mainmenu);
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
    }
}
