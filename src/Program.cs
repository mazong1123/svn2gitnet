using System;
using CommandLine;
using CommandLine.Text;
using System.IO;

namespace Svn2GitNet
{
    class Program
    {
        static void Migrate(Options options, string[] args)
        {
            Migrator migrator = new Migrator(options, args);
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
