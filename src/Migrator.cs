using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Svn2GitNet
{
    public class Migrator
    {
        // TODO: Add windows support.
        private const string DEFAULT_AUTHOR_FILE = "~/.svn2git/authors";
        private readonly string _dir;
        private Options _options;
        private string[] _args;

        private string _gitConfigCommandArguments;

        private string _url;

        public Migrator(Options options, string[] args)
        {
            _options = options;
            _args = args;
        }

        public MigrateResult Initialize()
        {
            if (string.IsNullOrWhiteSpace(_options.Authors))
            {
                _options.Authors = GetDefaultAuthorsOption();
            }

            if (_options.Rebase || _options.RebaseBranch)
            {
                if (_args.Length > 1)
                {
                    return MigrateResult.TooManyArguments;
                }

                return VerifyWorkingTreeIsClean();
            }
            else if (_args.Length == 0)
            {
                return MigrateResult.MissingSvnUrlParameter;
            }
            else if (_args.Length > 1)
            {
                return MigrateResult.TooManyArguments;
            }

            _url = _args[0].Replace(" ", "\\ ");

            return MigrateResult.OK;
        }

        public MigrateResult Run()
        {
            MigrateResult result = MigrateResult.OK;
            if (_options.Rebase)
            {
                GetBranches();
            }
            else if (_options.RebaseBranch)
            {
                GetRebaseBranch();
            }
            else
            {
                result = Clone();
            }

            if (result != MigrateResult.OK)
            {
                return result;
            }

            FixBranches();
            FixTags();
            FixTrunk();
            OptimizeRepos();

            return result;
        }

        private MigrateResult Clone()
        {
            StringBuilder arguments = new StringBuilder("svn init --prefix=svn/ ");
            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                arguments.AppendFormat("--username='{0}'", _options.UserName);
            }

            if (!string.IsNullOrWhiteSpace(_options.Password))
            {
                arguments.AppendFormat("--password='{0}'", _options.Password);
            }

            if (!_options.IncludeMetaData)
            {
                arguments.Append("--no-metadata ");
            }

            if (_options.NoMinimizeUrl)
            {
                arguments.Append("--no-minimize-url ");
            }

            var branches = new List<string>(_options.Branches);
            var tags = new List<string>(_options.Tags);

            if (_options.RootIsTrunk)
            {
                // Non-standard repository layout.
                // The repository root is effectively trunk.
                arguments.AppendFormat("--trunk='{0}'", _url);
                RunCommand("git", arguments.ToString());
            }
            else
            {
                // Add each component to the command that was passed as an argument.
                if (!string.IsNullOrWhiteSpace(_options.SubpathToTrunk))
                {
                    arguments.AppendFormat("--trunk='{0}'", _options.SubpathToTrunk);
                }

                if (!_options.NoTags)
                {
                    if (tags.Count == 0)
                    {
                        // Fill default tags here so that they can be filtered later
                        tags.Add("tags");
                    }

                    // Process default or user-supplied tags
                    foreach (var t in tags)
                    {
                        arguments.AppendFormat("--tags='{0}' ", t);
                    }
                }

                if (!_options.NoBranches)
                {
                    if (branches.Count == 0)
                    {
                        // Fill default branches here so that they can be filtered later
                        branches.Add("branches");
                    }

                    // Process default or user-supplied branches
                    foreach (var b in branches)
                    {
                        arguments.AppendFormat("--branches='{0}' ", b);
                    }
                }

                arguments.Append(_url);

                if (RunCommand("git", arguments.ToString()) != 0)
                {
                    return MigrateResult.FailToExecuteSvnInitCommand;
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.Authors))
            {
                RunCommand("git",
                            string.Format("{0} svn.authorsfile {1}",
                            _gitConfigCommandArguments, _options.Authors));
            }

            arguments = new StringBuilder("svn fetch ");
            if (!string.IsNullOrWhiteSpace(_options.Revision))
            {
                var range = _options.Revision.Split(":");
                string start = range[0];
                string end = range.Length < 2 || string.IsNullOrWhiteSpace(range[1]) ? "HEAD" : range[1];
                arguments.AppendFormat("-r {0}:{1}", start, end);
            }

            if (_options.Exclude != null && _options.Exclude.Any())
            {
                // Add exclude paths to the command line. Some versions of git support
                // this for fetch only, later also for init.
                List<string> regex = new List<string>();
                if (!_options.RootIsTrunk)
                {
                    if (!string.IsNullOrWhiteSpace(_options.SubpathToTrunk))
                    {
                        regex.Add(_options.SubpathToTrunk + "[/]");
                    }

                    if (!_options.NoTags && tags.Count > 0)
                    {
                        foreach(var t in tags)
                        {
                            regex.Add(t + "[/][^/]+[/]");
                        }
                    }

                    if (!_options.NoBranches && branches.Count > 0)
                    {
                        foreach(var b in branches)
                        {
                            regex.Add(b + "[/][^/]+[/]");
                        }
                    }
                }

                string regexStr = "^(?:" + string.Join("|", regex) + ")(?:" + string.Join("|", _options.Exclude) + ")";
                arguments.AppendFormat("--ignore-paths='{0}' ", regexStr);
            }

            if (RunCommand("git", arguments.ToString()) != 0)
            {
                return MigrateResult.FailToExecuteSvnFetchCommand;
            }

            GetBranches();

            return MigrateResult.OK;
        }

        private string GitConfigCommandArguments
        {
            get
            {
                if (_gitConfigCommandArguments == null)
                {
                    string standardOutput;
                    string standardError;
                    RunCommand("git", "config --local --get user.name", out standardOutput, out standardError);
                    string combinedOutput = standardOutput + standardError;
                    _gitConfigCommandArguments = Regex.IsMatch(combinedOutput, @"/unknown option/m") ? "config" : "config --local";
                }

                return _gitConfigCommandArguments;
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

        private MigrateResult VerifyWorkingTreeIsClean()
        {
            string standardOutput = string.Empty;
            string standardError = string.Empty;

            int exitCode = RunCommand("git", "status --porcelain --untracked-files=no", out standardOutput, out standardError);
            if (exitCode != 0)
            {
                return MigrateResult.FailToExecuteCommand;
            }

            if (!string.IsNullOrWhiteSpace(standardOutput) || !string.IsNullOrWhiteSpace(standardError))
            {
                return MigrateResult.WorkingTreeIsNotClean;
            }

            return MigrateResult.OK;
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