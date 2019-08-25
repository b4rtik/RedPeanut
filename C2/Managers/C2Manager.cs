//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using static RedPeanut.Utility;
using System.Collections.Generic;

namespace RedPeanut
{
    class C2Manager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "agents", "Manage agents" },
            { "listener", "Manage listener" },
            { "options", "Print help" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        string modulename = "C2";
        static C2Server srv = null;
        static CheckedInAgentsManager agentsm = null;
        static ListenerManager serverm = null;

        bool exit = false;

        public C2Manager()
        {

        }

        public void Execute()
        {
            exit = false;
            string input;
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(null,modulename);
                C2Menu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        public void CreateC2Server(string serverkey)
        {
            srv = new C2Server(serverkey);
        }

        public C2Server GetC2Server()
        {
            return srv;
        }

        private void C2Menu(string input)
        {

            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "agents":
                            agentsm = new CheckedInAgentsManager(srv);
                            Program.GetMenuStack().Push(agentsm);
                            exit = true;
                            return;
                        case "listener":
                            if (serverm == null)
                                serverm = new ListenerManager(srv);
                            Program.GetMenuStack().Push(serverm);
                            exit = true;
                            return;
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

        public string GetServerKey()
        {
            return srv.GetServerKey();
        }
    }
}
