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

            int exitCode = -1;
            try
            {
                commandProcess.Start();
                commandProcess.BeginOutputReadLine();
                commandProcess.BeginErrorReadLine();
                commandProcess.WaitForExit();
            }
            catch (Win32Exception)
            {
                throw new MigrateException($"Command {cmd} does not exit. Did you install it or add it to the Environment path?");
            }
            finally
            {
                exitCode = commandProcess.ExitCode;
                commandProcess.Close();
            }

            standardOutput = tempOutput;
            standardError = tempError;

            return exitCode;
        }

        public int RunGitSvnInitCommand(string arguments)
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
                    FileName = "git",
                    Arguments = arguments
                }
            };

            int exitCode = -1;
            try
            {
                commandProcess.Start();

                int lastChr = 0;

                string output = "";
                bool displayedPasswordFor = false;
                do
                {
                    if (displayedPasswordFor && commandProcess.StandardError.Peek() == -1)
                    {
                        break;
                    }

                    lastChr = commandProcess.StandardError.Read();

                    string outputChr = null;
                    outputChr += commandProcess.StandardError.CurrentEncoding.GetString(new byte[] { (byte)lastChr });
                    output += outputChr;

                    if (!displayedPasswordFor && output.Contains("Password for"))
                    {
                        displayedPasswordFor = true;
                    }

                    Console.Write(outputChr);
                } while (lastChr > 0);

                if (output.Contains("Password for"))
                {
                    string inputPassword = Console.ReadLine();
                    commandProcess.StandardInput.WriteLine(inputPassword);
                    commandProcess.StandardInput.Flush();
                }

                commandProcess.WaitForExit();
            }
            catch (Win32Exception)
            {
                throw new MigrateException($"Command git does not exit. Did you install it or add it to the Environment path?");
            }
            finally
            {
                exitCode = commandProcess.ExitCode;
                commandProcess.Close();
            }

            return exitCode;
        }
    }
}