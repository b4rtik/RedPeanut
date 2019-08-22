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
    class RedPeanutManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "launcher", "Launcher manager" },
            { "c2", "C2 Server" },
            { "list", "List modules available" },
            { "exit", "Bye" }
        };   
        
        static C2Manager c2m = null;
        static LauncherManager launcherm = null;

        static bool exit = false;

        public RedPeanutManager(string serverkey)
        {

        }

        public void Execute()
        {
            exit = false;
            string input = null;
            SetAutoCompletionHandler(mainmenu);

            do
            {
                input = RedPeanutCLI();
                MainMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
            
        }
        
        static void MainMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch(input)
                    {
                        case "launcher":
                            if (launcherm == null)
                                launcherm = new LauncherManager(null);
                            Program.GetMenuStack().Push(launcherm);
                            exit = true;
                            break;
                        case "c2":
                            if (c2m == null)
                                c2m = new C2Manager();
                            Program.GetMenuStack().Push(c2m);
                            exit = true;
                            break;
                        case "list":
                            PrintOptions("Modules availlable", mainmenu);
                            break;
                        case "exit":
                            exit = true;
                            Console.WriteLine("See you soon");
                            ShutDown();
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
