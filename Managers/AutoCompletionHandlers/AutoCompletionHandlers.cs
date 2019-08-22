//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPeanut
{
    public class AutoCompletionHandler : IAutoCompleteHandler
    {
        // new char[] { ' ', '.', '/', '\\', ':' };
        public Dictionary<string,string> menu { get; set; }
        public char[] Separators { get; set; } = new char[] { };
        public string[] GetSuggestions(string text, int index)
        {
            if (text.Split(' ').Length <= 2)
                return menu.Keys.ToArray<string>().Where(f => f.StartsWith(text)).ToArray();
            else
                return null;
        }
    }

    class AutoCompletionHandlerC2ServerManager : IAutoCompleteHandler
    {
        public string[] AgentIdList { get; set; }
        public Dictionary<string, string> menu { get; set; }
        public char[] Separators { get; set; } = new char[] { };
        public string[] GetSuggestions(string text, int index)
        {
            if (!text.StartsWith("interact"))
            {
                if (text.Split(' ').Length < 2)
                    return menu.Keys.ToArray<string>().Where(f => f.StartsWith(text)).ToArray();
                else
                    return null;
            }
            else
            {
                string[] agl = new string[AgentIdList.Length];
                for (int i = 0; i < agl.Length; i++)
                    agl[i] = "interact " + AgentIdList[i];

                if (text.Split(' ').Length <= 2)
                    return agl.Where(f =>  f.StartsWith(text)).ToArray();
                else
                    return null;
            }
            
        }
    }
}
