//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static RedPeanut.Utility;

namespace RedPeanut
{
    public class HtaPowerShellGenerator: IGenerator
    {
        string base64assembly = "";
        Dictionary<string, string> agr = new Dictionary<string, string>();

        public HtaPowerShellGenerator(string base64Assembly, Dictionary<string, string> agrs)
        {
            this.base64assembly = base64Assembly;

        }

        public string Base64Assembly
        {
            get
            {
                return base64assembly;
            }
            set
            {
                this.base64assembly = value;
            }
        }
        
        public string GetScriptText()
        {
            // generate .hta content
            Random random = new Random();
            string function = RandomAString(10,random).ToLower();
            string shell = RandomAString(10, random).ToLower();

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, HTA_POWERSHELL_TEMPLATE);

            if (File.Exists(templatePath))
            {
                StringBuilder sb = new StringBuilder(File.ReadAllText(templatePath));

                sb.Replace("#{pshstart}", Base64Assembly)
                    .Replace("#{function}", function)
                    .Replace("#{shell}", shell);

                Console.WriteLine("[*] Replace end");

                return sb.ToString() ;
            }
            else
            {
                Console.WriteLine("[x] Template not found");
                return "";
            }                     
        }
    }
}
