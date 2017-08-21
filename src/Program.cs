using System;

namespace Svn2GitNet
{
    class Program
    {
        static void ShowHelperMessage(OptionValidateResult opt)
        {
            switch (opt)
            {
                case OptionValidateResult.MissingSvnUrlParameter:
                    Console.WriteLine("Missing SVN_URL parameter.");
                    break;
                case OptionValidateResult.TooManyArguments:
                    Console.WriteLine("Too many arguments.");
                    break;
                default:
                    break;
            }
        }

        static void Main(string[] args)
        {
            OptionParser parser = new OptionParser(args);
            Option opt = parser.Parse();
            OptionValidateResult validateResult = parser.Validate(opt);
            if (validateResult != OptionValidateResult.OK)
            {
                ShowHelperMessage(validateResult);
                return;
            }
        }
    }
}
