//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System.Security.Cryptography;
using System.Text;
using static RedPeanut.Models;

namespace RedPeanut
{
    public interface IAgentInstance
    {
        string AgentId { get; }
        AesManaged AesManager { get; }
        Models.SystemInfo SysInfo { get; set; }
        int TargetFramework { get; }
        IAgentInstance Pivoter { get; set; }
        // Manage agentid request
        void Run();


        //Send agentid
        //RC4 with serverkey
        void SendAgentId(string agentid);


        //Send command to agent
        //AES
        void SendCommand(TaskMsg command_src);



        //Read oputput from agent
        //AES
        StringBuilder ReadOutput();

        Models.CheckInMsg GetCheckInMsg();

        Models.SystemInfo GetSystemInfo();
    }
}
