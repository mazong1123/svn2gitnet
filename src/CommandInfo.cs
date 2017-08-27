using System;

namespace Svn2GitNet
{
    public class CommandInfo
    {
        public string Command
        {
            get;
            set;
        }

        public string Arguments
        {
            get;
            set;
        }

        /// <summary>
        /// Build and output the full command string with arguments.
        /// </summary>
        /// <returns>Full command string with arguments</returns>
        public override string ToString()
        {
            return Command + " " + Arguments;
        }
    }
}