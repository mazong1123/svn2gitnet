using System;
using System.Collections.Generic;

namespace Svn2GitNet
{
    public class Option
    {
        /// <summary>
        /// Be verbose in logging -- useful for debugging issues
        /// </summary>
        /// <returns></returns>
        public bool IsVerbose
        {
            get;
            set;
        }

        /// <summary>
        /// Include metadata in git logs (git-svn-id)
        /// </summary>
        /// <returns></returns>
        public bool IncludeMetaData
        {
            get;
            set;
        }

        /// <summary>
        /// Accept URLs as-is without attempting to connect to a higher level directory
        /// </summary>
        /// <returns></returns>
        public bool NoMinimizeUrl
        {
            get;
            set;
        }

        public bool RootIsTrunk
        {
            get;
            set;
        }

        /// <summary>
        /// Subpath to trunk from repository URL (default: trunk)
        /// </summary>
        /// <returns></returns>
        public string SubpathToTrunk
        {
            get;
            set;
        }

        public List<string> Branches
        {
            get;
        }

        public List<string> Tags
        {
            get;
        }

        public List<string> Exclude
        {
            get;
        }

        public string Revision
        {
            get;
        }

        public string UserName
        {
            get;
        }

        public string Password
        {
            get;
        }

        public bool RebaseBranch
        {
            get;
        }

        public string Authors
        {
            get;
        }
    }
}