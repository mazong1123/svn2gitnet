using System;

namespace Svn2GitNet
{
    public enum MigrateResult
    {
        OK,
        TooManyArguments,
        MissingSvnUrlParameter,
        WorkingTreeIsNotClean,
        FailToExecuteCommand,
        FailToInitSvn
    }
}