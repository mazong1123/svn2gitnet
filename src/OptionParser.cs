using System;
using System.IO;

namespace Svn2GitNet
{
    public class OptionParser
    {
        // TODO: Add windows support.
        private readonly string _defaultAuthorFile = "~/.svn2git/authors";

        private string[] _args;

        public OptionParser(string[] args)
        {
            _args = args;
        }

        public Options Parse()
        {
            // Setup default options.
            Options opt = new Options()
            {
                IsVerbose = false,
                IncludeMetaData = false,
                NoMinimizeUrl = false,
                RootIsTrunk = false,
                SubpathToTrunk = "trunk",
                RebaseBranch = false
            };

            opt.Authors = GetDefaultAuthorsOption();

            return opt;
        }

        public OptionValidateResult Validate(Options option)
        {
            // TODO:
            return OptionValidateResult.OK;
        }

        virtual protected string GetDefaultAuthorsOption()
        {
            if (File.Exists(_defaultAuthorFile))
            {
                return _defaultAuthorFile;
            }

            return string.Empty;
        }
    }
}