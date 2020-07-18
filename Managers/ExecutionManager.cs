//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class ExecutionManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "execute-assembly", "Seatbelt" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        ExecuteAssemblyManager assemblymanager;

        IAgentInstance agent = null;
        string modulename = "execute";

        bool exit = false;

        public ExecutionManager()
        {

        }

        public ExecutionManager(IAgentInstance agent)
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
                ExecutionMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void ExecutionMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "execute-assembly":
                            assemblymanager = new ExecuteAssemblyManager(agent);
                            Program.GetMenuStack().Push(assemblymanager);
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
