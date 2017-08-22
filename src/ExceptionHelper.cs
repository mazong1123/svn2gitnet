using System;

namespace Svn2GitNet
{
    public static class ExceptionHelper
    {
        public static class ExceptionMessage
        {
            public const string TOO_MANY_ARGUMENTS = "Too many arguments.";
            public const string MISSING_SVN_URL_PARAMETER = "Missing SVN_URL parameter.";
            public const string FAIL_TO_EXECUTE_COMMAND = "Fail to execute command '{0}'";
            public const string TOO_MANY_MATCHING_LOCAL_BRANCHES = "Too many matching local branches found.";
            public const string TOO_MANY_MATCHING_REMOTE_BRANCHES = "Too many matching remote branches found.";
            public const string NO_REMOTE_BRANCH_FOUND = "No remote branch named '{0}' found.";
            public const string NO_LOCAL_BRANCH_FOUND = "No local branch named '{0}' found.";
        }
    }
}