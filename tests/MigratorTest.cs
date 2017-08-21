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
        public void MissingSvnUrlParameterWhenInitializeTest()
        {
            // Prepare
            string[] args = new string[] { };
            Migrator migrator = new Migrator(new Options(), args);
            MigrateResult expected = MigrateResult.MissingSvnUrlParameter;

            // Act
            MigrateResult actual = migrator.Initialize();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
