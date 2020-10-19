using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RenderTrainerLib
{
    class Tools
    {
        public static Process StartProcess(string program, string arguments, string id)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = arguments;
                //process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                File.WriteAllText(Path.Combine(Path.GetTempPath(), id + ".pid"), process.Id.ToString());

                return (process);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        public static void KillPythonProcess(string id)
        {
            try
            {
                string pidPath= Path.Combine(Path.GetTempPath(), id + ".pid");
                if (File.Exists(pidPath))
                {
                    int pid = int.Parse(File.ReadAllText(pidPath));

                    Process[] process = Process.GetProcesses();

                    foreach (Process prs in process)
                    {
                        if (prs.Id == pid)
                        {
                            prs.Kill();
                            prs.WaitForExit();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
