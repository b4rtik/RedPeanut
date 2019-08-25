//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Linq;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class CheckedInAgentsManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "interact", "INterac t with session" },
            { "list", "List agents" },
            { "options", "List modules available" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(null, modulename);
            return;
        }

        C2Server server = null;

        Dictionary<string, AgentManager> insteractList = new Dictionary<string, AgentManager>();
        
        string modulename = "agents";

        bool exit = false;

        public CheckedInAgentsManager(C2Server server)
        {
            this.server = server;
        }

        public void Execute()
        {
            exit = false;
            string input = null;

            AutoCompletionHandlerC2ServerManager autocomp = new AutoCompletionHandlerC2ServerManager();

            if(server.GetAgents() != null)
                autocomp.AgentIdList = server.GetAgents().Keys.ToArray();

            autocomp.menu = mainmenu;
            ReadLine.AutoCompletionHandler = autocomp;

            do
            {
                input = RedPeanutCLI(null, modulename);

                MainMenu(input);

                if (server.GetAgents() != null)
                    autocomp.AgentIdList = server.GetAgents().Keys.ToArray();

                ReadLine.AutoCompletionHandler = autocomp;

            } while (!exit);

        }

        private void MainMenu(string input)
        {
            
            string[] input_f = input.Split(' ');

            if (mainmenu.ContainsKey(input_f[0]) || input.Equals(""))
            {
                
                switch (input_f[0])
                {
                    case "interact":
                        if (input_f.Length == 2)
                        {
                            if (server.CheckSessionExists(input_f[1]))
                            {
                                if(insteractList.ContainsKey(input_f[1]))
                                {
                                    AgentManager agentm;
                                    insteractList.TryGetValue(input_f[1], out agentm);
                                    Program.GetMenuStack().Push(agentm);
                                    exit = true;
                                }
                                else
                                {
                                    AgentManager agentm = new AgentManager(server.GetAgent(input_f[1]));
                                    insteractList.Add(input_f[1], agentm);
                                    Program.GetMenuStack().Push(agentm);
                                    exit = true;
                                }
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Session not exists");
                                break;
                            }
                        }
                        break;
                    case "list":
                        server.ListAgents();
                        break;
                    case "options":
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
