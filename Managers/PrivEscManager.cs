//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class PrivEscManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "sharpup", "SharpUp" },
            { "uacbypass", "UAC bypass via token manipulation" },
            { "list", "List module available" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        SharpUpManager sharpupmanager;
        UACTokenManipulationManager uacbypassm;

        IAgentInstance agent = null;
        string modulename = "privesc";

        bool exit = false;

        public PrivEscManager()
        {

        }

        public PrivEscManager(IAgentInstance agent)
        {
            this.agent = agent;
        }

        public void Execute()
        {
            string input;
            SetAutoCompletionHandler(mainmenu);
            do
            {
                exit = false;
                input = RedPeanutCLI(agent, modulename);
                PrivEscMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void PrivEscMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "sharpup":
                            sharpupmanager = new SharpUpManager(agent);
                            Program.GetMenuStack().Push(sharpupmanager);
                            exit = true;
                            break;
                        case "uacbypass":
                            uacbypassm = new UACTokenManipulationManager(agent);
                            Program.GetMenuStack().Push(uacbypassm);
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
