using System;

namespace Svn2GitNet
{
    public class MigrateException : Exception
    {
        public MigrateException(string message)
        : base(message)
        {
        }

        public MigrateException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }
}