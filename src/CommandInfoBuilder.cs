using System;

namespace Svn2GitNet
{
    public static class CommandInfoBuilder
    {
        /// <summary>
        /// Build command "git checkout -b "{branch}" "remotes/svn/{branch}""
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildCheckoutSvnRemoteBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout -b \"{branch}\" \"remotes/svn/{branch}\""
            };
        }

        /// <summary>
        /// Build command "git checkout "{branch}""
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildCheckoutLocalBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout \"{branch}\""
            };
        }

        /// <summary>
        /// Build command "git checkout -f "{branch}""
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildForceCheckoutLocalBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout -f \"{branch}\""
            };
        }

        /// <summary>
        /// Build command "git rebase "remotes/svn/{branch}""
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildGitRebaseRemoteSvnBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"rebase \"remotes/svn/{branch}\""
            };
        }

        /// <summary>
        /// Build command "git svn fetch"
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildGitSvnFetchCommandInfo()
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = "svn fetch"
            };
        }

        /// <summary>
        /// Build command "git branch --track "{branch}" "remotes/svn/{branch}""
        /// </summary>
        /// <returns>Built command info.</returns>
        public static CommandInfo BuildGitBranchTrackCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"branch --track \"{branch}\" \"remotes/svn/{branch}\""
            };
        }
    }
}