//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

namespace RedPeanut
{
    public class Models
    {
        public class ServerKey
        {
            public string serverkey { get; set; }
        }

        public class HttpProfile
        {
            public string Name { get; set; }
            public bool Default { get; set; }
            public bool HtmlCovered { get; set; }
            public string TargetClass { get; set; }
            public int Delay { get; set; }
            public string ContentUri { get; set; }
            public HttpGet HttpGet { get; set; }
            public HttpPost HttpPost { get; set; }
            public string UserAgent { get; set; }
            public string Spawn { get; set; }
            public bool InjectionManaged { get; set; }
        }

        public class HttpClient
        {
            public HttpHeader[] Headers { get; set; }
        }

        public class HttpServer
        {
            public HttpHeader[] Headers { get; set; }
            public string Prepend { get; set; }
            public string Append { get; set; }
        }

        public class HttpPost
        {
            public string[] ApiPath { get; set; }
            public string Param { get; set; }
            public string Mask { get; set; }
            public HttpServer Server { get; set; }
            public HttpClient Client { get; set; }
        }

        public class HttpGet
        {
            public string[] ApiPath { get; set; }
            public HttpServer Server { get; set; }
            public HttpClient Client { get; set; }
        }

        public class HttpHeader
        {
            public string Name { get; set; }
            public string Value { get; set; }
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

        public class AgentIdReqMsg
        {
            public string AgentPivot { get; set; }
            public string address { get; set; }
            public int port { get; set; }
            public int framework { get; set; }
            public string request { get; set; }
        }

        public class AgentIdMsg
        {
            public string agentid { get; set; }
            public byte[] sessionkey { get; set; }
            public byte[] sessioniv { get; set; }
            public string stage { get; set; }
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
        
        public class InjectionManaged
        {
            public bool Managed { get; set; }
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
            public InjectionManaged InjectionManagedTask { get; set; }
        }

        public class ResponseMsg
        {
            public string Agentid { get; set; }
            public string AgentPivot { get; set; }
            public string TaskInstanceid { get; set; }
            public SystemInfo SystemInfo { get; set; }
            public bool Chunked { get; set; }
            public int Number { get; set; }
            public string Data { get; set; }
        }

        public enum WebResourceType
        {
            File, Script
        }

        public enum ListenerType
        {
            Http,Https
        }

        public class WebResource
        {
            public int WebResourceID { get; set; }
            public string Uri { get; set; }
            public string Content { get; set; }
            public string FileName { get; set; }
            public WebResourceType? WebResourceType { get; set; }
        }

        public class Listener
        {
            public int ListenerID { get; set; }
            public string name { get; set; }
            public string lhost { get; set; }
            public int lport { get; set; }
            public int profile { get; set; }
            public ListenerType? ListenerType { get; set; }
        }
    }
}
