using System;
using CommandLine;
using CommandLine.Text;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet
{
    class Program
    {
        static void Migrate(Options options, string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();

            ICommandRunner commandRunner = new CommandRunner(loggerFactory.CreateLogger<CommandRunner>(), options.IsVerbose);
            IMessageDisplayer messageDisplayer = new ConsoleMessageDisplayer();

            Migrator migrator = new Migrator(options,
                                             args,
                                             commandRunner,
                                             messageDisplayer,
                                             loggerFactory);
            migrator.Initialize();
            migrator.Run();
        }

        static int Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                              .WithParsed(options => Migrate(options, args));
            }
            catch (MigrateException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Type 'svn2gitnet --help' for more information");

                return -1;
            }

            return 0;
        }
    }
}
