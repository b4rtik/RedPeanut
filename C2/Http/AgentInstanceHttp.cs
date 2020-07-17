//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Security.Cryptography;
using System.Text;
using static RedPeanut.Models;

namespace RedPeanut
{
    public class AgentInstanceHttp : IAgentInstance
    {
        private string agentid = "";
        private string serverkey = "";
        private int targetframwork = 40;
        private string host = null;
        private int port = 0;
        private Models.SystemInfo sysinfo = null;
        AesManaged aes = null;
        private C2Server server = null;
        private IAgentInstance pivoter = null;
        private int profileid = 0;
        private bool managed = false;
        public string Cookie {get;set;}

        public AgentInstanceHttp(C2Server server,string agentid, string serverkey, string address, int port, int targetframework, int profileid, byte[] sessionkey = null, byte[] sessioniv = null)
        {
            this.agentid = agentid;
            this.serverkey = serverkey;
            this.server = server;
            this.host = address;
            this.port = port;
            this.targetframwork = targetframework;
            aes = new AesManaged();
            if(sessionkey != null && sessioniv != null)
            {
                aes.Key = sessionkey;
                aes.IV = sessioniv;
            }
            this.profileid = profileid;
            HttpProfile profile = Program.GetC2Manager().GetC2Server().GetProfile(profileid);
            Managed = profile.InjectionManaged;
        }

        public AgentInstanceHttp(C2Server server, string agentid, string serverkey, int targetframework, IAgentInstance agent, int profileid)
        {
            this.agentid = agentid;
            this.serverkey = serverkey;
            this.server = server;
            this.targetframwork = targetframework;
            aes = new AesManaged();
            pivoter = agent;
            this.profileid = profileid;
            HttpProfile profile = Program.GetC2Manager().GetC2Server().GetProfile(profileid);
            Managed = profile.InjectionManaged;
        }

        public int GetProfileid()
        {
            return profileid;
        }

        public string GetAddress()
        {
            return host;
        }

        public int GetPort()
        {
            return port;
        }

        public int TargetFramework
        {
            get
            {
                return targetframwork;
            }
            
        }

        public IAgentInstance Pivoter
        {
            get
            {
                return pivoter;
            }
            set
            {
                this.pivoter = value;
            }
        }

        public Models.SystemInfo SysInfo
        {
            get
            {
                return sysinfo;
            }
            set
            {
                this.sysinfo = value;
            }
        }

        public string AgentId
        {
            get
            {
               return agentid;
            }
        }

        public AesManaged AesManager
        {
            get
            {
                return aes;
            }
        }

        public bool Managed
        {
            get
            {
                return this.managed;
            }
            set
            {
                this.managed = value;
            }
        }

        // Manage agentid request
        public void Run()
        {
            
        }

        //Send agentid
        //RC4 with serverkey
        public void SendAgentId(string agentid)
        {
            
        }

        //Send command to agent
        //AES
        public void SendCommand(TaskMsg task)
        {
            server.AddCommand(this, task);
        }

        //Read oputput from agent
        //AES
        public StringBuilder ReadOutput()
        {
            StringBuilder output = new StringBuilder();
            

            return output;
        }

        public Models.CheckInMsg GetCheckInMsg()
        {
            
            Models.CheckInMsg msg = new Models.CheckInMsg();
            
            return msg;
        }

        public Models.SystemInfo GetSystemInfo()
        {
            return sysinfo;
        }
    }
}
