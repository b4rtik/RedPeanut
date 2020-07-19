using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using SharpSploit.Enumeration;

namespace SharpSploitDomainReconImpl
{
    public class Program
    {

        public static void GetDomainUsers(string[] Identity)
        {
            try
            {
                if (string.IsNullOrEmpty(Identity[0]))
                {
                    List<Domain.DomainObject> list = new Domain.DomainSearcher().GetDomainUsers();

                    foreach (Domain.DomainObject itemobject in list)
                        itemobject.ToString();
                }
                else
                {
                    new Domain.DomainSearcher().GetDomainUsers(new List<string>(new string[] { Identity[0] })).ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetDomainUsers error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetNetLoggedOnUsers(string[] computername)
        {
            try
            {
                if(string.IsNullOrEmpty(computername[0]))
                {
                    Console.WriteLine("Must provide computer name or ip address of the target");
                    return;
                }

                List<Net.LoggedOnUser> list = Net.GetNetLoggedOnUsers(new List<string>(new string[] { computername[0] }));

                foreach (Net.LoggedOnUser itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetNetLoggedOnUsers error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetNetSessions(string[] computername)
        {
            try
            {
                if (string.IsNullOrEmpty(computername[0]))
                {
                    Console.WriteLine("Must provide computer name or ip address of the target");
                    return;
                }

                List<Net.SessionInfo> list = Net.GetNetSessions(new List<string>(new string[] { computername[0] }));

                foreach (Net.SessionInfo itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetNetSessions error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetNetLocalGroupMembers(string[]args)
        {
            try
            {
                if (string.IsNullOrEmpty(args[0]))
                {
                    Console.WriteLine("Must provide computer name or ip address of the target");
                    return;
                }

                string groupname = "Administrators";
                if (args.Length > 1)
                    groupname = args[1];

                List<Net.LocalGroupMember> list = Net.GetNetLocalGroupMembers(new List<string>(new string[] { args[0] }), groupname);

                foreach (Net.LocalGroupMember itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetNetLocalGroupMembers error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetNetLocalGroups(string[] computername)
        {
            try
            {
                if (string.IsNullOrEmpty(computername[0]))
                {
                    Console.WriteLine("Must provide computer name or ip address of the target");
                    return;
                }

                List<Net.LocalGroup> list = Net.GetNetLocalGroups(new List<string>(new string[] { computername[0] }));

                foreach (Net.LocalGroup itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetNetLocalGroups error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetDomainGroups(string[] computername)
        {
            try
            {
                List<Domain.DomainObject> list = new Domain.DomainSearcher().GetDomainGroups();

                foreach (Domain.DomainObject itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetDomainGroups error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetDomainComputers(string[] computername)
        {
            try
            {
                List<Domain.DomainObject> list = new Domain.DomainSearcher().GetDomainComputers();

                foreach (Domain.DomainObject itemobject in list)
                    itemobject.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetDomainComputers error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static void GetKerberoast(string[] computername)
        {
            try
            {
                List<Domain.SPNTicket> list = new Domain.DomainSearcher().Kerberoast();

                foreach (Domain.SPNTicket itemobject in list)
                    itemobject.GetFormattedHash(Domain.SPNTicket.HashFormat.Hashcat);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetDomainComputers error : " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }


    }
}
