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
    public class SharpCOMManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set method", "Set method: ShellWindows,MMC,ShellBrowserWindow,ExcelDDE" },
            { "set computername", "Set target computer name" },
            { "set command", "Set command to run" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "sharpcom";
        string method = "";
        string computername = "";
        string command = "";

        bool exit = false;

        public SharpCOMManager()
        {

        }

        public SharpCOMManager(IAgentInstance agent)
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
                DCOMMenu(input);
            } while (!exit);
        }

        private void DCOMMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set method":
                            method = GetParsedSetString(input);
                            break;
                        case "set computername":
                            computername = GetParsedSetString(input);
                            break;
                       case "set command":
                            command = GetParsedSetStringMulti(input);
                            break;
                        case "run":
                            Run();
                            break;
                        case "options":
                            PrintCurrentConfig();
                            break;
                        case "info":
                            PrintOptions("info", mainmenu);
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
            List<string> args = new List<string>();
            if (!string.IsNullOrEmpty(method))
                args.Add("--Method " + method);
            if (!string.IsNullOrEmpty(computername))
                args.Add("--ComputerName " + computername);
            if (!string.IsNullOrEmpty(command))
                args.Add("--ComputerName " + command);
            string s = "";
            foreach (string ss in args.ToArray<string>())
                s += ss;
            Console.WriteLine("String command: " + s);
            RunAssembly(PL_MODULE_SHARPCOM, "SharpCOM.Program", args.ToArray<string>(), agent);
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "method", method },
                { "computername", computername },
                { "command", command }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
