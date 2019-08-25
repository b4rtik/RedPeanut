//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

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
        // https://stackoverflow.com/questions/32769630/how-to-compile-a-c-sharp-file-with-roslyn-programmatically

        private static string runtimePath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, DOTNET_40_REF_FOLDER, @"{0}.dll");
        private static string outputPath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, ASSEMBLY_OIUTPUT_FOLDER);
        private static string srcPath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, SRC_FOLDER);
        private static string srcExternalPath = Path.Combine(Directory.GetCurrentDirectory(), EXTERNAL_FOLDER);

        static Dictionary<CompilationProfile, string[]> compilationProfiles = new Dictionary<CompilationProfile, string[]>
        {
            {CompilationProfile.Agent ,new string[]{"Natives.cs", "Utility.cs", "Crypto.cs", "AgentInstanceNamedPipe.cs", "SmbListener.cs", "CommandExecuter.cs", "InjectionHelper.cs","InjectionLoaderListener.cs", "Spawner.cs", "ImageLoader.cs"}},
            {CompilationProfile.Generic, new string[]{""}},
            {CompilationProfile.UACBypass,new string[]{"Natives.cs", "Crypto.cs", "Utility.cs", "ImageLoader.cs", "Spawner.cs" ,"InjectionHelper.cs", "InjectionLoaderListener.cs", "UACBypassHelper.cs","WnfHelper.cs", "Enums.cs", "Imports.cs", "Loader.cs", "Structs.cs","Tokens.cs", "Win32.cs", "Generic.cs"}},
            {CompilationProfile.StandardCommand,new string[]{"Tokens.cs", "Win32.cs", "Generic.cs", "Utility.cs", "Crypto.cs", "ImageLoader.cs"}},
            {CompilationProfile.Persistence,new string[]{ "Autorun.cs", "Startup.cs", "WMI.cs", "Registry.cs"}},
            {CompilationProfile.PersistenceCLR,new string[]{}}
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
                MetadataReference.CreateFromFile(string.Format(runtimePathRef, "System.Drawing"))
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

    }
}
