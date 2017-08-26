using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Linq;
using Xunit;
using Moq;

namespace Svn2GitNet.Tests
{
    public class FixerTest
    {
        private string _testSvnUrl = "svn://testurl";

        [Fact]
        public void FixTrunkRemoteBranchIsNullTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo();

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null);

            // Act
            fixer.FixTrunk();

            // Assert
            mock.Verify(f => f.Run("git", "checkout -f master"), Times.Once());
        }

        [Fact]
        public void FixTrunkRemoteBranchIsWithoutTrunkBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                RemoteBranches = new List<string>()
                {
                    "dev"
                }
            };

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null);

            // Act
            fixer.FixTrunk();

            // Assert
            mock.Verify(f => f.Run("git", "checkout -f master"), Times.Once());
        }

        [Fact]
        public void FixTrunkRemoteBranchHasTrunkBranchIsRebaseTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                RemoteBranches = new List<string>()
                {
                    "trunk"
                }
            };

            Options option = new Options()
            {
                Rebase = true
            };

            IFixer fixer = new Fixer(metaInfo, option, mock.Object, "", null);

            // Act
            fixer.FixTrunk();

            // Assert
            mock.Verify(f => f.Run("git", "checkout -f master"), Times.Once());
        }

        [Fact]
        public void FixTrunkRemoteBranchHasTrunkBranchIsNotRebaseTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                RemoteBranches = new List<string>()
                {
                    "trunk"
                }
            };

            Options option = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, option, mock.Object, "", null);

            // Act
            fixer.FixTrunk();

            // Assert
            mock.Verify(f => f.Run("git", "checkout svn/trunk"), Times.Once());
            mock.Verify(f => f.Run("git", "branch -D master"), Times.Once());
            mock.Verify(f => f.Run("git", "checkout -f -b master"), Times.Once());
            mock.Verify(f => f.Run("git", "checkout -f master"), Times.Never());
        }

        [Fact]
        public void OptimizeReposSuccessTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            IFixer fixer = new Fixer(new MetaInfo(), new Options(), mock.Object, "", null);

            // Act
            fixer.OptimizeRepos();

            // Assert
            mock.Verify(f => f.Run("git", "gc"), Times.Once());
        }

        [Fact]
        public void FixBranchesRebaseFailTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(-1);

            Options options = new Options()
            {
                Rebase = true
            };

            IFixer fixer = new Fixer(new MetaInfo(), options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => fixer.FixBranches());

            // Assert
            mock.Verify(f => f.Run("git", "svn fetch"), Times.Once());
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(string.Format(ExceptionHelper.ExceptionMessage.FAIL_TO_EXECUTE_COMMAND, "git svn fetch"), ex.Message);
        }
    }
}
