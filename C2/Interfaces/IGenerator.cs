//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

namespace RedPeanut
{
    public interface IGenerator
    {
        string Base64Assembly { get; set; }

        string GetScriptText();

    }
}
