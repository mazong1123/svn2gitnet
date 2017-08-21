using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Svn2GitNet
{
    class Migrator
    {
        // TODO: Add windows support.
        private const string DEFAULT_AUTHOR_FILE = "~/.svn2git/authors";
        private readonly string _dir;
        private Options _options;
        private string[] _args;

        private string _gitConfigCommand;

        public Migrator(Options options, string[] args)
        {
            _options = options;
            _args = args;
            if (string.IsNullOrWhiteSpace(_options.Authors))
            {
                _options.Authors = GetDefaultAuthorsOption();
            }
        }

        public OptionsValidateResult ValidateOptions()
        {
            if (_options.Rebase || _options.RebaseBranch)
            {
                if (_args.Length > 1)
                {
                    return OptionsValidateResult.TooManyArguments;
                }

                return VerifyWorkingTreeIsClean();
            }
            else if (_args.Length == 0)
            {
                return OptionsValidateResult.MissingSvnUrlParameter;
            }
            else if (_args.Length > 1)
            {
                return OptionsValidateResult.TooManyArguments;
            }

            return OptionsValidateResult.OK;
        }

        public void Run()
        {

        }


        private string GitConfigCommand
        {
            get
            {
                if (_gitConfigCommand == null)
                {
                    string standardOutput;
                    string standardError;
                    RunCommand("git", "config --local --get user.name", out standardOutput, out standardError);
                    string combinedOutput = standardOutput + standardError;
                    _gitConfigCommand = Regex.IsMatch(combinedOutput, @"/unknown option/m") ? "git config" : "git config --local";
                }

                return _gitConfigCommand;
            }
        }

        private void GetBranches()
        {

        }

        private void GetRebaseBranch()
        {

        }

        private void FixTags()
        {

        }

        private void FixBranches()
        {

        }

        private void FixTrunk()
        {

        }

        private void OptimizeRepos()
        {
            RunCommand("git", "gc");
        }

        private void Log(string message)
        {
            if (_options.IsVerbose)
            {
                Console.WriteLine(message);
            }
        }

        private OptionsValidateResult VerifyWorkingTreeIsClean()
        {
            string standardOutput = string.Empty;
            string standardError = string.Empty;

            int exitCode = RunCommand("git", "status --porcelain --untracked-files=no", out standardOutput, out standardError);
            if (exitCode != 0)
            {
                return OptionsValidateResult.CommandExecutionFail;
            }

            if (!string.IsNullOrWhiteSpace(standardOutput) || !string.IsNullOrWhiteSpace(standardError))
            {
                return OptionsValidateResult.WorkingTreeIsNotClean;
            }

            return OptionsValidateResult.OK;
        }

        private string GetDefaultAuthorsOption()
        {
            if (File.Exists(DEFAULT_AUTHOR_FILE))
            {
                return DEFAULT_AUTHOR_FILE;
            }

            return string.Empty;
        }

        private int RunCommand(string cmd, string arguments)
        {
            string standardOutput;
            string standardError;

            return RunCommand(cmd, arguments, out standardOutput, out standardError);
        }

        private int RunCommand(string cmd, string arguments, out string standardOutput, out string standardError)
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

            commandProcess.Start();

            standardOutput = commandProcess.StandardOutput.ReadToEnd();
            standardError = commandProcess.StandardError.ReadToEnd();

            return commandProcess.ExitCode;
        }
    }
}