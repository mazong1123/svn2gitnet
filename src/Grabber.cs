using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Svn2GitNet
{
    public class Grabber : Worker, IGrabber
    {
        private string _svnUrl = string.Empty;
        private MetaInfo _metaInfo = null;

        public Grabber(string svnUrl,
                       Options options,
                       ICommandRunner commandRunner,
                       string gitConfigCommandArguments,
                       IMessageDisplayer messageDisplayer)
        : base(options, commandRunner, gitConfigCommandArguments, messageDisplayer)
        {
            _svnUrl = svnUrl;
            _metaInfo = new MetaInfo()
            {
                RemoteBranches = new List<string>(),
                LocalBranches = new List<string>(),
                Tags = new List<string>()
            };
        }

        public void Clone()
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
                arguments.AppendFormat("--trunk='{0}'", _svnUrl);
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

                arguments.Append(_svnUrl);

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

            FetchBranches();
        }

        public void FetchBranches()
        {
            _metaInfo.LocalBranches = FetchBranchesWorker(true);
            _metaInfo.RemoteBranches = FetchBranchesWorker(false);

            // Tags are remote branches that start with "tags/".
            _metaInfo.Tags = _metaInfo.RemoteBranches.ToList().FindAll(r => Regex.IsMatch(r.Trim(), @"^svn\/tags\/"));
        }

        public void FetchRebaseBraches()
        {
            FetchBranches();

            _metaInfo.LocalBranches = _metaInfo.LocalBranches.ToList().FindAll(l => l == _options.RebaseBranch);
            _metaInfo.RemoteBranches = _metaInfo.RemoteBranches.ToList().FindAll(r => r == _options.RebaseBranch);

            if (_metaInfo.LocalBranches.Count() > 1)
            {
                throw new MigrateException("Too many matching local branches found.");
            }

            if (!_metaInfo.LocalBranches.Any())
            {
                throw new MigrateException(string.Format(ExceptionHelper.ExceptionMessage.NO_LOCAL_BRANCH_FOUND, _options.RebaseBranch));
            }

            if (_metaInfo.RemoteBranches.Count() > 2)
            {
                // 1 if remote is not pushed, 2 if its pushed to remote.
                throw new MigrateException("Too many matching remote branches found.");
            }

            if (!_metaInfo.RemoteBranches.Any())
            {
                throw new MigrateException($"No remote branch named '{_options.RebaseBranch}' found.");
            }

            string foundLocalBranch = _metaInfo.LocalBranches.First();
            ShowMessageIfPossible($"Local branches '{foundLocalBranch}' found");

            string foundRemoteBranches = string.Join(" ", _metaInfo.RemoteBranches);
            ShowMessageIfPossible($"Remote branches '{foundRemoteBranches}' found");

            // We only rebase the specified branch
            _metaInfo.Tags = null;
        }

        public MetaInfo GetMetaInfo()
        {
            return _metaInfo;
        }

        private IEnumerable<string> FetchBranchesWorker(bool isLocal)
        {
            // Get the list of local and remote branches, taking care to ignore console color codes and ignoring the
            // '*' character used to indicate the currently selected branch.
            string parameter = isLocal ? "l" : "r";
            string branchInfo = RunCommandIgnoreExitCode("git", $"branch -{parameter} --no-color");

            IEnumerable<string> branches = new List<string>();
            if (!string.IsNullOrWhiteSpace(branchInfo))
            {
                branches = branchInfo
                           .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                           .Select(x => x.Replace("*", "").Trim());
            }

            return branches;
        }
    }
}