//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

namespace RedPeanut
{
    public class WebResourceInstance
    {
        private string uri = "";
        private IGenerator generator = null;
        
        public WebResourceInstance(IGenerator generator, string uri)
        {
            this.uri = uri;
            this.generator = generator;
        }

        public string Uri
        {
            get
            {
               return uri;
            }
        }

        public IGenerator Generator
        {
            get
            {
                return generator;
            }
        }
    }
}
