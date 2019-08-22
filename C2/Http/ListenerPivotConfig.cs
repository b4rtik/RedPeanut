//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using static RedPeanut.Models;

namespace RedPeanut
{
    public class ListenerPivotConfig
    {
        string Host;
        string Name;
        string Pipename;
        HttpProfile Profile;

        public ListenerPivotConfig(string name, string host, string pipename, HttpProfile profile)
        {
            Host = host;
            Pipename = pipename;
            Name = name;
            Profile = profile;
        }

        public HttpProfile GetProfile()
        {
            return Profile;
        }

        public string GetPipename()
        {
            return Pipename;
        }
        
        public string GetHost()
        {
            return Host;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
