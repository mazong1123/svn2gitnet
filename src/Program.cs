using System;
using CommandLine;

namespace Svn2GitNet
{
    class Program
    {
        static void ShowValidateErrorMessage(OptionsValidateResult validateResult)
        {
            switch (validateResult)
            {
                case OptionsValidateResult.MissingSvnUrlParameter:
                    Console.WriteLine("Missing SVN_URL parameter.");
                    break;
                case OptionsValidateResult.TooManyArguments:
                    Console.WriteLine("Too many arguments.");
                    break;
                case OptionsValidateResult.WorkingTreeIsNotClean:
                    Console.WriteLine("You have local pending changes.  The working tree must be clean in order to continue.");
                    break;
                case OptionsValidateResult.CommandExecutionFail:
                    Console.WriteLine("Command execution fail.");
                    break;
                default:
                    break;
            }
        }

        static void Migrate(Options options, string[] args)
        {
            Migrator migrator = new Migrator(options, args);
            OptionsValidateResult validateResult = migrator.ValidateOptions();
            if (validateResult != OptionsValidateResult.OK)
            {
                ShowValidateErrorMessage(validateResult);
                return;
            }

            migrator.Run();
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options => Migrate(options, args));
        }
    }
}
