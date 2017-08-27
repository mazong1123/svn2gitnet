using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Svn2GitNet
{
    public class CommandRunner : ICommandRunner
    {
        public int Run(string cmd, string arguments)
        {
            string standardOutput;
            string standardError;

            return Run(cmd, arguments, out standardOutput, out standardError);
        }

        public int Run(string cmd, string arguments, out string standardOutput)
        {
            string standardError;

            return Run(cmd, arguments, out standardOutput, out standardError);
        }

        public int Run(string cmd, string arguments, out string standardOutput, out string standardError)
        {
            Process commandProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    FileName = cmd,
                    Arguments = arguments
                }
            };

            string tempOutput = string.Empty;
            commandProcess.OutputDataReceived += (s, e) =>
            {
                Console.WriteLine(e.Data);
                tempOutput += e.Data;
            };

            string tempError = string.Empty;
            commandProcess.ErrorDataReceived += (s, e) =>
            {
                Console.WriteLine(e.Data);
                tempError += e.Data;
            };

            try
            {
                commandProcess.Start();
                commandProcess.BeginOutputReadLine();
                commandProcess.BeginErrorReadLine();
            }
            catch (Win32Exception)
            {
                throw new MigrateException($"Command {cmd} does not exit. Did you install it or add it to the Environment path?");
            }

            commandProcess.WaitForExit();

            standardOutput = tempOutput;
            standardError = tempError;

            return commandProcess.ExitCode;
        }

        private void CommandProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void CommandProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}