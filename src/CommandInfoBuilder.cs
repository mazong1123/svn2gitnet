using System;

namespace Svn2GitNet
{
    public static class CommandInfoBuilder
    {
        public static CommandInfo BuildCheckoutSvnBranchCommandInfo(string branch)
        {
            return new CommandInfo()
            {
                Command = "git",
                Arguments = $"checkout -b \"{branch}\" \"remotes/svn/{branch}\""
            };
        }
    }
}