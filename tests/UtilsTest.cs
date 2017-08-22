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
    public class UtilsTest
    {
        [Fact]
        public void EscapeDoubleQuoteTest()
        {
            // Prepare
            string input = "\"haha\"";

            string expected = "\\\"haha\\\"";

            // Act
            string actual = Utils.EscapeQuotes(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EscapeSingleQuoteTest()
        {
            // Prepare
            string input = "'haha'";

            string expected = "\\'haha\\'";

            // Act
            string actual = Utils.EscapeQuotes(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EscapeMultipleDoubleQuotesTest()
        {
            // Prepare
            string input = "\"haha\"hahaha\"ha\"";

            string expected = "\\\"haha\\\"hahaha\\\"ha\\\"";

            // Act
            string actual = Utils.EscapeQuotes(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EscapeMultipleSingleQuotesTest()
        {
            // Prepare
            string input = "'haha''haha'";

            string expected = "\\'haha\\'\\'haha\\'";

            // Act
            string actual = Utils.EscapeQuotes(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EscapeMixedSingleAndDoubleQuotesTest()
        {
            // Prepare
            string input = "'haha''haha\"haha\"";

            string expected = "\\'haha\\'\\'haha\\\"haha\\\"";

            // Act
            string actual = Utils.EscapeQuotes(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveFromTwoEndsOnlyEndTest()
        {
            // Prepare
            string source = "haha'";
            char pattern = '\'';

            string expected = "haha";

            // Act
            string actual = Utils.RemoveFromTwoEnds(source, pattern);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveFromTwoEndsOnlyBeginTest()
        {
            // Prepare
            string source = "'haha";
            char pattern = '\'';

            string expected = "haha";

            // Act
            string actual = Utils.RemoveFromTwoEnds(source, pattern);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveFromTwoEndsBeginEndTest()
        {
            // Prepare
            string source = "'haha'";
            char pattern = '\'';

            string expected = "haha";

            // Act
            string actual = Utils.RemoveFromTwoEnds(source, pattern);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveFromTwoEndsPatternNotFoundTest()
        {
            // Prepare
            string source = "haha";
            char pattern = '\'';

            string expected = "haha";

            // Act
            string actual = Utils.RemoveFromTwoEnds(source, pattern);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
