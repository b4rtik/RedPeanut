using System;
using donutCS.Payloads;

class Constants
{
    public const int CIPHER_BLK_LEN =               128 / 8;
    public const int DONUT_KEY_LEN =                128 / 8;
    public const int DONUT_BLK_LEN =                128 / 8;
    public const int DONUT_ERROR_SUCCESS =          0;
    public const int DONUT_ERROR_FILE_NOT_FOUND =   1;
    public const int DONUT_ERROR_FILE_EMPTY =       2;
    public const int DONUT_ERROR_FILE_ACCESS =      3;
    public const int DONUT_ERROR_FILE_INVALID =     4;
    public const int DONUT_ERROR_NET_PARAMS =       5;
    public const int DONUT_ERROR_NO_MEMORY =        6;
    public const int DONUT_ERROR_INVALID_ARCH =     7;
    public const int DONUT_ERROR_INVALID_URL =      8;
    public const int DONUT_ERROR_URL_LENGTH =       9;
    public const int DONUT_ERROR_INVALID_PARAMETER= 10;
    public const int DONUT_ERROR_RANDOM =           11;
    public const int DONUT_ERROR_DLL_FUNCTION =     12;
    public const int DONUT_ERROR_ARCH_MISMATCH =    13;
    public const int DONUT_ERROR_DLL_PARAM =        14;
    public const int DONUT_ERROR_BYPASS_INVALID =   15;
    public const int DONUT_ERROR_NORELOC =          16;

    // target architecture;
    public const int DONUT_ARCH_ANY = -1;   // just for vbs,js and xsl files;
    public const int DONUT_ARCH_X86 = 1;    // x86;
    public const int DONUT_ARCH_X64 = 2;    // AMD64;
    public const int DONUT_ARCH_X84 = 3;    // AMD64 + x86;

    // module type;
    public const int DONUT_MODULE_NET_DLL = 1;  // .NET DLL. Requires class and method;
    public const int DONUT_MODULE_NET_EXE = 2;  // .NET EXE. Executes Main if no class and method provided;
    public const int DONUT_MODULE_DLL =     3;  // Unmanaged DLL, function is optional;
    public const int DONUT_MODULE_EXE =     4;  // Unmanaged EXE;
    public const int DONUT_MODULE_VBS =     5;  // VBScript;
    public const int DONUT_MODULE_JS =      6;  // JavaScript or JScript;
    public const int DONUT_MODULE_XSL =     7;  // XSL with JavaScript/JScript or VBscript embedded;

    // instance type;
    public const int DONUT_INSTANCE_PIC = 1;  // Self-contained;
    public const int DONUT_INSTANCE_URL = 2;  // Download from remote server;

    // AMSI/WLDP options;
    public const int DONUT_BYPASS_SKIP =        1;  // Disables bypassing AMSI/WDLP;
    public const int DONUT_BYPASS_ABORT =       2;  // If bypassing AMSI/WLDP fails, the loader stops running;
    public const int DONUT_BYPASS_CONTINUE =    3;  // If bypassing AMSI/WLDP fails, the loader continues running;

    public const int DONUT_MAX_PARAM =      8;      // maximum number of parameters passed to method;
    public const int DONUT_MAX_NAME =       256;    // maximum length of string for domain, class, method and parameter names;
    public const int DONUT_MAX_DLL =        8;      // maximum number of DLL supported by instance;
    public const int DONUT_MAX_URL =        256;
    public const int DONUT_MAX_MODNAME =    8;
    public const int DONUT_SIG_LEN =        8;      // 64-bit string to verify decryption ok;
    public const int DONUT_VER_LEN =        32;
    public const int DONUT_DOMAIN_LEN =     8;

    public const string DONUT_RUNTIME_NET2 = "v2.0.50727";
    public const string DONUT_RUNTIME_NET4 = "v4.0.30319";

    public const string NTDLL_DLL =     "ntdll.dll";
    public const string KERNEL32_DLL =  "kernel32.dll";
    public const string ADVAPI32_DLL =  "advapi32.dll";
    public const string CRYPT32_DLL =   "crypt32.dll";
    public const string MSCOREE_DLL =   "mscoree.dll";
    public const string OLE32_DLL =     "ole32.dll";
    public const string OLEAUT32_DLL =  "oleaut32.dll";
    public const string WININET_DLL =   "wininet.dll";
    public const string COMBASE_DLL =   "combase.dll";
    public const string USER32_DLL =    "user32.dll";
    public const string SHLWAPI_DLL =   "shlwapi.dll";

