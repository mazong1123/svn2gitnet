using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Xunit;

namespace Svn2GitNet.Tests
{
    public class MigratorTest
    {
        [Fact]
        public void EmptyParameterInitializeTest()
        {
            // Prepare
            string[] args = new string[] { };
            Migrator migrator = new Migrator(new Options(), 
                                            args, 
                                            TestHelper.CreateCommandRunner(), 
                                            new ConsoleMessageDisplayer(), 
                                            TestHelper.CreateLoggerFactory());

            // Act
            Exception ex = Record.Exception(() => migrator.Initialize());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(ExceptionHelper.ExceptionMessage.MISSING_SVN_URL_PARAMETER, ex.Message);
        }

        [Fact]
        public void RebaseBranchWithExtraArgumentsInitializeTest()
        {
            // Prepare
            string[] args = new string[] { "--rebasebranch", "normalarg", "extraarg" };
            Migrator migrator = new Migrator(new Options()
            {
                RebaseBranch = "normalarg"
            },
            args,
            TestHelper.CreateCommandRunner(),
            new ConsoleMessageDisplayer(),
            TestHelper.CreateLoggerFactory());

            // Act
            Exception ex = Record.Exception(() => migrator.Initialize());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(ExceptionHelper.ExceptionMessage.TOO_MANY_ARGUMENTS, ex.Message);
        }

        [Fact]
        public void SvnUrlWithUserNameAndPasswordInitializeTest()
        {
            // Prepare
            string[] args = new string[] { "http://testsvnurl", "--username=\"userName\"", "--password=\"123123\"" };
            Migrator migrator = new Migrator(new Options()
            {
                UserName = "userName",
                Password = "123123"
            },
            args,
            TestHelper.CreateCommandRunner(),
            new ConsoleMessageDisplayer(),
            TestHelper.CreateLoggerFactory());

            // Act
            Exception ex = Record.Exception(() => migrator.Initialize());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void SvnUrlInitializeSuccessTest()
        {
            // Prepare
            string[] args = new string[] { "http://testsvnurl" };
            Migrator migrator = new Migrator(new Options(),
            args,
            TestHelper.CreateCommandRunner(),
            new ConsoleMessageDisplayer(),
            TestHelper.CreateLoggerFactory());

            // Act
            Exception ex = Record.Exception(() => migrator.Initialize());

            // Assert
            Assert.Null(ex);
        }
    }
}
