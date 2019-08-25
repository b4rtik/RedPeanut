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
    public class SharpDPAPIManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "backupkey", "Run backupkey command" },
            { "masterkeys", "Run masterkeys command" },
            { "machinemasterkeys", "Run machinemasterkeys command" },
            { "machinecredentials", "Run machinecredentials command" },
            { "vaults", "Run vaults command" },
            { "machinevaults", "Run machinevaults command" },
            { "rdg", "Run rdg command" },
            { "triage", "Run triage command" },
            { "machinetriage", "Run machinetriage command" },
            { "options", "Print help" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "sharpdpapi";
        static SharpDPAPIBackupKeyManager backupkeym = null;
        static SharpDPAPIMasterKeysManager masterkeysm = null;
        static SharpDPAPIMachineMasterKeysManager machinemasterkeysm = null;
        static SharpDPAPICredentialsManager credentialsm = null;
        static SharpDPAPIMachineCredentialsManager machinecredentialsm = null;
        static SharpDPAPIVaultsManager vaultsm = null;
        static SharpDPAPIMachineVaultsManager machinevaultsm = null;
        static SharpDPAPIRdgManager rdgm = null;
        static SharpDPAPITriageManager triagem = null;
        static SharpDPAPIMachineTriageManager machinetriagem = null;

        static bool exit = false;


        public SharpDPAPIManager()
        {

        }

        public SharpDPAPIManager(IAgentInstance agent)
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
                DPAPIMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void DPAPIMenu(string input)
        {

            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "backupkey":
                            backupkeym = new SharpDPAPIBackupKeyManager(agent);
                            Program.GetMenuStack().Push(backupkeym);
                            exit = true;
                            break;
                        case "masterkeys":
                            masterkeysm = new SharpDPAPIMasterKeysManager(agent);
                            Program.GetMenuStack().Push(masterkeysm);
                            exit = true;
                            break;
                        case "machinemasterkeys":
                            machinemasterkeysm = new SharpDPAPIMachineMasterKeysManager(agent);
                            Program.GetMenuStack().Push(machinemasterkeysm);
                            exit = true;
                            break;
                        case "credentials":
                            credentialsm = new SharpDPAPICredentialsManager(agent);
                            Program.GetMenuStack().Push(credentialsm);
                            exit = true;
                            break;
                        case "machinecredentials":
                            machinecredentialsm = new SharpDPAPIMachineCredentialsManager(agent);
                            Program.GetMenuStack().Push(machinecredentialsm);
                            exit = true;
                            break;
                        case "vaults":
                            vaultsm = new SharpDPAPIVaultsManager(agent);
                            Program.GetMenuStack().Push(vaultsm);
                            exit = true;
                            break;
                        case "machinevaults":
                            machinevaultsm = new SharpDPAPIMachineVaultsManager(agent);
                            Program.GetMenuStack().Push(machinevaultsm);
                            exit = true;
                            break;
                        case "rdg":
                            rdgm = new SharpDPAPIRdgManager(agent);
                            Program.GetMenuStack().Push(rdgm);
                            exit = true;
                            break;
                        case "triage":
                            triagem = new SharpDPAPITriageManager(agent);
                            Program.GetMenuStack().Push(triagem);
                            exit = true;
                            break;
                        case "machinetriage":
                            if (machinetriagem == null)
                                machinetriagem = new SharpDPAPIMachineTriageManager(agent);
                            Program.GetMenuStack().Push(machinetriagem);
                            exit = true;
                            break;
                        case "options":
                            PrintOptions("options", mainmenu);
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
    }
}
