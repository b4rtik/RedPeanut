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
    public class MSBuildGenerator : IGenerator
    {
        string base64assembly = "";
        Dictionary<string, string> arg = new Dictionary<string, string>();

        public MSBuildGenerator(string base64Assembly, Dictionary<string, string> args)
        {
            this.base64assembly = base64Assembly;
            this.arg = args;

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
            string buildxml = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, MSBUILD_TEMPLATE);

            if (File.Exists(buildxml))
            {
                StringBuilder sb_xml = new StringBuilder(File.ReadAllText(buildxml));

                sb_xml.Replace("#{assembly}", base64assembly);

                foreach (KeyValuePair<string, string> kp in arg.AsEnumerable())
                    sb_xml.Replace(kp.Key, kp.Value);

                Console.WriteLine("[*] Replace end");

                return sb_xml.ToString();
            }
            else
            {
                Console.WriteLine("[x] Template not found");
                return "";
            }
        }
    }
}
