using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet
{
    public class Fixer : InteractiveWorker, IFixer
    {
        private MetaInfo _metaInfo;

        public Fixer(MetaInfo metaInfo,
                     Options options,
                     ICommandRunner commandRunner,
                     string gitConfigCommandArguments,
                     IMessageDisplayer messageDisplayer,
                     ILogger logger)
        : base(options, commandRunner, gitConfigCommandArguments, messageDisplayer, logger)
        {
            _metaInfo = metaInfo;
        }

        public void FixBranches()
        {
            Log("Start fixing branches...");
            List<string> svnBranches = new List<string>();
            if (_metaInfo.RemoteBranches != null)
            {
                if (_metaInfo.Tags == null)
                {
                    svnBranches = _metaInfo.RemoteBranches.ToList();
                }
                else
                {
                    svnBranches = _metaInfo.RemoteBranches.Except(_metaInfo.Tags).ToList();
                }

                svnBranches.RemoveAll(b => !Regex.IsMatch(b.Trim(), @"^svn\/"));
            }

            if (Options.IsVerbose)
            {
                Log("To fix branches include:");
                foreach (var b in svnBranches)
                {
                    Log(b);
                }
            }

            if (Options.Rebase)
            {
                Log("Rebasing...");
                CommandInfo cmdInfo = CommandInfoBuilder.BuildGitSvnFetchCommandInfo(Options.UserName);

                int exitCode = 0;
                if (string.IsNullOrWhiteSpace(Options.UserName))
                {
                    exitCode = RunCommand(cmdInfo);
                }
                else
                {
                    exitCode = CommandRunner.RunGitSvnInteractiveCommand(cmdInfo.Arguments, Options.Password);
                }

                if (exitCode != 0)
                {
                    throw new MigrateException(string.Format(ExceptionHelper.ExceptionMessage.FAIL_TO_EXECUTE_COMMAND, cmdInfo.ToString()));
                }
            }

            // In case of large branches, we build a hash set to boost the query later.
            HashSet<string> localBranchSet = new HashSet<string>(_metaInfo.LocalBranches);
            if (Options.IsVerbose)
            {
                StringBuilder sb = new StringBuilder("We have following local branches:");
                sb.AppendLine();

                foreach (var b in _metaInfo.LocalBranches)
                {
                    sb.AppendLine(b);
                }

                Log(sb.ToString());
            }

            foreach (var b in svnBranches)
            {
                var branch = Regex.Replace(b, @"^svn\/", "").Trim();
                bool isTrunkBranchOrIsLocalBranch = branch.Equals("trunk", StringComparison.InvariantCulture)
                                                    || localBranchSet.Contains(branch);

                Log($"Current branch is {b}");
                Log($"Current branch without prefix: {branch}");
                Log($"IsTrunkBranchOrIsLocalBranch: {isTrunkBranchOrIsLocalBranch}");
                if (Options.Rebase && isTrunkBranchOrIsLocalBranch)
                {
                    string localBranch = branch == "trunk" ? "master" : branch;
                    CommandInfo forceCheckoutLocalBranchCommandInfo = CommandInfoBuilder.BuildForceCheckoutLocalBranchCommandInfo(localBranch);
                    CommandInfo rebaseRemoteBranchCommandInfo = CommandInfoBuilder.BuildGitRebaseRemoteSvnBranchCommandInfo(branch);

                    RunCommand(CommandInfoBuilder.BuildForceCheckoutLocalBranchCommandInfo(localBranch));

                    RunCommand(CommandInfoBuilder.BuildGitRebaseRemoteSvnBranchCommandInfo(branch));

                    continue;
                }

                if (isTrunkBranchOrIsLocalBranch)
                {
                    Log($"{branch} is trunk branch or local branch, skip.");
                    continue;
                }

                // Now checkout the remote svn branch.
                RunCommand(CommandInfoBuilder.BuildCheckoutSvnRemoteBranchCommandInfo(branch));
            }

            Log("End fixing branches.");
        }

        public void FixTags()
        {
            string currentUserName = string.Empty;
            string currentUserEmail = string.Empty;
            try
            {
                if (_metaInfo.Tags != null)
                {
                    Log("Reading user.name and user.email...");

                    CommandRunner.Run("git", $"{GitConfigCommandArguments} --get user.name", out currentUserName);
                    CommandRunner.Run("git", $"{GitConfigCommandArguments} --get user.email", out currentUserEmail);

                    Log($"user.name: {currentUserName}");
                    Log($"user.email: {currentUserEmail}");

                    foreach (string t in _metaInfo.Tags)
                    {
                        string tag = t.Trim();
                        Log($"Processing tag: {tag}");

                        string id = Regex.Replace(tag, @"^svn\/tags\/", "").Trim();
                        Log($"id: {id}");

                        string quotesFreeTag = Utils.EscapeQuotes(tag);
                        Log($"quotes free tag: {tag}");

                        string subject = Utils.RemoveFromTwoEnds(RunCommandIgnoreExitCode("git", $"log -1 --pretty=format:'%s' \"{quotesFreeTag}\""), '\'');
                        string date = Utils.RemoveFromTwoEnds(RunCommandIgnoreExitCode("git", $"log -1 --pretty=format:'%ci' \"{quotesFreeTag}\""), '\'');
                        string author = Utils.RemoveFromTwoEnds(RunCommandIgnoreExitCode("git", $"log -1 --pretty=format:'%an' \"{quotesFreeTag}\""), '\'');
                        string email = Utils.RemoveFromTwoEnds(RunCommandIgnoreExitCode("git", $"log -1 --pretty=format:'%ae' \"{quotesFreeTag}\""), '\'');

                        string quotesFreeAuthor = Utils.EscapeQuotes(author);
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} user.name \"{quotesFreeAuthor}\"");
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} user.email \"{quotesFreeAuthor}\"");

                        string originalGitCommitterDate = Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
                        Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", Utils.EscapeQuotes(date));
                        CommandRunner.Run("git", $"tag -a -m \"{Utils.EscapeQuotes(subject)}\" \"{Utils.EscapeQuotes(id)}\" \"{quotesFreeTag}\"");
                        Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", originalGitCommitterDate);

                        CommandRunner.Run("git", $"branch -d -r \"{quotesFreeTag}\"");
                    }
                }
            }
            finally
            {
                // We only change the git config values if there are @tags available.
                // So it stands to reason we should revert them only in that case.
                if (_metaInfo.Tags != null && _metaInfo.Tags.Any())
                {
                    // If a line was read, then there was a config value so restore it.
                    // Otherwise unset the value because originally there was none.
                    if (!string.IsNullOrWhiteSpace(currentUserName))
                    {
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} user.name \"{currentUserName.Trim()}\"");
                    }
                    else
                    {
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} --unset user.name");
                    }

                    if (!string.IsNullOrWhiteSpace(currentUserEmail))
                    {
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} user.email \"{currentUserEmail.Trim()}\"");
                    }
                    else
                    {
                        CommandRunner.Run("git", $"{GitConfigCommandArguments} --unset user.email");
                    }
                }
            }
        }

        public void FixTrunk()
        {
            if (_metaInfo.RemoteBranches != null)
            {
                string trunkBranch = _metaInfo.RemoteBranches.ToList().Find(b => b.Trim().Equals("trunk"));
                if (trunkBranch != null && !Options.Rebase)
                {
                    CommandRunner.Run("git", "checkout svn/trunk");
                    CommandRunner.Run("git", "branch -D master");
                    CommandRunner.Run("git", "checkout -f -b master");

                    return;
                }
            }

            CommandRunner.Run("git", "checkout -f master");
        }

        public void OptimizeRepos()
        {
            CommandRunner.Run("git", "gc");
        }
    }
}