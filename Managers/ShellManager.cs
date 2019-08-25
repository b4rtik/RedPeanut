//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using static RedPeanut.Utility;
using System.Collections.Generic;
using static RedPeanut.Models;

namespace RedPeanut
{

    public class ShellManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "shell";

        bool exit = false;

        public ShellManager(IAgentInstance agent )
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
                input = RedPeanutCLI(agent, modulename);
                Run(input);
                if(input != "back")
                {
                    Program.GetMenuStack().Pop();
                    exit = true;
                }
            } while (!exit);

        }
        
        private void Run(string input)
        {
            if(!string.IsNullOrEmpty(input) && !input.Equals("back"))
            {
                if (agent != null)
                {
                    RunRemote(input);
                }
            }
        }

        private void RunRemote(string input)
        {
            string command = input;
            CommandConfig cmdconfig = new CommandConfig
            {
                Command = command
            };
            TaskMsg task = new TaskMsg
            {
                TaskType = "command",
                CommandTask = cmdconfig,
                Agentid = agent.AgentId
            };
            if (agent.Pivoter != null)
                task.AgentPivot = agent.Pivoter.AgentId;
            agent.SendCommand(task);
        }
    }
}
