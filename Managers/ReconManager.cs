//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Collections.Generic;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class ReconManager : IMenu
    {
        public static Dictionary<string, string> mainmenu = new Dictionary<string, string>
        {
            { "seatbelt", "Seatbelt" },
            { "sharpadidnsdump", "SharpAdidnsdump" },
            { "domainusers", "Get Domain Users data" },
            { "netsession", "Get session info data from a target computer" },
            { "netloggedonusers", "Get users with session on target computer" },
            { "netlocalgroupmembers", "Get users member of a specified local group on target computer" },
            { "netlocalgroups", "List local groups on target computer" },
            { "domaingroups", "List domain groups" },
            { "domaincomputers", "List domain computers" },
            { "kerberoast", "List crackable service tickets" },
            { "back", "Back to main menu" }
        };
        
        public void RePrintCLI()
        {
            Utility.RePrintCLI(agent, modulename);
            return;
        }

        SeatbeltManager seatbeltmanager;
        SharpAdidnsdumpManager sharpadidnsdumpmanager;
        DomainUsersManager domainusersmanager;
        NetLoggedOnUsersManager netLoggedOnUsersManager;
        NetSessionsManager netsessionmanager;
        NetLocalGroupMembersManager netLocalGroupMembersManager;
        NetLocalGroupsManager netLocalGroupsManager;
        DomainGroupsManager domainGroupsManager;
        DomainComputersManager domainComputersManager;
        KerberoastManager kerberoastManager;

        IAgentInstance agent = null;
        string modulename = "recon";

        bool exit = false;

        public ReconManager()
        {

        }

        public ReconManager(IAgentInstance agent)
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
                ReconMenu(input);
                SetAutoCompletionHandler(mainmenu);
            } while (!exit);
        }

        private void ReconMenu(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (mainmenu.ContainsKey(input))
                {
                    switch (input)
                    {
                        case "seatbelt":
                            seatbeltmanager = new SeatbeltManager(agent);
                            Program.GetMenuStack().Push(seatbeltmanager);
                            exit = true;
                            break;
                        case "sharpadidnsdump":
                            sharpadidnsdumpmanager = new SharpAdidnsdumpManager(agent);
                            Program.GetMenuStack().Push(sharpadidnsdumpmanager);
                            exit = true;
                            break;
                        case "domainusers":
                            domainusersmanager = new DomainUsersManager(agent);
                            Program.GetMenuStack().Push(domainusersmanager);
                            exit = true;
                            break;
                        case "netloggedonusers":
                            netLoggedOnUsersManager = new NetLoggedOnUsersManager(agent);
                            Program.GetMenuStack().Push(netLoggedOnUsersManager);
                            exit = true;
                            break;
                        case "netsession":
                            netsessionmanager = new NetSessionsManager(agent);
                            Program.GetMenuStack().Push(netsessionmanager);
                            exit = true;
                            break;
                        case "netlocalgroupmembers":
                            netLocalGroupMembersManager = new NetLocalGroupMembersManager(agent);
                            Program.GetMenuStack().Push(netLocalGroupMembersManager);
                            exit = true;
                            break;
                        case "netlocalgroups":
                            netLocalGroupsManager = new NetLocalGroupsManager(agent);
                            Program.GetMenuStack().Push(netLocalGroupsManager);
                            exit = true;
                            break;
                        case "domaingroups":
                            domainGroupsManager = new DomainGroupsManager(agent);
                            Program.GetMenuStack().Push(domainGroupsManager);
                            exit = true;
                            break;
                        case "domaincomputers":
                            domainComputersManager = new DomainComputersManager(agent);
                            Program.GetMenuStack().Push(domainComputersManager);
                            exit = true;
                            break;
                        case "kerberoast":
                            kerberoastManager = new KerberoastManager(agent);
                            Program.GetMenuStack().Push(kerberoastManager);
                            exit = true;
                            break;
                        case "back":
                            Program.GetMenuStack().Pop();
                            exit = true;
                            return;
                        default:
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
