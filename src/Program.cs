using System;
using CommandLine;

namespace Svn2GitNet
{
    class Program
    {
        static void ShowMigrateErrorMessage(MigrateResult migrateResult)
        {
            switch (migrateResult)
            {
                case MigrateResult.MissingSvnUrlParameter:
                    Console.WriteLine("Missing SVN_URL parameter.");
                    break;
                case MigrateResult.TooManyArguments:
                    Console.WriteLine("Too many arguments.");
                    break;
                case MigrateResult.WorkingTreeIsNotClean:
                    Console.WriteLine("You have local pending changes.  The working tree must be clean in order to continue.");
                    break;
                case MigrateResult.FailToExecuteCommand:
                    Console.WriteLine("Fail to execute command. Run with -v or --verbose for details.");
                    break;
                default:
                    break;
            }
        }

        static void Migrate(Options options, string[] args)
        {
            Migrator migrator = new Migrator(options, args);
            MigrateResult migrateResult = migrator.Initialize();
            if (migrateResult != MigrateResult.OK)
            {
                ShowMigrateErrorMessage(migrateResult);
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
