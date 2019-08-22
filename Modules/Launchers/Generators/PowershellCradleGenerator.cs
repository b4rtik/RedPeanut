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
    public class PowershellCradleGenerator : IGenerator
    {
        string base64assembly = "";
        Dictionary<string, string> arg = new Dictionary<string, string>();

        public PowershellCradleGenerator(string base64Assembly, Dictionary<string, string> agrs)
        {
            this.base64assembly = base64Assembly;
            this.arg = agrs;
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

            string templatePath_s0 = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, POWERHSELL_S0_TEMPLATE);
            
            if (File.Exists(templatePath_s0))
            {
                StringBuilder sb_s0 = new StringBuilder(File.ReadAllText(templatePath_s0));

                foreach(KeyValuePair<string,string> kp in arg.AsEnumerable())
                    sb_s0.Replace(kp.Key , kp.Value);

                Console.WriteLine("[*] Replace end");

                return Convert.ToBase64String(Encoding.Unicode.GetBytes(sb_s0.ToString()));
            }
            else
            {
                Console.WriteLine("[x] Template not found");
                return "";
            }            
        }
    }
}
