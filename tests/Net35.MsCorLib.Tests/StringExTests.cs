// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="StringEx"/> (String.Join overloads and IsNullOrWhiteSpace).</summary>
    [TestFixture]
    public sealed class StringExTests
    {
        private sealed class BrokenToString
        {
            public override string ToString()
            {
                return null;
            }
        }

        [Test]
        public void JoinObjects_ConcatenatesWithTheSeparator()
        {
            Assert.AreEqual("1, 2, 3", StringEx.Join(", ", new object[] { 1, 2, 3 }));
        }

        [Test]
        public void JoinObjects_WithASingleValue_OmitsTheSeparator()
        {
            Assert.AreEqual("only", StringEx.Join(", ", new object[] { "only" }));
        }

        [Test]
        public void JoinObjects_WithNullSeparator_JoinsWithNothing()
        {
            Assert.AreEqual("abc", StringEx.Join(null, new object[] { "a", "b", "c" }));
        }

        [Test]
        public void JoinObjects_WithNoValues_ReturnsEmpty()
        {
            Assert.IsEmpty(StringEx.Join(", ", new object[0]));
        }

        [Test]
        public void JoinObjects_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { StringEx.Join(", ", (object[])null); });
        }

        [Test]
        public void JoinObjects_SkipsNullEntriesButKeepsTheirSeparator()
        {
            Assert.AreEqual("a, , c", StringEx.Join(", ", new object[] { "a", null, "c" }));
        }

        [Test, Description("Documents a divergence from string.Join in the BCL, which would return \", b\".")]
        public void JoinObjects_WithNullFirstEntry_ReturnsEmpty()
        {
            // StringEx.Join short-circuits when values[0] is null, unlike the BCL overload.
            Assert.IsEmpty(StringEx.Join(", ", new object[] { null, "b" }));
        }

        [Test]
        public void JoinObjects_ToleratesAToStringThatReturnsNull()
        {
            Assert.AreEqual("a, , c", StringEx.Join(", ", new object[] { "a", new BrokenToString(), "c" }));
        }

        [Test]
        public void JoinGeneric_ConcatenatesWithTheSeparator()
        {
            List<int> values = new List<int>(new int[] { 10, 20, 30 });

            Assert.AreEqual("10-20-30", StringEx.Join<int>("-", values));
        }

        [Test]
        public void JoinGeneric_WithAnEmptySequence_ReturnsEmpty()
        {
            Assert.IsEmpty(StringEx.Join<int>("-", new List<int>()));
        }

        [Test]
        public void JoinGeneric_WithNullSeparator_JoinsWithNothing()
        {
            Assert.AreEqual("123", StringEx.Join<int>(null, new List<int>(new int[] { 1, 2, 3 })));
        }

        [Test]
        public void JoinGeneric_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { StringEx.Join<int>("-", (IEnumerable<int>)null); });
        }

        [Test]
        public void JoinGeneric_SkipsNullEntriesButKeepsTheirSeparator()
        {
            List<string> values = new List<string>(new string[] { "a", null, "c" });

            Assert.AreEqual("a||c", StringEx.Join<string>("|", values));
        }

        [Test]
        public void JoinGeneric_WithANullFirstEntry_StillEmitsTheRest()
        {
            List<string> values = new List<string>(new string[] { null, "b" });

            // Unlike the object[] overload, the generic one does not short-circuit on a null head.
            Assert.AreEqual("|b", StringEx.Join<string>("|", values));
        }

        [Test]
        public void JoinStrings_ConcatenatesWithTheSeparator()
        {
            IEnumerable<string> values = new List<string>(new string[] { "x", "y", "z" });

            Assert.AreEqual("x/y/z", StringEx.Join("/", values));
        }

        [Test]
        public void JoinStrings_WithAnEmptySequence_ReturnsEmpty()
        {
            Assert.IsEmpty(StringEx.Join("/", (IEnumerable<string>)new List<string>()));
        }

        [Test]
        public void JoinStrings_WithNullValues_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { StringEx.Join("/", (IEnumerable<string>)null); });
        }

        [Test]
        public void JoinStrings_WithANullFirstEntry_StillEmitsTheRest()
        {
            IEnumerable<string> values = new List<string>(new string[] { null, "b" });

            Assert.AreEqual("/b", StringEx.Join("/", values));
        }

        [Test]
        public void Join_LongInputExceedsTheCachedBuilder()
        {
            // Longer than StringBuilderCache's 360-char ceiling, so the builder is not recycled.
            List<string> values = new List<string>();
            for (int i = 0; i < 200; i++)
            {
                values.Add("0123456789");
            }

            string joined = StringEx.Join(",", (IEnumerable<string>)values);

            Assert.AreEqual((200 * 10) + 199, joined.Length);
        }

        [Test]
        public void Join_IsReusableAcrossCalls()
        {
            // Exercises the StringBuilderCache acquire/release cycle more than once on the same thread.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual("a,b", StringEx.Join(",", (IEnumerable<string>)new List<string>(new string[] { "a", "b" })));
            }
        }

        [Test]
        public void IsNullOrWhiteSpace_NullIsTrue()
        {
            Assert.IsTrue(StringEx.IsNullOrWhiteSpace(null));
        }

        [Test]
        public void IsNullOrWhiteSpace_EmptyIsTrue()
        {
            Assert.IsTrue(StringEx.IsNullOrWhiteSpace(string.Empty));
        }

        [Test]
        public void IsNullOrWhiteSpace_WhitespaceIsTrue()
        {
            Assert.IsTrue(StringEx.IsNullOrWhiteSpace(" "));
            Assert.IsTrue(StringEx.IsNullOrWhiteSpace("\t\r\n "));
        }

        [Test]
        public void IsNullOrWhiteSpace_TextIsFalse()
        {
            Assert.IsFalse(StringEx.IsNullOrWhiteSpace("a"));
            Assert.IsFalse(StringEx.IsNullOrWhiteSpace("  a  "));
            Assert.IsFalse(StringEx.IsNullOrWhiteSpace("\t.\t"));
        }
    }
}
