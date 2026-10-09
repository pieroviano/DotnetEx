// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;

namespace Net35.MsCorLib.Tests
{
    public sealed class StringExTests
    {
        private sealed class BrokenToString
        {
            public override string ToString()
            {
                return null;
            }
        }

        [Fact]
        public void JoinObjects_ConcatenatesWithTheSeparator()
        {
            Assert.Equal("1, 2, 3", StringEx.Join(", ", new object[] { 1, 2, 3 }));
        }

        [Fact]
        public void JoinObjects_WithASingleValue_OmitsTheSeparator()
        {
            Assert.Equal("only", StringEx.Join(", ", new object[] { "only" }));
        }

        [Fact]
        public void JoinObjects_WithNullSeparator_JoinsWithNothing()
        {
            Assert.Equal("abc", StringEx.Join(null, new object[] { "a", "b", "c" }));
        }

        [Fact]
        public void JoinObjects_WithNoValues_ReturnsEmpty()
        {
            Assert.Empty(StringEx.Join(", ", new object[0]));
        }

        [Fact]
        public void JoinObjects_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                StringEx.Join(", ", (object[])null);
            });
        }

        [Fact]
        public void JoinObjects_SkipsNullEntriesButKeepsTheirSeparator()
        {
            Assert.Equal("a, , c", StringEx.Join(", ", new object[] { "a", null, "c" }));
        }

        [Fact, Trait("Description", "Documents a divergence from string.Join in the BCL, which would return \", b\".")]
        public void JoinObjects_WithNullFirstEntry_ReturnsEmpty()
        {
            // StringEx.Join short-circuits when values[0] is null, unlike the BCL overload.
            Assert.Empty(StringEx.Join(", ", new object[] { null, "b" }));
        }

        [Fact]
        public void JoinObjects_ToleratesAToStringThatReturnsNull()
        {
            Assert.Equal("a, , c", StringEx.Join(", ", new object[] { "a", new BrokenToString(), "c" }));
        }

        [Fact]
        public void JoinGeneric_ConcatenatesWithTheSeparator()
        {
            List<int> values = new List<int>(new int[] { 10, 20, 30 });
            Assert.Equal("10-20-30", StringEx.Join<int>("-", values));
        }

        [Fact]
        public void JoinGeneric_WithAnEmptySequence_ReturnsEmpty()
        {
            Assert.Empty(StringEx.Join<int>("-", new List<int>()));
        }

        [Fact]
        public void JoinGeneric_WithNullSeparator_JoinsWithNothing()
        {
            Assert.Equal("123", StringEx.Join<int>(null, new List<int>(new int[] { 1, 2, 3 })));
        }

        [Fact]
        public void JoinGeneric_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                StringEx.Join<int>("-", (IEnumerable<int>)null);
            });
        }

        [Fact]
        public void JoinGeneric_SkipsNullEntriesButKeepsTheirSeparator()
        {
            List<string> values = new List<string>(new string[] { "a", null, "c" });
            Assert.Equal("a||c", StringEx.Join<string>("|", values));
        }

        [Fact]
        public void JoinGeneric_WithANullFirstEntry_StillEmitsTheRest()
        {
            List<string> values = new List<string>(new string[] { null, "b" });
            Assert.Equal("|b", StringEx.Join<string>("|", values));
        }

        [Fact]
        public void JoinStrings_ConcatenatesWithTheSeparator()
        {
            IEnumerable<string> values = new List<string>(new string[] { "x", "y", "z" });
            Assert.Equal("x/y/z", StringEx.Join("/", values));
        }

        [Fact]
        public void JoinStrings_WithAnEmptySequence_ReturnsEmpty()
        {
            Assert.Empty(StringEx.Join("/", (IEnumerable<string>)new List<string>()));
        }

        [Fact]
        public void JoinStrings_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                StringEx.Join("/", (IEnumerable<string>)null);
            });
        }

        [Fact]
        public void JoinStrings_WithANullFirstEntry_StillEmitsTheRest()
        {
            IEnumerable<string> values = new List<string>(new string[] { null, "b" });
            Assert.Equal("/b", StringEx.Join("/", values));
        }

        [Fact]
        public void Join_LongInputExceedsTheCachedBuilder()
        {
            // Longer than StringBuilderCache's 360-char ceiling, so the builder is not recycled.
            List<string> values = new List<string>();
            for (int i = 0; i < 200; i++)
            {
                values.Add("0123456789");
            }

            string joined = StringEx.Join(",", (IEnumerable<string>)values);
            Assert.Equal((200 * 10) + 199, joined.Length);
        }

        [Fact]
        public void Join_IsReusableAcrossCalls()
        {
            // Exercises the StringBuilderCache acquire/release cycle more than once on the same thread.
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal("a,b", StringEx.Join(",", (IEnumerable<string>)new List<string>(new string[] { "a", "b" })));
            }
        }

        [Fact]
        public void IsNullOrWhiteSpace_NullIsTrue()
        {
            Assert.True(StringEx.IsNullOrWhiteSpace(null));
        }

        [Fact]
        public void IsNullOrWhiteSpace_EmptyIsTrue()
        {
            Assert.True(StringEx.IsNullOrWhiteSpace(string.Empty));
        }

        [Fact]
        public void IsNullOrWhiteSpace_WhitespaceIsTrue()
        {
            Assert.True(StringEx.IsNullOrWhiteSpace(" "));
            Assert.True(StringEx.IsNullOrWhiteSpace("\t\r\n "));
        }

        [Fact]
        public void IsNullOrWhiteSpace_TextIsFalse()
        {
            Assert.False(StringEx.IsNullOrWhiteSpace("a"));
            Assert.False(StringEx.IsNullOrWhiteSpace("  a  "));
            Assert.False(StringEx.IsNullOrWhiteSpace("\t.\t"));
        }
    }
}