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
    public class RubeusManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "asktgt", "Retrieve a service ticket for one or more SPNs, optionally applying the ticket" },
            { "renew", "Renew a TGT, optionally applying the ticket or auto-renewing the ticket up to its renew-till limit" },
            { "s4u", "Perform S4U constrained delegation abuse" },
            { "ptt", "Submit a TGT, optionally targeting a specific LUID (if elevated)" },
            { "purge", "Purge tickets from the current logon session, optionally targeting a specific LUID (if elevated)" },
            { "describe", "Parse and describe a ticket (service ticket or TGT)" },
            { "triage", "Triage all current tickets (if elevated, list for all users), optionally targeting a specific LUID, username, or service" },
            { "klist", "List all current tickets in detail (if elevated, list for all users), optionally targeting a specific LUID" },
            { "dump", "Dump all current ticket data (if elevated, dump for all users), optionally targeting a specific service/LUID" },
            { "tgtdeleg", "Retrieve a usable TGT .kirbi for the current user (w/ session key) without elevation by abusing the Kerberos GSS-API, faking delegation" },
            { "monitor", "Monitor every SECONDS (default 60) for 4624 logon events and dump any TGT data for new logon" },
            { "harvest", "Monitor every MINUTES (default 60) for 4624 logon events, dump any new TGT data, and auto-renew TGTs" },
            { "kerberoast", "Perform Kerberoasting" },
            { "asreproast", "Perform AS-REP \"roasting\"" },
            { "createnetonly", "Create a hidden program (unless /show is passed) with random /netonly credentials, displaying the PID and LUID" },
            { "changepw", "Reset a user's password from a supplied TGT (AoratoPw)" },
            { "hash", "Calculate rc4_hmac, aes128_cts_hmac_sha1, aes256_cts_hmac_sha1, and des_cbc_md5 hashes" },
            { "list", "Print help" },
            { "back", "Back to main menu" }
        };

        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        IAgentInstance agent = null;
        string modulename = "rubeus";
        static RubeusAskTgtManager asktgtm = null;
        static RubeusRenewManager renewm = null;
        static RubeusS4UManager s4um = null;
        static RubeusPttManager pttm = null;
        static RubeusPurgeManager purgem = null;
        static RubeusDescribeManager describem = null;
        static RubeusTriageManager triagem = null;
        static RubeusKlistManager klistm = null;
        static RubeusDumpManager dumpm = null;

        static RubeusTgtDelegManager tgtdelegem = null;
        static RubeusMonitorManager monitorm = null;
        static RubeusHarvestManager harvestm = null;
        static RubeusKerberoastManager kerberoastm = null;
        static RubeusASREPRoastManager arsrepm = null;

        static RubeusCreateNetOnlyManager createnetonlym = null;
        static RubeusChangePwManager changepwm = null;
        static RubeusHashManager hashm = null;

        static bool exit = false;


        public RubeusManager()
        {

        }

        public RubeusManager(IAgentInstance agent)
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
                RubeusMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void RubeusMenu(string input)
        {

            string f_input = ParseSelection(input);

            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(f_input.TrimEnd()))
                {
                    switch (f_input.TrimEnd())
                    {
                        case "asktgt":
                            asktgtm = new RubeusAskTgtManager(agent);
                            Program.GetMenuStack().Push(asktgtm);
                            exit = true;
                            break;
                        case "renew":
                            renewm = new RubeusRenewManager(agent);
                            Program.GetMenuStack().Push(renewm);
                            exit = true;
                            break;
                        case "s4u":
                            s4um = new RubeusS4UManager(agent);
                            Program.GetMenuStack().Push(s4um);
                            exit = true;
                            break;
                        case "ptt":
                            pttm = new RubeusPttManager(agent);
                            Program.GetMenuStack().Push(pttm);
                            exit = true;
                            break;
                        case "purge":
                            purgem = new RubeusPurgeManager(agent);
                            Program.GetMenuStack().Push(purgem);
                            exit = true;
                            break;
                        case "describe":
                            describem = new RubeusDescribeManager(agent);
                            Program.GetMenuStack().Push(describem);
                            exit = true;
                            break;
                        case "triage":
                            triagem = new RubeusTriageManager(agent);
                            Program.GetMenuStack().Push(triagem);
                            exit = true;
                            break;
                        case "klist":
                            klistm = new RubeusKlistManager(agent);
                            Program.GetMenuStack().Push(klistm);
                            exit = true;
                            break;
                        case "dump":
                            dumpm = new RubeusDumpManager(agent);
                            Program.GetMenuStack().Push(dumpm);
                            exit = true;
                            break;
                        case "tgtdelege":
                            tgtdelegem = new RubeusTgtDelegManager(agent);
                            Program.GetMenuStack().Push(tgtdelegem);
                            exit = true;
                            break;
                        case "monitor":
                            monitorm = new RubeusMonitorManager(agent);
                            Program.GetMenuStack().Push(monitorm);
                            exit = true;
                            break;
                        case "harvest":
                            harvestm = new RubeusHarvestManager(agent);
                            Program.GetMenuStack().Push(harvestm);
                            exit = true;
                            break;
                        case "kerberoast":
                            kerberoastm = new RubeusKerberoastManager(agent);
                            Program.GetMenuStack().Push(kerberoastm);
                            exit = true;
                            break;
                        case "arsrep":
                            arsrepm = new RubeusASREPRoastManager(agent);
                            Program.GetMenuStack().Push(arsrepm);
                            exit = true;
                            break;
                        case "createnetonly":
                            createnetonlym = new RubeusCreateNetOnlyManager(agent);
                            Program.GetMenuStack().Push(createnetonlym);
                            exit = true;
                            break;
                        case "changepw":
                            changepwm = new RubeusChangePwManager(agent);
                            Program.GetMenuStack().Push(changepwm);
                            exit = true;
                            break;
                        case "hash":
                            hashm = new RubeusHashManager(agent);
                            Program.GetMenuStack().Push(hashm);
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
