//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class AgentManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "module", "Modules manager" },
            { "shell", "Command line" },
            { "pivot", "Enable agent pivot mode" },
            { "upload", "Upload file" },
            { "download", "Download file" },
            { "list", "List agents" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, null);
            return;
        }

        IAgentInstance agent = null;
        ShellManager shellm = null;
        ModulesManager modulem = null;
        PivotManager pivotm = null;
        UpLoadManager uploadm = null;
        DownLoadManager downloadm = null;

        bool exit = false;

        public AgentManager(IAgentInstance agent)
        {
            this.agent = agent;
        }

        public void Execute()
        {
            exit = false;
            string input = null;
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(agent);
                MainMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);

        }

        private void MainMenu(string input)
        {
            
            string[] a_input = input.Split(' ');
            string f_input = "";
            for (int i = 0; i < 2 && i < a_input.Length; i++)
                f_input += a_input[i] + " ";

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "module":
                            modulem = new ModulesManager(agent);
                            Program.GetMenuStack().Push(modulem);
                            exit = true;
                            break;
                        case "shell":
                            shellm = new ShellManager(agent);
                            Program.GetMenuStack().Push(shellm);
                            exit = true;
                            break;
                        case "pivot":
                            pivotm = new PivotManager(agent);
                            Program.GetMenuStack().Push(pivotm);
                            exit = true;
                            break;
                        case "upload":
                            uploadm = new UpLoadManager(agent);
                            Program.GetMenuStack().Push(uploadm);
                            exit = true;
                            break;
                        case "download":
                            downloadm = new DownLoadManager(agent);
                            Program.GetMenuStack().Push(downloadm);
                            exit = true;
                            break;
                        case "list":
                            PrintOptions("Options availlable", mainmenu);
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
