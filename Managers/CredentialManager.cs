//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class CredentialManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "sharpdpapi", "SharpDPAPI" },
            { "safetykatz", "SafetyKatz" },
            { "sharpweb", "SharpWeb" },
            { "rubeus", "Rubeus" },
            { "list", "List module" },
            { "back", "Back to main menu" }
        };

        SharpDPAPIManager dpapimanager;
        SafetyKatzManager safetymanager;
        SharpWebManager sharpwebm;
        RubeusManager rubeusm;

        IAgentInstance agent = null;
        string modulename = "credential";

        bool exit = false;

        public CredentialManager()
        {

        }

        public CredentialManager(IAgentInstance agent)
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
                        case "sharpdpapi":
                            dpapimanager = new SharpDPAPIManager(agent);
                            Program.GetMenuStack().Push(dpapimanager);
                            exit = true;
                            break;
                        case "safetykatz":
                            safetymanager = new SafetyKatzManager(agent);
                            Program.GetMenuStack().Push(safetymanager);
                            exit = true;
                            break;
                        case "sharpweb":
                            sharpwebm = new SharpWebManager(agent);
                            Program.GetMenuStack().Push(sharpwebm);
                            exit = true;
                            break;
                        case "rubeus":
                            rubeusm = new RubeusManager(agent);
                            Program.GetMenuStack().Push(rubeusm);
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
