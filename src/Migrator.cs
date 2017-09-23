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
        private readonly string _defaultAuthorsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".svn2gitnet", "authors");
        private readonly string _gitSvnCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".subversion", "auth");

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

            try
            {
                PreRunPrepare();

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
            finally
            {
                PostRunCleanup();
            }
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
                    _gitConfigCommandArguments = Regex.IsMatch(combinedOutput, @"(?m)unknown option") ? "config" : "config --local";
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
            if (File.Exists(_defaultAuthorsFile))
            {
                return _defaultAuthorsFile;
            }

            return string.Empty;
        }

        private void PreRunPrepare()
        {
            try
            {
                string svnSimpleFolder = Path.Combine(_gitSvnCacheDirectory, "svn.simple");
                if (!Directory.Exists(svnSimpleFolder))
                {
                    return;
                }

                var cacheFiles = Directory.GetFiles(svnSimpleFolder);
                if (cacheFiles.Length > 0)
                {
                    Console.WriteLine("Temporarily disabling the cached credentials...");
                    foreach (var cf in cacheFiles)
                    {
                        if (string.IsNullOrEmpty(Path.GetExtension(cf)))
                        {
                            string newFileName = cf + ".svn2gitnet";
                            if (File.Exists(newFileName))
                            {
                                File.Delete(newFileName);
                            }

                            File.Move(cf, newFileName);
                        }
                    }
                    Console.WriteLine("The cached credentials are disabled.");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to disable the cached credentials. We'll use the cached credentials for further actions.");
                Log(ex.ToString());
            }
        }

        private void PostRunCleanup()
        {
            try
            {
                string svnSimpleFolder = Path.Combine(_gitSvnCacheDirectory, "svn.simple");
                if (!Directory.Exists(svnSimpleFolder))
                {
                    return;
                }

                var cacheFiles = Directory.GetFiles(svnSimpleFolder);
                if (cacheFiles.Length > 0)
                {
                    Console.WriteLine("Recoverying cached credentials...");
                    foreach (var cf in cacheFiles)
                    {
                        if (string.IsNullOrEmpty(Path.GetExtension(cf)))
                        {
                            continue;
                        }

                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(cf);
                        var cacheFilePath = Path.Combine(svnSimpleFolder, fileNameWithoutExt);
                        if (!File.Exists(cacheFilePath))
                        {
                            File.Move(cf, cacheFilePath);
                        }
                        else
                        {
                            // A new cache file with the same hash generated.
                            // No need to recover the old one.
                            File.Delete(cf);
                        }
                    }
                    Console.WriteLine("Cached credentials recovered");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to recover the cached credentials.");
                Log(ex.ToString());
            }
        }

        private void Log(string message)
        {
            if (_options.IsVerbose)
            {
                Console.WriteLine(message);
            }
        }
    }
}