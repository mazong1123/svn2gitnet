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
        #region FixTrunk Tests

        [Fact]
        public void FixTrunkRemoteBranchIsNullTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo();

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null, null);

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

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null, null);

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

            IFixer fixer = new Fixer(metaInfo, option, mock.Object, "", null, null);

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

            IFixer fixer = new Fixer(metaInfo, option, mock.Object, "", null, null);

            // Act
            fixer.FixTrunk();

            // Assert
            mock.Verify(f => f.Run("git", "checkout svn/trunk"), Times.Once());
            mock.Verify(f => f.Run("git", "branch -D master"), Times.Once());
            mock.Verify(f => f.Run("git", "checkout -f -b master"), Times.Once());
            mock.Verify(f => f.Run("git", "checkout -f master"), Times.Never());
        }

        #endregion

        #region OptimizeRepos Tests

        [Fact]
        public void OptimizeReposSuccessTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            IFixer fixer = new Fixer(new MetaInfo(), new Options(), mock.Object, "", null, null);

            // Act
            fixer.OptimizeRepos();

            // Assert
            mock.Verify(f => f.Run("git", "gc"), Times.Once());
        }

        #endregion

        #region FixBranches Tests

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

            IFixer fixer = new Fixer(new MetaInfo(), options, mock.Object, "", null, null);

            // Act
            Exception ex = Record.Exception(() => fixer.FixBranches());

            // Assert
            mock.Verify(f => f.Run("git", "svn fetch"), Times.Once());
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(string.Format(ExceptionHelper.ExceptionMessage.FAIL_TO_EXECUTE_COMMAND, "git svn fetch"), ex.Message);
        }

        [Fact]
        public void FixBranchesRebaseHasRemoteTrunkBranchSuccessTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "trunk1"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/trunk",
                    "ddd"
                }
            };

            Options options = new Options()
            {
                Rebase = true
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "svn fetch"), Times.Once());
            mock.Verify(f => f.Run("git", "checkout -f \"master\""), Times.Once());
            mock.Verify(f => f.Run("git", "rebase \"remotes/svn/trunk\""), Times.Once());
        }

        [Fact]
        public void FixBranchesIsNotRebaseHasRemoteTrunkBranchSuccessTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "trunk1"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/trunk",
                    "ddd"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "svn fetch"), Times.Never());
            mock.Verify(f => f.Run("git", "checkout -f \"master\""), Times.Never());
            mock.Verify(f => f.Run("git", "rebase \"remotes/svn/trunk\""), Times.Never());
        }

        [Fact]
        public void FixBranchesIsNotRebaseIsTrunkBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "trunk1"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/trunk",
                    "ddd"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "checkout \"trunk\""), Times.Never());
        }

        [Fact]
        public void FixBranchesIsNotRebaseIsLocalBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "trunk1"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/trunk1",
                    "ddd"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "checkout \"trunk\""), Times.Never());
        }

        [Fact]
        public void FixBranchesIsNotRebaseIsNotTrunkBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = string.Empty;
            string standardError = "Hello. Cannot setup tracking information!";
            mock.Setup(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError));


            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "nodev"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/dev"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "checkout -b \"dev\" \"remotes/svn/dev\""), Times.Once());
            mock.Verify(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError), Times.Once());
        }

        [Fact]
        public void FixBranchesIsNotRebaseIsNotTrunkBranchTrackingWarningTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = string.Empty;
            string standardError = "Hello. Cannot setup tracking information!";
            mock.Setup(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError));

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "nodev"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/dev",
                    "svn/branch2"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "checkout -b \"dev\" \"remotes/svn/dev\""), Times.Once());
            mock.Verify(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError), Times.Once());
        }

        [Fact]
        public void FixBranchesIsNotRebaseIsNotTrunkBranchTrackingNoWarningTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = string.Empty;
            string standardError = string.Empty;
            mock.Setup(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError));

            MetaInfo metaInfo = new MetaInfo()
            {
                LocalBranches = new List<string>()
                {
                    "nodev"
                },
                RemoteBranches = new List<string>()
                {
                    "svn/dev",
                    "svn/branch2"
                }
            };

            Options options = new Options()
            {
                Rebase = false
            };

            IFixer fixer = new Fixer(metaInfo, options, mock.Object, "", null, null);

            // Act
            fixer.FixBranches();

            // Assert
            mock.Verify(f => f.Run("git", "checkout \"dev\""), Times.Once());
            mock.Verify(f => f.Run("git", "branch --track \"dev\" \"remotes/svn/dev\"", out standardOutput, out standardError), Times.Once());
        }

        #endregion

        #region FixTags Tests

        [Fact]
        public void FixTagsWhenTagsIsNullTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            MetaInfo metaInfo = new MetaInfo()
            {
                Tags = null
            };

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null, null);

            // Act
            fixer.FixTags();

            // Assert
            mock.Verify(f => f.Run("git", It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void FixTagsHasOneTagTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = "subject1";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%s' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " date1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ci' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " author1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%an' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " test@email.com ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ae' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                Tags = new List<string>()
                {
                    "svn/tags/tag1"
                }
            };

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "", null, null);

            // Act
            fixer.FixTags();

            // Assert
            mock.Verify(f => f.Run("git", $"tag -a -m \"subject1\" \"tag1\" \"svn/tags/tag1\""), Times.Once());
            mock.Verify(f => f.Run("git", $"branch -d -r \"svn/tags/tag1\""), Times.Once());
        }

        [Fact]
        public void FixTagsHasOneTagRestoreTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = "subject1";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%s' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " date1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ci' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " author1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%an' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " test@email.com ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ae' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            string currentUserName = "userName1";
            mock.Setup(f => f.Run("git", "config --get user.name", out currentUserName))
                .Returns(0);

            string currentUserEmail = "userEmail1@email.com";
            mock.Setup(f => f.Run("git", "config --get user.email", out currentUserEmail))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                Tags = new List<string>()
                {
                    "svn/tags/tag1"
                }
            };

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "config", null, null);

            // Act
            fixer.FixTags();

            // Assert
            mock.Verify(f => f.Run("git", "config user.name \"userName1\""), Times.Once());
            mock.Verify(f => f.Run("git", "config user.email \"userEmail1@email.com\""), Times.Once());
        }

        [Fact]
        public void FixTagsHasOneTagUnsetTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            string standardOutput = "subject1";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%s' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " date1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ci' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " author1 ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%an' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            standardOutput = " test@email.com ";
            mock.Setup(f => f.Run("git", "log -1 --pretty=format:'%ae' \"svn/tags/tag1\"", out standardOutput))
                .Returns(0);

            string currentUserName = string.Empty;
            mock.Setup(f => f.Run("git", "config --get user.name", out currentUserName))
                .Returns(0);

            string currentUserEmail = string.Empty;
            mock.Setup(f => f.Run("git", "config --get user.email", out currentUserEmail))
                .Returns(0);

            MetaInfo metaInfo = new MetaInfo()
            {
                Tags = new List<string>()
                {
                    "svn/tags/tag1"
                }
            };

            IFixer fixer = new Fixer(metaInfo, new Options(), mock.Object, "config", null, null);

            // Act
            fixer.FixTags();

            // Assert
            mock.Verify(f => f.Run("git", "config --unset user.name"), Times.Once());
            mock.Verify(f => f.Run("git", "config --unset user.email"), Times.Once());
        }

        #endregion
    }
}
