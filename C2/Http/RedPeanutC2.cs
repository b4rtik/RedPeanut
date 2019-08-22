//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class RedPeanutC2
    {
        ListenerConfig Lconfig;

        static string pfx = "certs/redpeanut.pfx";
        static string cert = "certs/redpeanut.cer";

        bool ssl = false;

        public static C2Server server;

        public RedPeanutC2(C2Server server, ListenerConfig lconfig)
        {
            this.Lconfig = lconfig;
            RedPeanutC2.server = server;
            this.ssl = lconfig.GetSsl();
        }
        
        public void Execute()
        {
            //Check ssl setup
            X509Certificate2 x509cert = null;
            if ((!File.Exists(pfx) || !File.Exists(cert)) && ssl)
            {
                Console.WriteLine("Building cert...");
                if (!Directory.Exists("certs"))
                    Directory.CreateDirectory("certs");

                BuildSelfSignedServerCertificate("RedPeanut", Lconfig.GetHost(), pfx, cert);
                
            }
            try
            {                
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                if(ssl)
                {
                    x509cert = new X509Certificate2(pfx);

                    CreateWebHostBuilder(new string[] { })
                    .UseKestrel(options =>
                    {
                        options.Listen(new IPEndPoint(IPAddress.Parse(Lconfig.GetHost()), Lconfig.GetPort()), listenOptions =>
                         {
                             listenOptions.UseHttps(httpsOptions =>
                             {
                                 httpsOptions.ServerCertificate = x509cert;
                                 httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                                 httpsOptions.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                                 Console.WriteLine("\n[*] Using cert with hash: {0}", httpsOptions.ServerCertificate.GetCertHashString());
                             });
                         });

                        options.AddServerHeader = false;
                    })
                    .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PAYLOADS_FOLDER))
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddFilter("System", LogLevel.Error )
                               .AddFilter("Microsoft", LogLevel.Error);
                    })
                    .UseUrls("https://" + Lconfig.GetHost() + ":" + Lconfig.GetPort())
                    .UseSetting("FrameworkHost", Lconfig.GetHost())
                    .UseSetting("FrameworkPort", Lconfig.GetPort().ToString())
                    .UseSetting("FrameworkSSL", ssl.ToString())
                    .UseSetting("FrameworkProfileid", Lconfig.GetProfileid().ToString())
                    .Build()
                    .RunAsync(cancellationTokenSource.Token);
                }
                else
                {
                    CreateWebHostBuilder(new string[] { })
                    .UseKestrel(options =>
                    {
                        options.Listen(new IPEndPoint(IPAddress.Parse(Lconfig.GetHost()), Lconfig.GetPort()));
                        options.AddServerHeader = false;
                    })
                    .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PAYLOADS_FOLDER))
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddFilter("System", LogLevel.Error)
                               .AddFilter("Microsoft", LogLevel.Error);
                    })
                    .UseUrls("http://" + Lconfig.GetHost() + ":" + Lconfig.GetPort())
                    .UseSetting("FrameworkHost", Lconfig.GetHost())
                    .UseSetting("FrameworkPort", Lconfig.GetPort().ToString())
                    .UseSetting("FrameworkSSL", ssl.ToString())
                    .UseSetting("FrameworkProfileid", Lconfig.GetProfileid().ToString())
                    .Build()
                    .RunAsync(cancellationTokenSource.Token);
                }
                Lconfig.CancellationTokenSource = cancellationTokenSource;
            }
            catch (CryptographicException)
            {
                Console.Error.WriteLine("Error importing certificate.");
            }           
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
