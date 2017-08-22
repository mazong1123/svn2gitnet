using System;

namespace Svn2GitNet
{
    public class ConsoleMessageDisplayer : IMessageDisplayer
    {
        public void Show(string message)
        {
            Console.WriteLine(message);
        }
    }
}