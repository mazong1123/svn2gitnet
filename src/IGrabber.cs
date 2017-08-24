using System;

namespace Svn2GitNet
{
    public interface IGrabber
    {
        void FetchBranches();
        void FetchRebaseBraches();
        void Clone();
        MetaInfo GetMetaInfo();
    }
}