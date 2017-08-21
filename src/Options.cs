using System;
using System.Collections.Generic;
using CommandLine;

namespace Svn2GitNet
{
    public class Options
    {
        private string _trunk;
        private IEnumerable<string> _tags;
        private IEnumerable<string> _branches;

        /// <summary>
        /// Be verbose in logging -- useful for debugging issues
        /// </summary>
        /// <returns></returns>
        [Option('v', "verbose", Default = false, HelpText = "Be verbose in logging -- useful for debugging issues")]
        public bool IsVerbose
        {
            get;
            set;
        }

        /// <summary>
        /// Include metadata in git logs (git-svn-id)
        /// </summary>
        /// <returns></returns>
        [Option('m', "metadata", Default = false, HelpText = "Include metadata in git logs (git-svn-id)")]
        public bool IncludeMetaData
        {
            get;
            set;
        }

        /// <summary>
        /// Accept URLs as-is without attempting to connect to a higher level directory
        /// </summary>
        /// <returns></returns>
        [Option("no-minimize-url", HelpText = "Accept URLs as-is without attempting to connect to a higher level directory")]
        public bool NoMinimizeUrl
        {
            get;
            set;
        }

        [Option("rootistrunk", HelpText = "Use this if the root level of the repo is equivalent to the trunk and there are no tags or branches")]
        public bool RootIsTrunk
        {
            get;
            set;
        }

        /// <summary>
        /// Subpath to trunk from repository URL (default: trunk)
        /// </summary>
        /// <returns></returns>
        [Option("trunk", Default = "trunk", HelpText = "Subpath to trunk from repository URL (default: trunk)")]
        public string SubpathToTrunk
        {
            get
            {
                return _trunk;
            }

            set
            {
                _trunk = value;
            }
        }

        [Option("notrunk", HelpText = "Do not import anything from trunk")]
        public string NoTrunk
        {
            get
            {
                return _trunk;
            }

            set
            {
                _trunk = string.Empty;
            }
        }

        [Option("branches", HelpText = "Subpath to branches from repository URL (default: branches); can be used multiple times")]
        public IEnumerable<string> Branches
        {
            get
            {
                return _branches;
            }

            set
            {
                _branches = value;
            }
        }

        [Option("nobranches", HelpText = "Do not try to import any branches")]
        public IEnumerable<string> NoBranches
        {
            get
            {
                return _branches;
            }

            set
            {
                _branches = null;
            }
        }

        [Option("tags", HelpText = "Subpath to tags from repository URL (default: tags); can be used multiple times")]
        public IEnumerable<string> Tags
        {
            get
            {
                return _tags;
            }

            set
            {
                _tags = value;
            }
        }


        [Option("notags", HelpText = "Do not try to import any tags")]
        public IEnumerable<string> NoTags
        {
            get
            {
                return _tags;
            }

            set
            {
                _tags = null;
            }
        }

        [Option("exclude", HelpText = "Specify a Perl regular expression to filter paths when fetching; can be used multiple times")]
        public IEnumerable<string> Exclude
        {
            get;
            set;
        }

        [Option("revision", HelpText = "Start importing from SVN revision START_REV; optionally end at END_REV")]
        public string Revision
        {
            get;
            set;
        }

        [Option("username", HelpText = "Username for transports that needs it (http(s), svn)")]
        public string UserName
        {
            get;
            set;
        }

        [Option("password", HelpText = "Password for transports that need it (http(s), svn)")]
        public string Password
        {
            get;
            set;
        }

        [Option("rebase", HelpText = "Instead of cloning a new project, rebase an existing one against SVN")]
        public bool Rebase
        {
            get;
            set;
        }

        [Option("rebasebranch", HelpText = "Rebase specified branch")]
        public bool RebaseBranch
        {
            get;
            set;
        }

        [Option("authors", HelpText = "Path to file containing svn-to-git authors mapping")]
        public string Authors
        {
            get;
            set;
        }
    }
}