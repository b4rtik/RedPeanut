//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

namespace RedPeanut
{
    public class RedPeanutDBInitializer 
    {
        public static void Initialize(RedPeanutDBContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
