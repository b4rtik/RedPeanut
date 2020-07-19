//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using static RedPeanut.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.Tasks;
using static RedPeanut.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RedPeanut
{
    public class Startup
    {
        private readonly HttpProfile profile = null;
        bool ssl;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            profile = RedPeanutC2.server.GetProfile(Int32.Parse(Configuration["FrameworkProfileid"]));
            ssl = bool.Parse(Configuration["FrameworkSSL"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddEntityFrameworkSqlite().AddDbContext<RedPeanutDBContext>(ServiceLifetime.Transient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.Response.OnStarting(() =>
                {
                    foreach(HttpHeader h in profile.HttpPost.Server.Headers)
                    {
                        //if(!context.Response.Headers.TryAdd(h.Name, h.Value))
                        //    Console.WriteLine("[!] Server Header {0} can't be added", h.Name);
                        context.Response.Headers.TryAdd(h.Name, h.Value);
                    }
                    
                    return Task.FromResult(0);
                });
                await nextMiddleware();
            });

            app.UseMvc(routes =>
            {
                if(ssl)
                {
                    foreach(string s in profile.HttpGet.ApiPath)
                        routes.MapRoute(s, s.TrimStart('/'), new { controller = "HttpListener", action = "Get" });

                    foreach(string s in profile.HttpPost.ApiPath)
                        routes.MapRoute(s + "Post", s.TrimStart('/'), new { controller = "HttpListener", action = "Post" });
                }

                string uricontent = profile.ContentUri.TrimStart('/');
                if(!uricontent.EndsWith("/"))
                    uricontent += "/";

                //Console.WriteLine("[*] uricontent " + uricontent);
                routes.MapRoute(uricontent, uricontent + "{filename}", new { controller = "HttpDynamic", action = "Get" });
                routes.MapRoute("imaqges", "images/" + "{filename}", new { controller = "HttpImage", action = "Get" });
                routes.MapRoute("office", "office/" + "{filename}", new { controller = "HttpEvilClippy", action = "Get" });
                //routes.MapRoute(uricontent + "Post", uricontent + "{filename}", new { controller = "HttpDynamic", action = "Post" });

            });

            var options = new StaticFileOptions
            {
                ContentTypeProvider = new FileExtensionContentTypeProvider()
            };

            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".dll", "application/x-msdownload");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".exe", "application/x-msdownload");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".hta", "application/hta");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".html", "text/html");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".jsp", "text/html");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".php", "text/html");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".xls", "application/vnd.ms-excel");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".vbs", "application/vbs");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".doc", "application/msword");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".docm", "application/vnd.ms-word.document.macroEnabled.12");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".docm", "application/application/vnd.ms-excel.sheet.macroEnabled.12");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".dot", "application/msword");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".dotm", "application/vnd.ms-word.template.macroEnabled.12");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".xml", "text/xml");
            ((FileExtensionContentTypeProvider)options.ContentTypeProvider).Mappings.TryAdd(".xsl", "text/xml");

            options.FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, PAYLOADS_FOLDER));
            options.RequestPath = "/file";
            options.ServeUnknownFileTypes = true;
            options.DefaultContentType = "text/plain";

            app.UseStaticFiles(options);


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

        }
        
        public Dictionary<string,string> ParseServerHeader(string param)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach (string s in param.Split('|'))
            {
                if(!string.IsNullOrEmpty(s))
                    res.Add(s.Split(':')[0], s.Split(':')[1]);
            }           
            return res;
        }
    }
}
