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
    public class SharpWmiManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set action", "Action to use: query, create, executevbs" },
            { "set query", "Query text" },
            { "set computername", "Set target computer name" },
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "set namespace", "Set namespace" },
            { "set command", "Command to execute with action=create" },
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
        string modulename = "sharpwmi";
        string action = "";
        string query = "";
        string computername = "";
        string name = "";
        string password = "";
        string nameSpace = "";
        string command = "";

        bool exit = false;

        public SharpWmiManager()
        {

        }

        public SharpWmiManager(IAgentInstance agent)
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
                WmiMenu(input);
            } while (!exit);
        }

        private void WmiMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set action":
                            action = GetParsedSetString(input);
                            break;
                        case "set query":
                            query = GetParsedSetStringMulti(input);
                            break;
                        case "set computername":
                            computername = GetParsedSetString(input);
                            break;
                        case "set username":
                            name = GetParsedSetString(input);
                            break;
                        case "set password":
                            password = GetParsedSetString(input);
                            break;
                        case "set namespace":
                            nameSpace = GetParsedSetString(input);
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
            if (!string.IsNullOrEmpty(action))
                args.Add("action=" + action);
            if (!string.IsNullOrEmpty(query))
                args.Add("query=" + query);
            if (!string.IsNullOrEmpty(computername))
                args.Add("computername=" + computername);

            if (!string.IsNullOrEmpty(name))
                args.Add("username=" + name);
            if (!string.IsNullOrEmpty(password))
                args.Add("password=" + password);
            if (!string.IsNullOrEmpty(nameSpace))
                args.Add("namespace=" + nameSpace);
            if (!string.IsNullOrEmpty(command))
                args.Add("command=" + command);
            string s = "";
            foreach (string ss in args.ToArray<string>())
                s += ss;
            Console.WriteLine("String command: " + s);
            RunAssembly(PL_MODULE_SHARPWMI, "SharpWMI.Program", args.ToArray<string>(), agent);
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "action", action },
                { "query", query },
                { "computername", computername },
                { "username", name },
                { "password", password },
                { "command", command }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
