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
                    FileName = cmd,
                    Arguments = arguments
                }
            };

            try
            {
                commandProcess.Start();
            }
            catch (Win32Exception)
            {
                throw new MigrateException($"Command {cmd} does not exit. Did you install it or add it to the Environment path?");
            }

            standardOutput = commandProcess.StandardOutput.ReadToEnd();
            standardError = commandProcess.StandardError.ReadToEnd();

            return commandProcess.ExitCode;
        }
    }
}