//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Threading;
using static RedPeanut.Models;

namespace RedPeanut
{
    public class ListenerConfig
    {
        string Host;
        string Name;
        int Port;
        int Profileid;
        HttpProfile Profile;
        bool started = false;
        bool Ssl = false;


        public CancellationTokenSource CancellationTokenSource  {get; set;}

        public ListenerConfig(string name, string host, int port, HttpProfile profile, int profileid, bool ssl = true)
        {
            Host = host;
            Port = port;
            Name = name;
            Profile = profile;
            Profileid = profileid;
            Ssl = ssl;
        }

        public bool GetSsl()
        {
            return Ssl;
        }

        public int GetProfileid()
        {
            return Profileid;
        }

        public HttpProfile GetProfile()
        {
            return Profile;
        }

        public string GetPostHeaders()
        {
            string urls = "";
            foreach (HttpHeader h in Profile.HttpPost.Server.Headers)
                urls += h.Name + ":" + h.Value + "|";
            return urls;
        }

        public string GetGetHeaders()
        {
            string urls = "";
            foreach (HttpHeader h in Profile.HttpPost.Server.Headers)
                urls += h.Name + ":" + h.Value + "|";
            return urls;
        }

        public string[] GetGetUrls()
        {
            string[] urls = Profile.HttpGet.ApiPath;
            return urls;
        }

        public string[] GetPostUrls()
        {
            string[] urls = Profile.HttpPost.ApiPath;
            return urls;
        }

        public int GetPort()
        {
            return Port;
        }

        public string GetHost()
        {
            return Host;
        }

        public string GetName()
        {
            return Name;
        }

        public bool GetStarted()
        {
            return started;
        }

        public void SetStarted(bool started)
        {
            this.started = started;
        }
    }
}
