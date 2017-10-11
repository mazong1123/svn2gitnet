using System;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet
{
    public class InteractiveWorker : Worker
    {
        private string _gitConfigCommandArguments;

        public InteractiveWorker(Options options,
                      ICommandRunner commandRunner,
                      string gitConfigCommandArguments,
                      IMessageDisplayer messageDisplayer,
                      ILogger logger)
        : base(options, commandRunner, messageDisplayer, logger)
        {
            _gitConfigCommandArguments = gitConfigCommandArguments;
        }

        public string GitConfigCommandArguments
        {
            get
            {
                return _gitConfigCommandArguments;
            }

            set
            {
                _gitConfigCommandArguments = value;
            }
        }
    }
}