using System;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet
{
    public class Worker
    {
        private Options _options;
        private ICommandRunner _commandRunner;
        private IMessageDisplayer _messageDisplayer;
        private ILogger _logger;

        public Worker(Options options,
                      ICommandRunner commandRunner,
                      IMessageDisplayer messageDisplayer,
                      ILogger logger)
        {
            _options = options;
            _commandRunner = commandRunner;
            _messageDisplayer = messageDisplayer;
            _logger = logger;
        }

        protected Options Options
        {
            get
            {
                return _options;
            }

            set
            {
                _options = value;
            }
        }

        protected ICommandRunner CommandRunner
        {
            get
            {
                return _commandRunner;
            }

            set
            {
                _commandRunner = value;
            }
        }

        protected IMessageDisplayer MessageDisplayer
        {
            get
            {
                return _messageDisplayer;
            }

            set
            {
                _messageDisplayer = value;
            }
        }

        protected ILogger Logger
        {
            get
            {
                return _logger;
            }

            set
            {
                _logger = value;
            }
        }

        protected void ShowMessageIfPossible(string message)
        {
            if (_messageDisplayer != null)
            {
                _messageDisplayer.Show(message);
            }
        }

        protected void Log(string message)
        {
            if (_logger != null && _options.IsVerbose)
            {
                _logger.LogInformation(message);
            }
        }

        protected string RunCommandIgnoreExitCode(CommandInfo cmdInfo)
        {
            return RunCommandIgnoreExitCode(cmdInfo.Command, cmdInfo.Arguments);
        }

        protected string RunCommandIgnoreExitCode(string cmd, string arguments)
        {
            string standardOutput;
            _commandRunner.Run(cmd, arguments, out standardOutput);

            return standardOutput;
        }

        protected int RunCommand(CommandInfo cmdInfo)
        {
            return _commandRunner.Run(cmdInfo.Command, cmdInfo.Arguments);
        }

        protected int RunCommand(CommandInfo cmdInfo, out string standardOutput)
        {
            return _commandRunner.Run(cmdInfo.Command, cmdInfo.Arguments, out standardOutput);
        }

        protected int RunCommand(CommandInfo cmdInfo, out string standardOutput, out string standardError)
        {
            return _commandRunner.Run(cmdInfo.Command, cmdInfo.Arguments, out standardOutput, out standardError);
        }
    }
}