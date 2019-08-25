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
    class ListenerManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "http", "Http listener" },
            { "list", "Print listenets list" },
            { "kill", "Stop a listener" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        string modulename = "listener";

        C2Server srv = null;
        HttpServerManager httpmanager = null;
        StopListenerManager stopmanager = null;

        bool exit = false;

        public ListenerManager(C2Server srv)
        {
            this.srv = srv;
        }

        public void Execute()
        {
            exit = false;
            string input = "";
            SetAutoCompletionHandler(mainmenu);
            do
            {
                input = RedPeanutCLI(null, modulename);
                ServerMenu(input);
            } while (!exit);
        }

        private void ServerMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "http":
                            if (httpmanager == null)
                                httpmanager = new HttpServerManager(srv);
                            Program.GetMenuStack().Push(httpmanager);
                            exit = true;
                            return;
                        case "list":
                            srv.ListListeners();
                            break;
                        case "kill":
                            if (stopmanager == null)
                                stopmanager = new StopListenerManager(srv);
                            Program.GetMenuStack().Push(stopmanager);
                            exit = true;
                            break;
                        case "options":
                            PrintOptions("Options", mainmenu);
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
    }
}
