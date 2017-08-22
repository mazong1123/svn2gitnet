using System;

namespace Svn2GitNet
{
    public interface IMessageDisplayer
    {
        void Show(string message);
    }
}