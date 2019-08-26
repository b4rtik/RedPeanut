//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class ReconManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "seatbelt", "Seatbelt" },
            { "sharpadidnsdump", "SharpAdidnsdump" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        SeatbeltManager seatbeltmanager;
        SharpAdidnsdumpManager sharpadidnsdumpmanager;

        IAgentInstance agent = null;
        string modulename = "recon";

        bool exit = false;

        public ReconManager()
        {

        }

        public ReconManager(IAgentInstance agent)
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
                ReconMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void ReconMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "seatbelt":
                            seatbeltmanager = new SeatbeltManager(agent);
                            Program.GetMenuStack().Push(seatbeltmanager);
                            exit = true;
                            break;
                        case "sharpadidnsdump":
                            sharpadidnsdumpmanager = new SharpAdidnsdumpManager(agent);
                            Program.GetMenuStack().Push(sharpadidnsdumpmanager);
                            exit = true;
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
