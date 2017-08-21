using System;

namespace Svn2GitNet
{
    public class OptionParser
    {
        private string[] _args;

        public OptionParser(string[] args)
        {
            _args = args;
        }

        public Option Parse()
        {
            Option opt = new Option();

            // TODO:

            return opt;
        }

        public OptionValidateResult Validate(Option option)
        {
            // TODO:
            return OptionValidateResult.OK;
        }
    }
}