//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using static RedPeanut.Models;
using static RedPeanut.Utility;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace RedPeanut
{
    public class C2Server
    {
        
        Thread servert = null;
        
        RedPeanutC2 httplistener = null;

        string serverkey = "";

        private static Random random = new Random();

        RedPeanutDBContext dbcontext = null;

        Dictionary<string, AgentManager> insteractList = new Dictionary<string, AgentManager>();
        Dictionary<string, IAgentInstance> agentList = new Dictionary<string, IAgentInstance>();
        Dictionary<string, IAgentInstance> agentInboundList = new Dictionary<string, IAgentInstance>();
        Dictionary<string, List<TaskMsg>> commandqueue = new Dictionary<string, List<TaskMsg>>();
        Dictionary<string, TaskMsg> awaitresponsequeue = new Dictionary<string, TaskMsg>();
        //Dictionary<string, WebResourceInstance> webresources = new Dictionary<string, WebResourceInstance>();
        Dictionary<int, HttpProfile> availableprofile = new Dictionary<int, HttpProfile>();
        Dictionary<string, ListenerConfig> listeners = new Dictionary<string, ListenerConfig>();

        int defaultprofileid = 0;
        
        public C2Server(string serverkey)
        {
            this.serverkey = serverkey;
            this.dbcontext = new RedPeanutDBContext(new DbContextOptions<RedPeanutDBContext>());

            RedPeanutDBInitializer.Initialize(dbcontext);
        }

        public RedPeanutDBContext GetDBContext()
        {
            return dbcontext;
        }

        public Random GetRandomObject()
        {
            return random;
        }

        public TaskMsg GetTaskResponse(string taskid)
        {
            return awaitresponsequeue.GetValueOrDefault(taskid);
        }

        public void AddTaskResponse(string taskid, TaskMsg task)
        {
            awaitresponsequeue.Add(taskid, task);
        }

        public void RemoveTaskResponse(string taskid)
        {
            awaitresponsequeue.Remove(taskid);
        }

        public TaskMsg GetCommand(IAgentInstance agent)
        {
            return commandqueue.GetValueOrDefault(agent.AgentId).First();
        }

        public void AddCommand(IAgentInstance agent, TaskMsg task)
        {
            if(agent.Pivoter != null)
            {
                //Agent pivoted so command will be routed via pivoter
                if (!commandqueue.ContainsKey(agent.Pivoter.AgentId))
                    commandqueue.Add(agent.Pivoter.AgentId, new List<TaskMsg>());
                commandqueue.GetValueOrDefault(agent.Pivoter.AgentId).Add(task);
            }
            else
            {
                if (!commandqueue.ContainsKey(agent.AgentId))
                    commandqueue.Add(agent.AgentId, new List<TaskMsg>());
                commandqueue.GetValueOrDefault(agent.AgentId).Add(task);
            }
        }

        public void RemoveCommand(IAgentInstance agent, TaskMsg task)
        {
            TaskMsg msg = commandqueue.GetValueOrDefault(agent.AgentId).ElementAt(commandqueue.GetValueOrDefault(agent.AgentId).IndexOf(task));
            AddTaskResponse(msg.Instanceid,msg);
            commandqueue.GetValueOrDefault(agent.AgentId).Remove(task);
        }

        public WebResource GetWebResource(string uri, RedPeanutDBContext context)
        {
            
            //return webresources.GetValueOrDefault(uri);
            return dbcontext.WebResources.FirstOrDefault<WebResource>(s => s.Uri == uri);
        }

        public void RemoveWebResource(WebResource webrrersource, RedPeanutDBContext context)
        {
            try
            {
                dbcontext.WebResources.Remove(webrrersource);
                dbcontext.SaveChanges();
            }
            catch(Exception)
            {
                Console.WriteLine("[x] Error removing resource");
            }
            
        }

        public void RegisterWebResource(String uri, WebResourceInstance webresource)
        {
            WebResource res = dbcontext.WebResources.FirstOrDefault<WebResource>(s => s.Uri == uri);
            if (res == null)
            {
                if (webresource.Generator != null)
                {
                    res = new WebResource
                    {
                        Uri = uri,
                        Content = webresource.Generator.GetScriptText(),
                        WebResourceType = WebResourceType.Script
                    };
                }
                else
                {
                    res = new WebResource
                    {
                        Uri = uri,
                        FileName = webresource.Uri,
                        WebResourceType = WebResourceType.File
                    };
                }
                
                dbcontext.WebResources.Add(res);
                dbcontext.SaveChanges();
            }
            else
            {
                Console.WriteLine("[x] Resource already exist");
            }
            
        }

        public Dictionary<string,ListenerConfig> GetListenersConfig()
        {
            return listeners;
        }

        public void RemoveListenerConfig(ListenerConfig listenerconfig)
        {
            try
            {
                listeners.Remove(listenerconfig.GetName());
                Listener res = dbcontext.Listeners.FirstOrDefault<Listener>(s => s.name == listenerconfig.GetName());
                dbcontext.Listeners.Remove(res);
                dbcontext.SaveChanges();
            }
            catch (Exception)
            {
                Console.WriteLine("[x] Error removing listener");
            }
        }

        public void UpdateListenerConfig(string listenername, ListenerConfig listenerconfig)
        {
            try
            {
                listeners.Remove(listenerconfig.GetName());
                Listener res = dbcontext.Listeners.FirstOrDefault<Listener>(s => s.name == listenerconfig.GetName());
                res.name = listenerconfig.GetName();
                res.lhost = listenerconfig.GetHost();
                res.lport = listenerconfig.GetPort();
                res.profile = listenerconfig.GetProfileid();
                if (listenerconfig.GetSsl())
                    res.ListenerType = ListenerType.Https;
                else
                    res.ListenerType = ListenerType.Http;

                dbcontext.Listeners.Update(res);
                dbcontext.SaveChanges();
            }
            catch (Exception)
            {
                Console.WriteLine("[x] Error removing listener");
            }
        }

        public void ReloadListenerConfig(string listenername, ListenerConfig listenerconfig)
        {
            try
            {
                if (!listeners.ContainsKey(listenername))                
                {
                    listeners.Add(listenername, listenerconfig);
                }
                else
                {
                    Console.WriteLine("[x] Listener already exist");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Error adding listener {0}", e.Message);
            }
        }

        public void RegisterListenerConfig(string listenername, ListenerConfig listenerconfig)
        {
            try
            {
                if (!listeners.ContainsKey(listenername))
                {
                    listeners.Add(listenername, listenerconfig);
                    Listener listener = dbcontext.Listeners.FirstOrDefault<Listener>(s => s.name == listenername);
                    if (listener == null)
                    {
                        listener = new Listener
                        {
                            name = listenerconfig.GetName(),
                            lhost = listenerconfig.GetHost(),
                            lport = listenerconfig.GetPort(),
                            profile = listenerconfig.GetProfileid(),
                        };

                        if (listenerconfig.GetSsl())
                        {
                            listener.ListenerType = ListenerType.Https;
                        }
                        else
                        {
                            listener.ListenerType = ListenerType.Http;
                        }
                        dbcontext.Listeners.Add(listener);
                        dbcontext.SaveChanges();
                    }
                }
                else
                {
                    Console.WriteLine("[x] Listener already exist");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Error adding listener {0}", e.Message);
            }
        }

        public void RegisterAgentInbound(String agentid, IAgentInstance agent)
        {
            agentInboundList.Add(agentid, agent);
        }

        public void RemoveAgentInbound(String agentid)
        {
            agentInboundList.Remove(agentid);
        }

        public void RegisterAgent(String agentid,IAgentInstance agent)
        {
            agentList.Add(agentid, agent);
        }

        public IAgentInstance GetAgent(string agentid)
        {
            IAgentInstance aginst = null;
            agentList.TryGetValue(agentid, out aginst);
            return aginst;
        }

        public Dictionary<string, IAgentInstance> GetInboundAgents()
        {
            return agentInboundList;
        }

        public Dictionary<string,IAgentInstance> GetAgents()
        {
            return agentList;
        }

        public RedPeanutC2 GetHttpListener()
        {
            return httplistener;
        }
        
        public void StartServerHttpServer(ListenerConfig lconfig)
        {
            httplistener = new RedPeanutC2(this, lconfig);
            servert = new Thread(new ThreadStart(httplistener.Execute));
            servert.Start();
            lconfig.SetStarted(true);
        }

        public bool IsStarted(string name)
        {
            try
            {
                return listeners.GetValueOrDefault(name).GetStarted();
            }
            catch(Exception)
            {
                return false;
            }
        }
        

        public void ListAgents()
        {
            if (GetAgents() != null)
            {
                Console.WriteLine("[*] | {0,-10} | {1,-15} | {2,-10} | {3,-32} | {4,-20} | {5,-40} |", "Agent", "IP", "Integrity", "User", "Process", "System");
                Console.WriteLine("[*]  {0}", new string('-', 144));

                foreach (KeyValuePair<string , IAgentInstance> item in GetAgents())
                {
                    try
                    {                   
                        SystemInfo sysinfo = item.Value.GetSystemInfo();

                        Console.WriteLine("[*] | {0,-10} | {1,-15} | {2,-10} | {3,-32} | {4,-20} | {5,-40} |", item.Key, sysinfo.Ip, sysinfo.Integrity, sysinfo.User, sysinfo.ProcessName, sysinfo.Os);
                    }catch(Exception)
                    {
                        //Console.WriteLine("[x] agent need to be removed {0}", e.Message);
                    }
                }
                Console.WriteLine("[*]  {0}", new string('-', 144));
            }
        }

        public void ListListeners()
        {
            if (GetListenersConfig() != null)
            {
                foreach (KeyValuePair<string, ListenerConfig> item in GetListenersConfig())
                {
                    try
                    {
                        ListenerConfig listenerConfig = item.Value;
                        Console.WriteLine("[*] {0} | Host: {1} | port: {2} | profile: {3}", listenerConfig.GetName(), listenerConfig.GetHost(), listenerConfig.GetPort(), listenerConfig.GetProfileid());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[x] Listener need to be removed {0}", e.Message);
                    }
                }
            }
        }

        public string GetServerKey()
        {
            return serverkey;
        }

        //Validate session
        public bool CheckSessionExists(string agentid)
        {
            IAgentInstance srv = null;
            return agentList.TryGetValue(agentid, out srv);
        }

        public HttpProfile GetProfile(int profileid)
        {
            return availableprofile.GetValueOrDefault(profileid);
        }

        public Dictionary<int,HttpProfile> GetProfiles()
        {
            return availableprofile;
        }

        public int GetDefaultProfile()
        {
            if(defaultprofileid == 0)
            {
                if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PROFILES_FOLDER)))
                {
                    string[] fileEntries = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PROFILES_FOLDER));

                    foreach (string filepath in fileEntries)
                    {
                        string filename = Path.GetFileName(filepath);

                        if (filename.EndsWith(".json") && filename.Split('_').Length == 2)
                        {
                            try
                            {
                                string profileidstr = filename.Split('_')[1].Split('.')[0];

                                if (Int32.TryParse(profileidstr, out int profileid))
                                {
                                    string profilefilecont = File.ReadAllText(filepath);
                                    HttpProfile profile = JsonConvert.DeserializeObject<HttpProfile>(profilefilecont);

                                    if (profile.Default)
                                        defaultprofileid = profileid;

                                    availableprofile.Add(profileid, profile);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (availableprofile.Count == 0)
                    {
                        Console.WriteLine("[*] No profile avalilable, creating new one...");

                        var profile = new HttpProfile
                        {
                            Delay = 5000,
                            Default = true,
                            Name = "Default",
                            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36",
                            ContentUri = "/content/",
                            Spawn = "notepad.exe",
                            HtmlCovered = false
                        };

                        HttpGet httpget = new HttpGet
                        {
                            ApiPath = new string[] { "/newslist.jsp" }
                        };

                        HttpClient getclient = new HttpClient();

                        HttpHeader[] headers = new HttpHeader[3];

                        HttpHeader header = new HttpHeader
                        {
                            Name = "Cache-Control",
                            Value = "no-cache"
                        };

                        headers[0] = header;

                        header = new HttpHeader
                        {
                            Name = "Connection",
                            Value = "Keep-Alive"
                        };

                        headers[1] = header;

                        header = new HttpHeader
                        {
                            Name = "Pragma",
                            Value = "no-cache"
                        };

                        headers[2] = header;

                        getclient.Headers = headers;

                        httpget.Client = getclient;

                        HttpServer getserver = new HttpServer();

                        HttpHeader[] serverheaders = new HttpHeader[3];

                        HttpHeader serverheader = new HttpHeader
                        {
                            Name = "Content-Type",
                            Value = "application/octet-stream"
                        };

                        serverheaders[0] = serverheader;

                        serverheader = new HttpHeader
                        {
                            Name = "Connection",
                            Value = "Keep-Alive"
                        };

                        serverheaders[1] = serverheader;

                        serverheader = new HttpHeader
                        {
                            Name = "Server",
                            Value = "Apache"
                        };

                        serverheaders[2] = serverheader;

                        getserver.Headers = serverheaders;

                        httpget.Server = getserver;

                        profile.HttpGet = httpget;

                        var httppost = new HttpPost
                        {
                            ApiPath = new string[] { "/newslist.jsp" },
                            Client = getclient,
                            Server = getserver,
                            Param = "stocklevel",
                            Mask = @"{0}={1}"
                        };
                        profile.HttpPost = httppost;

                        string deser = JsonConvert.SerializeObject(profile, Formatting.Indented);
                        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PROFILES_FOLDER, "default_1.json"), deser);

                        defaultprofileid = 1;
                        availableprofile.Add(defaultprofileid, profile);
                    }
                }
                else
                    Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PROFILES_FOLDER));

                if (defaultprofileid == 0 && availableprofile.Count > 0)
                {
                    availableprofile.First().Value.Default = true;
                    defaultprofileid = availableprofile.First().Key;
                }

                return defaultprofileid;
            }
            else
            {
                return defaultprofileid;
            }
            


            
        }
    }
}
