//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    class StopListenerManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set listenername", "Set listener to stop" },
            { "stop", "Stop listener" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        C2Server server = null;

        IAgentInstance agent = null;
        string modulename = "stoplistener";
        string listenername;

        bool exit = false;

        public StopListenerManager(C2Server server)
        {
            this.server = server;
        }

        public StopListenerManager(IAgentInstance agent)
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
                ListenerMenu(input);
            } while (!exit);
        }

        private void ListenerMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "stop":
                            if(Program.GetC2Manager().GetC2Server().GetListenersConfig().ContainsKey(listenername) )
                            {
                                ListenerConfig lc = Program.GetC2Manager().GetC2Server().GetListenersConfig()[listenername];
                                lc.CancellationTokenSource.Cancel();
                                Program.GetC2Manager().GetC2Server().GetListenersConfig().Remove(listenername);
                                exit = true;
                            }
                            break;
                        case "set listenername":
                            listenername = GetParsedSetString(input);
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
        
    }
}
