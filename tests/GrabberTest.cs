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
    public class GrabberTest
    {
        private string _testSvnUrl = "svn://testurl";

        [Fact]
        public void FetchBranchesOneLocalBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = "*master";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(_testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "master"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().LocalBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchBranchesOneRemoteBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = "origin/master";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(_testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "origin/master"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().RemoteBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchBranchesMultipleLocalBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = $"*master{Environment.NewLine}dev{Environment.NewLine}test";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(_testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "master",
                "dev",
                "test"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().LocalBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchBranchesMultipleRemoteBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = $"origin/master{Environment.NewLine}origin/dev{Environment.NewLine}origin/test";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(_testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "origin/master",
                "origin/dev",
                "origin/test"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().RemoteBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchBranchesHasOneTagTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = $"origin/master{Environment.NewLine}origin/dev{Environment.NewLine}svn/tags/v1.0.0";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(_testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "svn/tags/v1.0.0"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().Tags;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchRebaseBranchesTooManyMatchingLocalBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var localBranchInfo = $"*master{Environment.NewLine}dev{Environment.NewLine}dev";
            var remoteBranchInfo = "dev";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);
            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => grabber.FetchRebaseBraches());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(ExceptionHelper.ExceptionMessage.TOO_MANY_MATCHING_LOCAL_BRANCHES, ex.Message);
        }

        [Fact]
        public void FetchRebaseBranchesNoMatchedLocalBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var localBranchInfo = $"*master{Environment.NewLine}dev1{Environment.NewLine}dev2";
            var remoteBranchInfo = "dev";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);
            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => grabber.FetchRebaseBraches());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(string.Format(ExceptionHelper.ExceptionMessage.NO_LOCAL_BRANCH_FOUND, options.RebaseBranch), ex.Message);
        }

        [Fact]
        public void FetchRebaseBranchesTooManyMatchingRemoteBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();

            var localBranchInfo = "*dev";
            var remoteBranchInfo = $"*master{Environment.NewLine}dev{Environment.NewLine}dev{Environment.NewLine}dev";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);

            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => grabber.FetchRebaseBraches());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(ExceptionHelper.ExceptionMessage.TOO_MANY_MATCHING_REMOTE_BRANCHES, ex.Message);
        }

        [Fact]
        public void FetchRebaseBranchesNoMatchedRemoteBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var localBranchInfo = "*dev";
            var remoteBranchInfo = $"*master{Environment.NewLine}dev1{Environment.NewLine}dev2{Environment.NewLine}dev3";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);

            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => grabber.FetchRebaseBraches());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(string.Format(ExceptionHelper.ExceptionMessage.NO_REMOTE_BRANCH_FOUND, options.RebaseBranch), ex.Message);
        }

        [Fact]
        public void FetchRebaseBranchesSingleMatchingLocalBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var localBranchInfo = $"*master{Environment.NewLine}dev";
            var remoteBranchInfo = "dev";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);
            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            var expected = new List<string>(new string[]
            {
                "dev"
            });

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.FetchRebaseBraches();
            var actual = grabber.GetMetaInfo().LocalBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FetchRebaseBranchesSingleMatchingRemoteBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();

            var localBranchInfo = "*dev";
            var remoteBranchInfo = $"*master{Environment.NewLine}dev";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out localBranchInfo))
                .Returns(0);

            mock.Setup(f => f.Run("git", "branch -r --no-color", out remoteBranchInfo))
                .Returns(0);

            var options = new Options()
            {
                RebaseBranch = "dev"
            };

            var expected = new List<string>(new string[]
            {
                "dev"
            });

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.FetchRebaseBraches();
            var actual = grabber.GetMetaInfo().LocalBranches;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CloneWhenRootIsTrunkWithAllParametersTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                UserName = "userName",
                Password = "password",
                IncludeMetaData = false,
                NoMinimizeUrl = true,
                RootIsTrunk = true
            };

            string expectedArguments = $"svn init --prefix=svn/ --username=\"userName\" --no-metadata --no-minimize-url --trunk=\"{_testSvnUrl}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsTrunkWithoutUserNameAndPasswordTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = false,
                NoMinimizeUrl = true,
                RootIsTrunk = true
            };

            string expectedArguments = $"svn init --prefix=svn/ --no-metadata --no-minimize-url --trunk=\"{_testSvnUrl}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsTrunkHasMetaDataTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = true,
                NoMinimizeUrl = true,
                RootIsTrunk = true
            };

            string expectedArguments = $"svn init --prefix=svn/ --no-minimize-url --trunk=\"{_testSvnUrl}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsTrunkHasMinimizeUrlTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = true,
                NoMinimizeUrl = true,
                RootIsTrunk = true
            };

            string expectedArguments = $"svn init --prefix=svn/ --no-minimize-url --trunk=\"{_testSvnUrl}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkWithoutBranchesAndTagsTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false
            };

            string expectedArguments = $"svn init --prefix=svn/ {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false
            };

            string expectedArguments = $"svn init --prefix=svn/ --trunk=\"subpath\" {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkAndTagsTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = false,
                RootIsTrunk = false,
                Tags = new List<string>()
                {
                    "tag1",
                    "tag2"
                }
            };

            string expectedArguments = $"svn init --prefix=svn/ --trunk=\"subpath\" --tags=\"tag1\" --tags=\"tag2\" {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkAndDefaultTagTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = false,
                RootIsTrunk = false
            };

            string expectedArguments = $"svn init --prefix=svn/ --trunk=\"subpath\" --tags=\"tags\" {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkAndDefaultBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = false,
                NoTags = true,
                RootIsTrunk = false
            };

            string expectedArguments = $"svn init --prefix=svn/ --trunk=\"subpath\" --branches=\"branches\" {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkAndBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = false,
                NoTags = true,
                RootIsTrunk = false,
                Branches = new List<string>()
                {
                    "branch1",
                    "branch2"
                }
            };

            string expectedArguments = $"svn init --prefix=svn/ --trunk=\"subpath\" --branches=\"branch1\" --branches=\"branch2\" {_testSvnUrl}";

            mock.Setup(f => f.Run("git", It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.RunGitSvnInitCommand(expectedArguments, options.Password), Times.Once());
        }

        [Fact]
        public void CloneWhenRootIsNotTrunkHasSubPathToTrunkGitCommandExecutionFailTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false,
            };

            string expectedExceptionMessage = string.Format(ExceptionHelper.ExceptionMessage.FAIL_TO_EXECUTE_COMMAND, $"git svn init --prefix=svn/ --trunk=\"subpath\" {_testSvnUrl}");

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(-1);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            Exception ex = Record.Exception(() => grabber.Clone());

            // Assert
            Assert.IsType<MigrateException>(ex);
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void CloneWhenAuthorsIsNotEmptyTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false,
                Authors = "author1"
            };

            string expectedArguments = "config svn.authorsfile author1";

            mock.Setup(f => f.RunGitSvnInitCommand(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0);

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "config", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenRevisionIsNotEmptyTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false,
                Revision = "123:456"
            };

            string expectedArguments = "svn fetch -r 123:456";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenExcludeIsNotEmptyRootIsTrunkTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = true,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                }
            };

            string ignorePathsRegEx = @"^(?:)(?:ex1|ex2)";
            string expectedArguments = $"svn fetch --ignore-paths=\"{ignorePathsRegEx}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenExcludeIsNotEmptyRevisionIsNotEmptyRootIsTrunkTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = true,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                },
                Revision = "123:456"
            };

            string ignorePathsRegEx = @"^(?:)(?:ex1|ex2)";
            string expectedArguments = $"svn fetch -r 123:456 --ignore-paths=\"{ignorePathsRegEx}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenExcludeIsNotEmptyRootIsNotTrunkTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = true,
                RootIsTrunk = false,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                }
            };

            string ignorePathsRegEx = @"^(?:subpath[\/])(?:ex1|ex2)";
            string expectedArguments = $"svn fetch --ignore-paths=\"{ignorePathsRegEx}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenExcludeIsNotEmptyRootIsNotTrunkHasTagsTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = true,
                NoTags = false,
                RootIsTrunk = false,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                },
                Tags = new List<string>()
                {
                    "tag1",
                    "tag2"
                }
            };

            string ignorePathsRegEx = @"^(?:subpath[\/]|tag1[\/][^\/]+[\/]|tag2[\/][^\/]+[\/])(?:ex1|ex2)";
            string expectedArguments = $"svn fetch --ignore-paths=\"{ignorePathsRegEx}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void CloneWhenExcludeIsNotEmptyRootIsNotTrunkHasTagsHasBranchesTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = false,
                NoTags = false,
                RootIsTrunk = false,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                },
                Tags = new List<string>()
                {
                    "tag1",
                    "tag2"
                },
                Branches = new List<string>()
                {
                    "branch1",
                    "branch2"
                }
            };

            string ignorePathsRegEx = @"^(?:subpath[\/]|tag1[\/][^\/]+[\/]|tag2[\/][^\/]+[\/]|branch1[\/][^\/]+[\/]|branch2[\/][^\/]+[\/])(?:ex1|ex2)";
            string expectedArguments = $"svn fetch --ignore-paths=\"{ignorePathsRegEx}\"";

            mock.Setup(f => f.Run("git", It.IsAny<string>())).Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.Clone();

            // Assert
            mock.Verify(f => f.Run("git", expectedArguments), Times.Once());
        }

        [Fact]
        public void GetMetaInfoTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            Options options = new Options()
            {
                SubpathToTrunk = "subpath",
                IncludeMetaData = true,
                NoBranches = false,
                NoTags = false,
                RootIsTrunk = false,
                Exclude = new List<string>()
                {
                    "ex1",
                    "ex2"
                },
                Tags = new List<string>()
                {
                    "tag1",
                    "tag2"
                },
                Branches = new List<string>()
                {
                    "branch1",
                    "branch2"
                }
            };

            var standardOutput = "*branch1";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out standardOutput))
                .Returns(0);

            standardOutput = "svn/tags/branch2";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);

            IGrabber grabber = new Grabber(_testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.FetchBranches();
            var actual = grabber.GetMetaInfo();

            // Assert
            Assert.Equal(new List<string>() { "svn/tags/branch2" }, actual.Tags);
            Assert.Equal(new List<string>() { "branch1" }, actual.LocalBranches);
            Assert.Equal(new List<string>() { "svn/tags/branch2" }, actual.RemoteBranches);
        }
    }
}
