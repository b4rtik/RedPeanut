//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;
using static RedPeanut.Models;

namespace RedPeanut
{
    class PivotManager :IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set pipename", "Set pipe name" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "pivot";
        string pipename;

        bool exit = false;

        public PivotManager()
        {

        }

        public PivotManager(IAgentInstance agent)
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
                PivotMenu(input);
            } while (!exit);
        }

        private void PivotMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "run":
                            Run();
                            break;
                        case "set pipename":
                            pipename = GetParsedSetString(input);
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
        
        private void Run()
        {
            if(!string.IsNullOrEmpty(pipename))
            {
                TaskMsg msg = new TaskMsg();
                msg.Instanceid = RandomAString(10, new Random());
                msg.Agentid = agent.AgentId;
                msg.TaskType = "pivot";

                agent.SendCommand(msg);
            }
        }
    }
}
