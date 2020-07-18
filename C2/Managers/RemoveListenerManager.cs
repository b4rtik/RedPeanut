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
    class RemoveListenerManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set listenername", "Set listener to Remove" },
            { "remove", "Remove listener" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        C2Server server = null;

        IAgentInstance agent = null;
        string modulename = "removelistener";
        string listenername;

        bool exit = false;

        public RemoveListenerManager(C2Server server)
        {
            this.server = server;
        }

        public RemoveListenerManager(IAgentInstance agent)
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
                        case "remove":
                            if(Program.GetC2Manager().GetC2Server().GetListenersConfig().ContainsKey(listenername) )
                            {
                                try
                                {
                                    ListenerConfig lc = Program.GetC2Manager().GetC2Server().GetListenersConfig()[listenername];
                                    lc.CancellationTokenSource.Cancel();
                                    Program.GetC2Manager().GetC2Server().RemoveListenerConfig(lc);
                                }catch(Exception)
                                {}
                                exit = true;
                            }
                            break;
                        case "set listenername":
                            listenername = GetParsedSetString(input);
                            break;
                        case "options":
                            PrintOptionsNoStd("options", mainmenu);
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
                    PrintOptionsNoStd("Command not found", mainmenu);
                }
            }
        }
        
    }
}
