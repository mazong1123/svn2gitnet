using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet
{
    public class Migrator : Worker
    {
        private readonly string _defaultAuthorsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".svn2gitnet", "authors");
        private readonly string _gitSvnCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".subversion", "auth");

        private string[] _args;
        private string _gitConfigCommandArguments;
        private string _svnUrl;
        private ILoggerFactory _loggerFactory;

        public Migrator(Options options,
                        string[] args,
                        ICommandRunner commandRunner,
                        IMessageDisplayer messageDisplayer,
                        ILoggerFactory loggerFactory)
            : base(options, commandRunner, messageDisplayer, loggerFactory.CreateLogger<Migrator>())
        {
            _args = args;
            _loggerFactory = loggerFactory;
        }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(Options.Authors))
            {
                Options.Authors = GetDefaultAuthorsOption();
            }

            if (Options.Rebase)
            {
                VerifyWorkingTreeIsClean();
            }
            else if (!string.IsNullOrWhiteSpace(Options.RebaseBranch))
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
            Grabber grabber = new Grabber(_svnUrl,
                                          Options,
                                          CommandRunner,
                                          GitConfigCommandArguments,
                                          MessageDisplayer,
                                          _loggerFactory.CreateLogger<Grabber>());

            Fixer fixer = new Fixer(grabber.GetMetaInfo(),
                                    Options,
                                    CommandRunner,
                                    GitConfigCommandArguments,
                                    MessageDisplayer,
                                    _loggerFactory.CreateLogger<Fixer>());

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

                if (Options.Rebase)
                {
                    grabber.FetchBranches();
                }
                else if (!string.IsNullOrWhiteSpace(Options.RebaseBranch))
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
                    CommandRunner.Run("git", "config --local --get user.name", out standardOutput, out standardError);
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

            int exitCode = CommandRunner.Run("git", "status --porcelain --untracked-files=no", out standardOutput, out standardError);
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
                    MessageDisplayer.Show("Temporarily disabling the cached credentials...");
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
                    MessageDisplayer.Show("The cached credentials are disabled.");
                }
            }
            catch (IOException ex)
            {
                MessageDisplayer.Show("Failed to disable the cached credentials. We'll use the cached credentials for further actions.");
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
                    MessageDisplayer.Show("Recoverying cached credentials...");
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
                    MessageDisplayer.Show("Cached credentials recovered");
                }
            }
            catch (IOException ex)
            {
                MessageDisplayer.Show("Failed to recover the cached credentials.");
                Log(ex.ToString());
            }
        }
    }
}