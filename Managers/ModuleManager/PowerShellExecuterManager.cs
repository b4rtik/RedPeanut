//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using static RedPeanut.Utility;

namespace RedPeanut
{
    class PowerShellExecuterManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set command", "One liner command to executed" },
            { "set file", "Local path to ps1 file" },
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
        string modulename = "powershellexecuter";

        string command = "";
        string file = "";

        bool exit = false;

        public PowerShellExecuterManager()
        {

        }

        public PowerShellExecuterManager(IAgentInstance agent)
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
                PowerShellMenu(input);
            } while (!exit);
        }

        private void PowerShellMenu(string input)
        {
            string f_input = ParseSelection(input);
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    if (mainmenu.ContainsKey(f_input.TrimEnd()))
                    {
                        switch (f_input.TrimEnd())
                        {
                            case "run":
                                Run();
                                break;
                            case "set command":
                                command = GetParsedSetStringMulti(input);
                                break;
                            case "set file":
                                file = GetParsedSetFilePath(input);
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
            catch (Exception e)
            {
                Console.WriteLine("[*] Error running task build {0}", e.Message);
                return;
            }
        }

        private void Run()
        {
            
            if (string.IsNullOrEmpty(command) && !File.Exists(file))
                return;

            try
            {
                string commandstr = "";
                if(File.Exists(file))
                {
                    commandstr = File.ReadAllText(file);
                }else
                {
                    commandstr = command;
                }

                // Parse .cs sorce and repalce variable
                string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);

                string source = File.ReadAllText(Path.Combine(folderrpath, POWERSHELLEXECUTER_TEMPLATE));
                //source = ProfileReplacer.ReplaceFileUpLoad(source, filesrc, destinationpath, destinationfilename, username, password, domainname);

                string assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll",40)));

                RunAssemblyBase64(assembly, "PowerShellExecuter", new string[] { commandstr }, agent);
            }
            catch(Exception )
            {
                Console.WriteLine("[*] Error creating task");
            }
            
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "command", command },
                { "file", file }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
