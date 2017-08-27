using System;

namespace Svn2GitNet
{
    public static class CommandInfoBuilder
    {
        public static CommandInfo BuildCheckoutSvnRemoteBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout -b \"{branch}\" \"remotes/svn/{branch}\""
            };
        }

        public static CommandInfo BuildCheckoutLocalBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout \"{branch}\""
            };
        }
    }
}