using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Xunit;

namespace Svn2GitNet.Tests
{
    public class End2EndTest
    {
        private const string PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL = "https://svn.code.sf.net/p/svn2gitnetclassicstructure/code";

        [Fact]
        public void PrivateRepositoryEnd2EndTest()
        {
            string svnUrl = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_URL");
            string userName = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_USER_NAME");
            string password = Environment.GetEnvironmentVariable("SVN2GITNET_PRIVATE_REPO_PASSWORD");

            int exitCode = RunSvn2GitNet($"{svnUrl} --username {userName} --password {password} -v", "PrivateRepoSmokeTest");
            
            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void PublicClassicLayoutRepositoryEnd2EndSmokeTest()
        {
            int exitCode = RunSvn2GitNet($"{PUBLIC_CLASSIC_LAYOUT_REPOSITORY_URL} -v", "PublicRepoSmokeTest");

            Assert.Equal(0, exitCode);
        }

        private int RunSvn2GitNet(string arguments, string subWorkingFolder = "")
        {
            string platformSepcifier = GetPlatformSpecifier();
            string testTempFolderPath = GetIntegrationTestsTempFolderPath();
            string binaryName = GetBinaryName();
            string binaryPath = Path.Combine(testTempFolderPath, "netcoreapp2.0", platformSepcifier, "publish", binaryName);

            string workingdirectory = string.IsNullOrEmpty(subWorkingFolder) ? testTempFolderPath : Path.Combine(testTempFolderPath, subWorkingFolder);
            if (!Directory.Exists(workingdirectory))
            {
                Directory.CreateDirectory(workingdirectory);
            }

            Process commandProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = binaryPath,
                    Arguments = arguments,
                    WorkingDirectory = workingdirectory
                }
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
