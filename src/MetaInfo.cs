using System;
using System.Collections.Generic;

namespace Svn2GitNet
{
    public class MetaInfo
    {
        public IEnumerable<string> LocalBranches
        {
            get;
            set;
        }

        public IEnumerable<string> RemoteBranches
        {
            get;
            set;
        }

        public IEnumerable<string> Tags
        {
            get;
            set;
        }
    }
}