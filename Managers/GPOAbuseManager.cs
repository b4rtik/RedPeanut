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
            { "adduserrights", "Set pipe name" },
            { "addlocaladmin", "Start server" },
            { "addstartupscript", "Start server" },
            { "addimmediatetask", "Start server" },
            { "options", "Print help" },
            { "back", "Back to main menu" }
        };

        IAgentInstance agent = null;
        string modulename = "gpoabuse";
        static SharpGPOAddImmediateTaskManager taskm = null;
        static SharpGPOAddLocalAdminManager locadminm = null;
        static SharpGPOAddStartupScriptManager starupscrm = null;
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
                        case "addstartupscript":
                            starupscrm = new SharpGPOAddStartupScriptManager(agent);
                            Program.GetMenuStack().Push(starupscrm);
                            exit = true;
                            break;
                        case "addimmediatetask":
                            taskm = new SharpGPOAddImmediateTaskManager(agent);
                            Program.GetMenuStack().Push(taskm);
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
