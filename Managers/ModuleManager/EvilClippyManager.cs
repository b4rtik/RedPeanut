//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class EvilClippyManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "set targetfile", "The target file" },
            { "set name", "The target module name to stomp" },
            { "set sourcefile", "File containing substitution VBA code (fake code)" },
            { "set guihide", "Hide code from VBA editor GUI" },
            { "set guiunhide", "Unhide code from VBA editor GUI" },
            { "set targetversion", "Target MS Office version the pcode will run on" },
            { "set delmetadata", "Remove metadata stream (may include your name etc.)" },
            { "set randomnames", "Set random module names, confuses some analyst tools" },
            { "set resetmodulenames", "Undo the set random module names by making the ASCII module names in the DIR stream match their Unicode counter parts" },
            { "set unviewableVBA", "Make VBA Project unviewable/locked" },
            { "set viewableVBA", "Make VBA Project viewable/unlocked" },
            { "set hosted", "Add resulting file to webserver resource and serve with specific controller" },
            { "run", "Execute module" },
            { "options", "Print current config" },
            { "info", "Print help" },
            { "back", "Back to lateral menu" }
        };

        IAgentInstance agent = null;
        string modulename = "evilclippy";
        string targetfile = "";
        string names = "";
        string sourcefile = "";
        bool guihide = false;
        bool guiunhide = false;
        string targetversion = "";
        bool delmetadata = false;
        bool randomnames = false;
        bool resetmodulenames = false;
        bool unviewableVBA = false;
        bool viewableVBA = false;

        bool hosted = false;

        bool exit = false;

        public EvilClippyManager()
        {

        }

        public EvilClippyManager(IAgentInstance agent)
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
                EvilClippyMenu(input);
            } while (!exit);
        }

        private void EvilClippyMenu(string input)
        {
            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {

                        case "set targetfile":
                            targetfile = GetParsedSetString(input);
                            break;
                        case "set sourcefile":
                            sourcefile = GetParsedSetString(input);
                            break;
                        case "set guihide":
                            guihide = GetParsedSetBool(input);
                            break;
                        case "set guiunhide":
                            guiunhide = GetParsedSetBool(input);
                            break;
                        case "set name":
                            names = GetParsedSetStringMulti(input);
                            break;
                        case "set targetversion":
                            targetversion = GetParsedSetString(input);
                            break;
                        case "set delmetadata":
                            delmetadata = GetParsedSetBool(input);
                            break;
                        case "set randomnames":
                            randomnames = GetParsedSetBool(input);
                            break;
                        case "set resetmodulenames":
                            resetmodulenames = GetParsedSetBool(input);
                            break;
                        case "set unviewableVBA":
                            unviewableVBA = GetParsedSetBool(input);
                            break;
                        case "set viewableVBA":
                            viewableVBA = GetParsedSetBool(input);
                            break;
                        case "set hosted":
                            hosted = GetParsedSetBool(input);
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
                if (!string.IsNullOrEmpty(targetfile))
                {
                    string filetowork = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, EVILCLIPPY_FOLDER, targetfile);

                    if(!File.Exists(filetowork))
                    {
                        Console.WriteLine("[x] File not found {0}", filetowork);
                        return;
                    }

                    string vbasrc = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, EVILCLIPPY_FOLDER, sourcefile);
                    if (!string.IsNullOrEmpty(sourcefile))
                    {
                        if (!File.Exists(vbasrc))
                            Console.WriteLine("[x] VBA source not found {0}", vbasrc);
                    
                    }

                    try
                    {
                        MSOfficeManipulator mSOfficeManipulator = new MSOfficeManipulator(filetowork,names.Split(' '));

                        List<string> args = new List<string>();

                        if (!string.IsNullOrEmpty(targetversion))
                            mSOfficeManipulator.SetTargetOfficeVersion(targetversion);
                        if (unviewableVBA)
                            mSOfficeManipulator.UnviewableVBA();
                        if (viewableVBA)
                            mSOfficeManipulator.ViewableVBA();
                        if (guihide)
                            mSOfficeManipulator.HideInGUI();
                        if (guiunhide)
                            mSOfficeManipulator.UnhideInGUI();

                        if (!string.IsNullOrEmpty(sourcefile))
                        {
                            mSOfficeManipulator.StompVBAModules(vbasrc);
                        }
                    
                        if (randomnames)
                            mSOfficeManipulator.SetRandomNames();
                        if (resetmodulenames)
                            mSOfficeManipulator.ResetModuleNames();
                        if (delmetadata)
                            mSOfficeManipulator.DeleteMetadata();

                        string outputfile = Path.GetFileName(mSOfficeManipulator.Commit());
                        Console.WriteLine("[*] Output file {0}", outputfile);
                        //Add resource to webserver if required
                        if (hosted)
                        {
                            //Add resource to webserver storage
                            C2Manager c2manager = Program.GetC2Manager();
                            c2manager.GetC2Server().RegisterWebResource(outputfile, new WebResourceInstance(null, outputfile));

                            Console.WriteLine("[*] File added to webserver resources /office/{0}", outputfile);
                        }
                    }
                    catch(Exception )
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[*] Error running task build {0}", e.Message);
                return;
            }
        }

        private void PrintCurrentConfig()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "targetfile", targetfile },
                { "name", names },
                { "sourcefile", sourcefile },
                { "guihide", guihide.ToString() },
                { "guiunhide", guiunhide.ToString() },
                { "targetversion", targetversion },
                { "delmetadata", delmetadata.ToString() },
                { "randomnames", randomnames.ToString() },
                { "resetmodulenames", resetmodulenames.ToString() },
                { "unviewableVBA", unviewableVBA.ToString() },
                { "viewableVBA", viewableVBA.ToString() },
                { "hosted", hosted.ToString() }
            };

            Utility.PrintCurrentConfig(modulename, properties);
        }
    }
}
