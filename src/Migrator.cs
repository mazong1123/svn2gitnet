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

        private ICommandRunner _commandRunner;
        private IMessageDisplayer _messageDisplayer;

        private Options _options;
        private string[] _args;
        private string _gitConfigCommandArguments;
        private string _url;
        private IEnumerable<string> _localBranches;
        private IEnumerable<string> _remoteBranches;
        private IEnumerable<string> _tags;

        public Migrator(Options options, string[] args)
        : this(options, args, new CommandRunner())
        {
        }

        public Migrator(Options options, string[] args, ICommandRunner commandRunner)
        : this(options, args, commandRunner, new ConsoleMessageDisplayer())
        {
        }

        public Migrator(Options options, string[] args, ICommandRunner commandRunner, IMessageDisplayer messageDisplayer)
        {
            _options = options;
            _args = args;
            _commandRunner = commandRunner;
            _messageDisplayer = messageDisplayer;
        }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(_options.Authors))
            {
                _options.Authors = GetDefaultAuthorsOption();
            }

            if (_options.Rebase || !string.IsNullOrWhiteSpace(_options.RebaseBranch))
            {
                if (_args.Length > 1)
                {
                    throw new MigrateException(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS);
                }

                VerifyWorkingTreeIsClean();
            }
            else if (_args.Length == 0)
            {
                throw new MigrateException(ExceptionHelper.ExceptionMessage.MISSING_SVN_URL_PARAMETER);
            }
            else if (_args.Length > 1)
            {
                throw new MigrateException(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS);
            }

            _url = _args[0].Replace(" ", "\\ ");
        }

        public void Run()
        {
            if (_options.Rebase)
            {
                GetBranches();
            }
            else if (!string.IsNullOrWhiteSpace(_options.RebaseBranch))
            {
                GetRebaseBranch();
            }
            else
            {
                Clone();
            }

            FixBranches();
            FixTags();
            FixTrunk();
            OptimizeRepos();
        }

        private void Clone()
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
                _commandRunner.Run("git", arguments.ToString());
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

                if (_commandRunner.Run("git", arguments.ToString()) != 0)
                {
                    throw new MigrateException($"Fail to execute command 'git {arguments.ToString()}'. Run with -v or --verbose for details.");
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.Authors))
            {
                _commandRunner.Run("git",
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
                        foreach (var t in tags)
                        {
                            regex.Add(t + "[/][^/]+[/]");
                        }
                    }

                    if (!_options.NoBranches && branches.Count > 0)
                    {
                        foreach (var b in branches)
                        {
                            regex.Add(b + "[/][^/]+[/]");
                        }
                    }
                }

                string regexStr = "^(?:" + string.Join("|", regex) + ")(?:" + string.Join("|", _options.Exclude) + ")";
                arguments.AppendFormat("--ignore-paths='{0}' ", regexStr);
            }

            if (_commandRunner.Run("git", arguments.ToString()) != 0)
            {
                throw new MigrateException($"Fail to execute command 'git {arguments.ToString()}'. Run with -v or --verbose for details.");
            }

            GetBranches();
        }

        private string GitConfigCommandArguments
        {
            get
            {
                if (_gitConfigCommandArguments == null)
                {
                    string standardOutput;
                    string standardError;
                    _commandRunner.Run("git", "config --local --get user.name", out standardOutput, out standardError);
                    string combinedOutput = standardOutput + standardError;
                    _gitConfigCommandArguments = Regex.IsMatch(combinedOutput, @"/unknown option/m") ? "config" : "config --local";
                }

                return _gitConfigCommandArguments;
            }
        }

        private void GetBranches()
        {
            // Get the list of local and remote branches, taking care to ignore console color codes and ignoring the
            // '*' character used to indicate the currently selected branch.
            string standardOutput = string.Empty;
            string standardError = string.Empty;
            _commandRunner.Run("git", "branch -l --no-color", out standardOutput, out standardError);
            _localBranches = standardOutput
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Replace("*", "").Trim());

            _commandRunner.Run("git", "branch -r --no-color", out standardOutput, out standardError);
            _remoteBranches = standardOutput
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Replace("*", "").Trim());

            // Tags are remote branches that start with "tags/".
            _tags = _remoteBranches.ToList().FindAll(r => Regex.IsMatch(r.Trim(), @"%r{^svn\/tags\/"));
        }

        private void GetRebaseBranch()
        {
            GetBranches();

            _localBranches = _localBranches.ToList().FindAll(l => l == _options.RebaseBranch);
            _remoteBranches = _remoteBranches.ToList().FindAll(r => r == _options.RebaseBranch);

            if (_localBranches.Count() > 1)
            {
                throw new MigrateException("Too many matching local branches found.");
            }

            if (!_localBranches.Any())
            {
                throw new MigrateException(string.Format(ExceptionHelper.ExceptionMessage.NO_LOCAL_BRANCH_FOUND, _options.RebaseBranch));
            }

            if (_remoteBranches.Count() > 2)
            {
                // 1 if remote is not pushed, 2 if its pushed to remote.
                throw new MigrateException("Too many matching remote branches found.");
            }

            if (!_remoteBranches.Any())
            {
                throw new MigrateException($"No remote branch named '{_options.RebaseBranch}' found.");
            }

            string foundLocalBranch = _localBranches.First();
            _messageDisplayer.Show($"Local branches '{foundLocalBranch}' found");

            string foundRemoteBranches = string.Join(" ", _remoteBranches);
            _messageDisplayer.Show($"Remote branches '{foundRemoteBranches}' found");

            // We only rebase the specified branch
            _tags = null;
        }

        private void FixTags()
        {
            string currentUserName;
            _commandRunner.Run("git", $"{_gitConfigCommandArguments} --get user.name", out currentUserName);

            string currentUserEmail;
            _commandRunner.Run("git", $"{_gitConfigCommandArguments} --get user.email", out currentUserEmail);

            if (_tags != null)
            {
                foreach(string t in _tags)
                {
                    string tag = t.Trim();
                    string id = Regex.Replace(tag, @"%r{^svn\/tags\/}", "").Trim();

                    // TODO:
                }
            }
        }

        private void FixBranches()
        {

        }

        private void FixTrunk()
        {

        }

        private void OptimizeRepos()
        {
            _commandRunner.Run("git", "gc");
        }

        private void Log(string message)
        {
            if (_options.IsVerbose)
            {
                Console.WriteLine(message);
            }
        }

        private void VerifyWorkingTreeIsClean()
        {
            string standardOutput = string.Empty;
            string standardError = string.Empty;

            int exitCode = _commandRunner.Run("git", "status --porcelain --untracked-files=no", out standardOutput, out standardError);
            if (exitCode != 0)
            {
                throw new MigrateException($"Fail to execute command 'git status --porcelain --untracked-files=no'. Run with -v or --verbose for details.");
            }

            if (!string.IsNullOrWhiteSpace(standardOutput) || !string.IsNullOrWhiteSpace(standardError))
            {
                throw new MigrateException("You have local pending changes. The working tree must be clean in order to continue.");
            }
        }

        private string GetDefaultAuthorsOption()
        {
            if (File.Exists(DEFAULT_AUTHOR_FILE))
            {
                return DEFAULT_AUTHOR_FILE;
            }

            return string.Empty;
        }
    }
}