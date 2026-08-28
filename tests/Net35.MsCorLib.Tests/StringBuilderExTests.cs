// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for the <see cref="StringBuilderEx.Clear"/> extension method.</summary>
    [TestFixture]
    public sealed class StringBuilderExTests
    {
        [Test]
        public void Clear_EmptiesTheBuilder()
        {
            StringBuilder builder = new StringBuilder("some content");

            builder.Clear();

            Assert.AreEqual(0, builder.Length);
            Assert.IsEmpty(builder.ToString());
        }

        [Test]
        public void Clear_ReturnsTheSameBuilderForChaining()
        {
            StringBuilder builder = new StringBuilder("some content");

            Assert.AreSame(builder, builder.Clear());
            Assert.AreEqual("appended", builder.Clear().Append("appended").ToString());
        }

        [Test]
        public void Clear_OnAnAlreadyEmptyBuilder_IsANoOp()
        {
            StringBuilder builder = new StringBuilder();

            Assert.DoesNotThrow(delegate { builder.Clear(); });
            Assert.AreEqual(0, builder.Length);
        }

        [Test]
        public void Clear_KeepsTheBuilderUsable()
        {
            StringBuilder builder = new StringBuilder("first");

            builder.Clear();
            builder.Append("second");

            Assert.AreEqual("second", builder.ToString());
        }

        [Test]
        public void Clear_DoesNotShrinkTheCapacity()
        {
            StringBuilder builder = new StringBuilder(256);
            builder.Append(new string('x', 200));

            int capacityBefore = builder.Capacity;
            builder.Clear();

            Assert.AreEqual(0, builder.Length);
            Assert.AreEqual(capacityBefore, builder.Capacity);
        }

        [Test]
        public void Clear_WorksForALargeBuilder()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                builder.Append("0123456789");
            }

            Assert.AreEqual(10000, builder.Length);

            builder.Clear();

            Assert.AreEqual(0, builder.Length);
        }
    }
}
