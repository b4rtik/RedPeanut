//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class PersistenceManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "autorun", "Autorun" },
            { "startup", "Startup" },
            { "wmi", "Wmi" },
            { "list", "List module" },
            { "back", "Back to main menu" }
        };


        PersAutorunManager registrym;
        PersStartupManager startupm;
        PersWMIManager wmim;
        IAgentInstance agent = null;
        string modulename = "persistence";

        bool exit = false;

        public PersistenceManager()
        {

        }

        public PersistenceManager(IAgentInstance agent)
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
                PersistenceMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void PersistenceMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "autorun":                      
                            registrym = new PersAutorunManager(agent);
                            Program.GetMenuStack().Push(registrym);
                            exit = true;
                            break;
                        case "startup":
                            startupm = new PersStartupManager(agent);
                            Program.GetMenuStack().Push(startupm);
                            exit = true;
                            break;
                        case "wmi":
                            wmim = new PersWMIManager(agent);
                            Program.GetMenuStack().Push(wmim);
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
