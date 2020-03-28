using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Svn2GitNet.Tests
{
    public class End2EndTest
    {
        private const string PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL = "https://svn.code.sf.net/p/svn2gitnetclassicstructure/code";
        private const string PUBLIC_NON_STANDARD_LAYOUT_REPOSITORY_URL = "https://svn.code.sf.net/p/svn2gitnetnonstandard/code";
        private const string PUBLIC_NON_STANDARD_LAYOUT_NO_BRANCH_NO_TAG_REPOSITORY_URL = "https://svn.code.sf.net/p/svn2gitnetnonstandardsole/code";

        [Fact]
        public void PrivateRepositoryEnd2EndTest()
        {
            // Fix me: while travis-ci cannot wait for stdin of git svn, we do not
            // have a chance to pipe our password in so the test always failed.
            // So we just skip this test for travis-ci now.
            string isTravisCI = Environment.GetEnvironmentVariable("IS_TRAVIS_CI");
            if (!string.IsNullOrWhiteSpace(isTravisCI) && isTravisCI.Equals("1"))
            {
                Assert.True(true, "Skipped this test in Travis-CI!");

                return;
            }

            string svnUrl = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_URL");
            string userName = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_USER_NAME");
            string password = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_PASSWORD");

            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{svnUrl} --username {userName} --password {password} -v", "PrivateRepoSmokeTest"));

            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void PublicClassicLayoutRepositoryEnd2EndSmokeTest()
        {
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL} -v", "PublicRepoSmokeTest"));

            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void PublicClassicLayoutRepositoryEnd2EndBranchTest()
        {
            string subWorkingFolder = "PublicRepoBranchTest";
            string expectedBranchInfo = "  dev  dev@1* master";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL} -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            ICommandRunner commandRunner = TestHelper.CreateCommandRunner();

            string actualBranchInfo = string.Empty;
            string dummyError = string.Empty;
            commandRunner.Run("git", "branch", out actualBranchInfo, out dummyError, Path.Combine(GetIntegrationTestsTempFolderPath(), subWorkingFolder));

            Assert.Equal(0, exitCode);
            Assert.Equal(expectedBranchInfo, actualBranchInfo);
        }

        [Fact]
        public void PublicClassicLayoutRepositoryEnd2EndTagTest()
        {
            string subWorkingFolder = "PublicRepoTagTest";
            string expectedTagInfo = "1.0.01.0.0@2";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL} -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            ICommandRunner commandRunner = TestHelper.CreateCommandRunner();

            string actualTagInfo = string.Empty;
            string dummyError = string.Empty;
            exitCode = commandRunner.Run("git", "tag", out actualTagInfo, out dummyError, Path.Combine(GetIntegrationTestsTempFolderPath(), subWorkingFolder));

            Assert.Equal(0, exitCode);
            Assert.Equal(expectedTagInfo, actualTagInfo);
        }

        [Fact]
        public void PublicNonstandardLayoutRepositoryEnd2EndSmokeTest()
        {
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_NON_STANDARD_LAYOUT_REPOSITORY_URL} --rootistrunk -v", "PublicNonStandardRepoSmokeTest"));

            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void PublicNonstandardLayoutRepositoryEnd2EndNoBranchNoTagTest()
        {
            string subWorkingFolder = "PublicNonStandardRepoNoBranchNoTagTest";
            string expectedBranchInfo = "* master";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_NON_STANDARD_LAYOUT_NO_BRANCH_NO_TAG_REPOSITORY_URL} --rootistrunk -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            ICommandRunner commandRunner = TestHelper.CreateCommandRunner();

            string actualBranchInfo = string.Empty;
            string dummyError = string.Empty;
            commandRunner.Run("git", "branch", out actualBranchInfo, out dummyError, Path.Combine(GetIntegrationTestsTempFolderPath(), subWorkingFolder));

            Assert.Equal(0, exitCode);
            Assert.Equal(expectedBranchInfo, actualBranchInfo);
        }

        [Fact]
        public void PublicNonstandardLayoutRepositoryEnd2EndBranchTest()
        {
            string subWorkingFolder = "PublicNonStandardRepoBranchTest";
            string expectedBranchInfo = "  1.0.0  1.0.0@1  1.0.0@3  br1  br1@17  br1@3* master";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_NON_STANDARD_LAYOUT_REPOSITORY_URL} --trunk main --branches dev --tags rel -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            ICommandRunner commandRunner = TestHelper.CreateCommandRunner();

            string actualBranchInfo = string.Empty;
            string dummyError = string.Empty;
            commandRunner.Run("git", "branch", out actualBranchInfo, out dummyError, Path.Combine(GetIntegrationTestsTempFolderPath(), subWorkingFolder));

            Assert.Equal(0, exitCode);
            Assert.Equal(expectedBranchInfo, actualBranchInfo);
        }

        [Fact]
        public void PublicNonstandardLayoutRepositoryEnd2EndTagTest()
        {
            string subWorkingFolder = "PublicNonStandardRepoTagTest";
            string expectedBranchInfo = "1.0.01.0.0@3";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_NON_STANDARD_LAYOUT_REPOSITORY_URL} --trunk main --branches dev --tags rel -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            ICommandRunner commandRunner = TestHelper.CreateCommandRunner();

            string actualBranchInfo = string.Empty;
            string dummyError = string.Empty;
            commandRunner.Run("git", "tag", out actualBranchInfo, out dummyError, Path.Combine(GetIntegrationTestsTempFolderPath(), subWorkingFolder));

            Assert.Equal(0, exitCode);
            Assert.Equal(expectedBranchInfo, actualBranchInfo);
        }

        [Fact]
        public void PublicClassicLayoutRepositoryEnd2EndRebaseTest()
        {
            string subWorkingFolder = "PublicRepoRebaseTest";
            int exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo($"{PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL} -v", subWorkingFolder));

            Assert.Equal(0, exitCode);

            exitCode = RunCommand(BuildSvn2GitNetProcessStartInfo("--rebase -v", subWorkingFolder));

            Assert.Equal(0, exitCode);
        }

        private ProcessStartInfo BuildSvn2GitNetProcessStartInfo(string arguments, string subWorkingFolder = "")
        {
            string platformSepcifier = GetPlatformSpecifier();
            string testTempFolderPath = GetIntegrationTestsTempFolderPath();
            string binaryName = GetBinaryName();
            string binaryPath = Path.Combine(testTempFolderPath, "netcoreapp3.1", platformSepcifier, "publish", binaryName);

            string workingdirectory = string.IsNullOrEmpty(subWorkingFolder) ? testTempFolderPath : Path.Combine(testTempFolderPath, subWorkingFolder);
            if (!Directory.Exists(workingdirectory))
            {
                Directory.CreateDirectory(workingdirectory);
            }

            return new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = binaryPath,
                Arguments = arguments,
                WorkingDirectory = workingdirectory
            };
        }

        private int RunCommand(ProcessStartInfo startInfo)
        {
            Process commandProcess = new Process()
            {
                StartInfo = startInfo
            };

            commandProcess.Start();

            // Timeout = 2 min.
            commandProcess.WaitForExit(120000);

            if (!commandProcess.HasExited)
            {
                commandProcess.Kill();
                commandProcess.WaitForExit();
            }

            int exitCode = commandProcess.ExitCode;
            commandProcess.Close();

            return exitCode;
        }

        private string GetIntegrationTestsTempFolderPath()
        {
            return Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName, "integrationtests");
        }

        private string GetBinaryName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "svn2gitnet.exe" : "svn2gitnet";
        }

        private string GetPlatformSpecifier()
        {
            var osNameAndVersion = RuntimeInformation.OSDescription;
            var platformArch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
            string platformSpecifier = string.Empty;
            if (osNameAndVersion.Contains("Ubuntu"))
            {
                platformSpecifier = $"ubuntu.16.04-{platformArch}";
            }
            else
            {
                platformSpecifier = $"win10-{platformArch}";
            }

            // TODO: We need to support more platforms.

            return platformSpecifier;
        }
    }
}
