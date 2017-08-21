using System;

namespace Svn2GitNet
{
    public enum OptionsValidateResult
    {
        OK,
        TooManyArguments,
        MissingSvnUrlParameter,
        WorkingTreeIsNotClean,
        CommandExecutionFail
    }
}