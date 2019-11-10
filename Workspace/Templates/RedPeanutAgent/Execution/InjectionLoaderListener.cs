//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using RedPeanutAgent.Core;

namespace RedPeanutAgent.Execution
{
    class InjectionLoaderListener
    {
        private string pipename = "";
        private bool IsStarted = false;
        private Utility.TaskMsg command = null;

        public InjectionLoaderListener(string pipename, Utility.TaskMsg command)
        {
            this.pipename = pipename;
            this.command = command;
        }

        // Server main thread
        public string Execute(IntPtr hProcess, IntPtr hReadPipe)
        {
            if (hProcess == IntPtr.Zero)
                return "";

            StringBuilder output = ReadOutput(hProcess, hReadPipe);

            return output.ToString();

        }

        private StringBuilder ReadOutput(IntPtr hProcess, IntPtr hReadPipe)
        {
            bool bSuccess;
            uint dwRead = 0;
            StringBuilder output = new StringBuilder();
            byte[] chBuf = new byte[1024];
            uint exitCode;
            bool ok;

            do
            {
                Thread.Sleep(500);
                exitCode = 0;
                ok = Natives.GetExitCodeProcess(hProcess, out exitCode);
                //Console.WriteLine("ok " + ok + " " + exitCode);
                bSuccess = Natives.ReadFile(hReadPipe, chBuf, 1024, out dwRead, IntPtr.Zero);
                //Console.WriteLine("1 bSuccess " + bSuccess + " dwRead " + dwRead);
                if (!bSuccess || dwRead == 0)
                {
                    break;
                }

                output.Append(Encoding.Default.GetString(chBuf).Substring(0, (int)dwRead));

            } while (bSuccess && dwRead != 0 && ((exitCode == 259 || !ok) || dwRead == 1024));

            return new StringBuilder(output.ToString());
        }

    }
}