    // required to load .NET assemblies
    public static Guid xCLSID_CorRuntimeHost =  new Guid(0xcb2f6723, 0xab3a, 0x11d2, 0x9c, 0x40, 0x00, 0xc0, 0x4f, 0xa3, 0x0a, 0x3e);
    public static Guid xIID_ICorRuntimeHost =   new Guid(0xcb2f6722, 0xab3a, 0x11d2, 0x9c, 0x40, 0x00, 0xc0, 0x4f, 0xa3, 0x0a, 0x3e);
    public static Guid xCLSID_CLRMetaHost =     new Guid(0x9280188d, 0xe8e, 0x4867, 0xb3, 0xc, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);
    public static Guid xIID_ICLRMetaHost =      new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);
    public static Guid xIID_ICLRRuntimeInfo =   new Guid(0xBD39D1D2, 0xBA2F, 0x486a, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91);
    public static Guid xIID_AppDomain =         new Guid(0x05F696DC, 0x2B29, 0x3663, 0xAD, 0x8B, 0xC4, 0x38, 0x9C, 0xF2, 0xA7, 0x13);

    // required to load VBS and JS files
    public static Guid xIID_IUnknown =                  new Guid(0x00000000, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
    public static Guid xIID_IDispatch =                 new Guid(0x00020400, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
    public static Guid xIID_IHost =                     new Guid(0x91afbd1b, 0x5feb, 0x43f5, 0xb0, 0x28, 0xe2, 0xca, 0x96, 0x06, 0x17, 0xec);
    public static Guid xIID_IActiveScript =             new Guid(0xbb1a2ae1, 0xa4f9, 0x11cf, 0x8f, 0x20, 0x00, 0x80, 0x5f, 0x2c, 0xd0, 0x64);
    public static Guid xIID_IActiveScriptSite =         new Guid(0xdb01a1e3, 0xa42b, 0x11cf, 0x8f, 0x20, 0x00, 0x80, 0x5f, 0x2c, 0xd0, 0x64);
    public static Guid xIID_IActiveScriptSiteWindow =   new Guid(0xd10f6761, 0x83e9, 0x11cf, 0x8f, 0x20, 0x00, 0x80, 0x5f, 0x2c, 0xd0, 0x64);
    public static Guid xIID_IActiveScriptParse32 =      new Guid(0xbb1a2ae2, 0xa4f9, 0x11cf, 0x8f, 0x20, 0x00, 0x80, 0x5f, 0x2c, 0xd0, 0x64);
    public static Guid xIID_IActiveScriptParse64 =      new Guid(0xc7ef7658, 0xe1ee, 0x480e, 0x97, 0xea, 0xd5, 0x2c, 0xb4, 0xd7, 0x6d, 0x17);
    public static Guid xCLSID_VBScript =                new Guid(0xB54F3741, 0x5B07, 0x11cf, 0xA4, 0xB0, 0x00, 0xAA, 0x00, 0x4A, 0x55, 0xE8);
    public static Guid xCLSID_JScript =                 new Guid(0xF414C260, 0x6AC0, 0x11CF, 0xB6, 0xD1, 0x00, 0xAA, 0x00, 0xBB, 0xBB, 0x58);

    // required to load XSL files
    public static Guid xCLSID_DOMDocument30 =   new Guid(0xf5078f32, 0xc551, 0x11d3, 0x89, 0xb9, 0x00, 0x00, 0xf8, 0x1f, 0xe2, 0x21);
    public static Guid xIID_IXMLDOMDocument =   new Guid(0x2933BF81, 0x7B36, 0x11D2, 0xB2, 0x0E, 0x00, 0xC0, 0x4F, 0x98, 0x3E, 0x60);
    public static Guid xIID_IXMLDOMNode =       new Guid(0x2933bf80, 0x7b36, 0x11d2, 0xb2, 0x0e, 0x00, 0xc0, 0x4f, 0x98, 0x3e, 0x60);

    // Maru stuff
    public static int MARU_MAX_STR =    64;
    public static int MARU_BLK_LEN =    16;
    public static int MARU_HASH_LEN =   8;
    public static int MARU_IV_LEN =     MARU_HASH_LEN;

    // Payload shorthand
    public static byte[] PAYLOAD_EXE_x86 = payload_exe_x86.PAYLOAD_EXE_X86;
    public static byte[] PAYLOAD_EXE_x64 = payload_exe_x64.PAYLOAD_EXE_X64;
}