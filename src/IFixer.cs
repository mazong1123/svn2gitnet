using System;

namespace Svn2GitNet
{
    public interface IFixer
    {
        void FixBranches();
        void FixTags();
        void FixTrunk();
        void OptimizeRepos();
    }
}