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
            var standardOutput = "*master";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "master"
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
            var standardOutput = $"*master{Environment.NewLine}dev{Environment.NewLine}test";
            mock.Setup(f => f.Run("git", "branch -r --no-color", out standardOutput))
                .Returns(0);
            IGrabber grabber = new Grabber(testSvnUrl, new Options(), mock.Object, "", null);

            List<string> expected = new List<string>(new string[]{
                "master",
                "dev",
                "test"
            });

            // Act
            grabber.FetchBranches();

            var actual = grabber.GetMetaInfo().RemoteBranches;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
