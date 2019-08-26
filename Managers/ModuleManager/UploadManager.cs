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
    class UpLoadManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filepath", "File to upload" },
            { "set pathdest", "File destination folder, if not provided file will be upoaded to current directory" },
            { "set filename", "File destination name, current file name if not provided" },
            { "run", "Execute module" },
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "upload";

        string filepath = "";
        string pathdest = "";
        string filename = "";
        string username = "";
        string password = "";
        string domain = "";

        bool exit = false;

        public UpLoadManager()
        {

        }

        public UpLoadManager(IAgentInstance agent)
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
                UpLoadMenu(input);
            } while (!exit);
        }

        private void UpLoadMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "run":
                            Run();
                            break;
                        case "set filepath":
                            filepath = GetParsedSetFilePath(input);
                            break;
                        case "set pathdest":
                            pathdest = GetParsedSetString(input);
                            break;
                        case "set filename":
                            filename = GetParsedSetString(input);
                            break;
                        case "set username":
                            username = GetParsedSetString(input);
                            break;
                        case "set password":
                            password = GetParsedSetString(input);
                            break;
                        case "set domain":
                            domain = GetParsedSetString(input);
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
            
            if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
                return;

            try
            {
                string filesrc = Convert.ToBase64String(CompressGZipAssembly(File.ReadAllBytes(filepath)));

                string destinationpath = ".";
                if (!string.IsNullOrEmpty(pathdest))
                    destinationpath = pathdest.Replace("\\", "\\\\");

                string destinationfilename = "";
                if (!string.IsNullOrEmpty(filename))
                    destinationfilename = filename;

                string domainname = ".";
                if (!string.IsNullOrEmpty(domain))
                    domainname = domain;

                // Parse .cs sorce and repalce variable
                string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);

                string source = File.ReadAllText(Path.Combine(folderrpath, FILEUPLOAD_TEMPLATE));
                source = Replacer.ReplaceFileUpLoad(source, filesrc, destinationpath, destinationfilename, username, password, domainname);

                string assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, "FileUpLoader.dll",40)));

                RunAssemblyBase64(assembly, "FileUpLoader", new string[] { "pippo" }, agent);
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
                { "filepath", filepath },
                { "pathdest", pathdest },
                { "filename", filename },
                { "username", username },
                { "password", password }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
