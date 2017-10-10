using System;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Svn2GitNet.Tests
{
    public static class TestHelper
    {
        public static ILoggerFactory CreateLoggerFactory()
        {
            return new LoggerFactory().AddConsole();
        }

        public static ICommandRunner CreateCommandRunner()
        {
            return new CommandRunner(CreateLoggerFactory().CreateLogger<CommandRunner>(), false);
        }
    }
}