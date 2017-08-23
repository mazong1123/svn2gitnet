using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Svn2GitNet
{
    public class Migrator
    {
        // TODO: Add windows support.
        private const string DEFAULT_AUTHOR_FILE = "~/.svn2git/authors";

        private ICommandRunner _commandRunner;
        private IMessageDisplayer _messageDisplayer;

        private Options _options;
        private string[] _args;
        private string _gitConfigCommandArguments;
        private string _svnUrl;

        public Migrator(Options options, string[] args)
        : this(options, args, new CommandRunner())
        {
        }

        public Migrator(Options options, string[] args, ICommandRunner commandRunner)
        : this(options, args, commandRunner, new ConsoleMessageDisplayer())
        {
        }

        public Migrator(Options options, string[] args, ICommandRunner commandRunner, IMessageDisplayer messageDisplayer)
        {
            _options = options;
            _args = args;
            _commandRunner = commandRunner;
            _messageDisplayer = messageDisplayer;
        }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(_options.Authors))
            {
                _options.Authors = GetDefaultAuthorsOption();
            }

            if (_options.Rebase)
            {
                if (_args.Length > 1)
                {
                    throw new MigrateException(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS);
                }

                VerifyWorkingTreeIsClean();
            }
            else if (!string.IsNullOrWhiteSpace(_options.RebaseBranch))
            {
                if (_args.Length > 2)
                {
                    throw new MigrateException(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS);
                }

                VerifyWorkingTreeIsClean();
            }
            else if (_args.Length == 0)
            {
                throw new MigrateException(ExceptionHelper.ExceptionMessage.MISSING_SVN_URL_PARAMETER);
            }
            else if (_args.Length > 1)
            {
                throw new MigrateException(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS);
            }

            _svnUrl = _args[0].Replace(" ", "\\ ");
        }

        public void Run()
        {
            Grabber grabber = new Grabber(_svnUrl, _options, _commandRunner, GitConfigCommandArguments, _messageDisplayer);
            Fixer fixer = new Fixer(grabber.GetMetaInfo(), _options, _commandRunner, GitConfigCommandArguments, _messageDisplayer);

            Run(grabber, fixer);
        }

        public void Run(IGrabber grabber, IFixer fixer)
        {
            if (grabber == null)
            {
                throw new ArgumentNullException("grabber");
            }

            if (fixer == null)
            {
                throw new ArgumentNullException("fixer");
            }

            if (_options.Rebase)
            {
                grabber.FetchBranches();
            }
            else if (!string.IsNullOrWhiteSpace(_options.RebaseBranch))
            {
                grabber.FetchRebaseBraches();
            }
            else
            {
                grabber.Clone();
            }

            fixer.FixBranches();
            fixer.FixTags();
            fixer.FixTrunk();
            fixer.OptimizeRepos();
        }

        private string GitConfigCommandArguments
        {
            get
            {
                if (_gitConfigCommandArguments == null)
                {
                    string standardOutput;
                    string standardError;
                    _commandRunner.Run("git", "config --local --get user.name", out standardOutput, out standardError);
                    string combinedOutput = standardOutput + standardError;
                    _gitConfigCommandArguments = Regex.IsMatch(combinedOutput, @"/unknown option/m") ? "config" : "config --local";
                }

                return _gitConfigCommandArguments;
            }
        }

        private void VerifyWorkingTreeIsClean()
        {
            string standardOutput = string.Empty;
            string standardError = string.Empty;

            int exitCode = _commandRunner.Run("git", "status --porcelain --untracked-files=no", out standardOutput, out standardError);
            if (exitCode != 0)
            {
                throw new MigrateException($"Fail to execute command 'git status --porcelain --untracked-files=no'. Run with -v or --verbose for details.");
            }

            if (!string.IsNullOrWhiteSpace(standardOutput) || !string.IsNullOrWhiteSpace(standardError))
            {
                throw new MigrateException("You have local pending changes. The working tree must be clean in order to continue.");
            }
        }

        private string GetDefaultAuthorsOption()
        {
            if (File.Exists(DEFAULT_AUTHOR_FILE))
            {
                return DEFAULT_AUTHOR_FILE;
            }

            return string.Empty;
        }
    }
}