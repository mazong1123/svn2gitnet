using System;

namespace Svn2GitNet
{
    public interface ICommandRunner
    {
        int Run(string cmd, string arguments);

        int Run(string cmd, string arguments, out string standardOutput);

        int Run(string cmd, string arguments, out string standardOutput, out string standardError);
    }
}