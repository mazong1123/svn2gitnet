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
        private string testSvnUrl = "svn://testurl";

        [Fact]
        public void FetchBranchesOneLocalBranchTest()
        {
            // Prepare
            var mock = new Mock<ICommandRunner>();
            var standardOutput = "*master";
            mock.Setup(f => f.Run("git", "branch -l --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

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
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

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
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

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
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

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
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

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

            IGrabber grabber = new Grabber(testSvnUrl, options, mock.Object, "", null);

            // Act
            grabber.FetchRebaseBraches();
            var actual = grabber.GetMetaInfo().LocalBranches;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
