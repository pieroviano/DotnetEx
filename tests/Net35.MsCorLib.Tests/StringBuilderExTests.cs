// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Text;

namespace Net35.MsCorLib.Tests
{
    public sealed class StringBuilderExTests
    {
        [Fact]
        public void Clear_EmptiesTheBuilder()
        {
            StringBuilder builder = new StringBuilder("some content");
            StringBuilderEx.Clear(builder);
            Assert.Equal(0, builder.Length);
            Assert.Empty(builder.ToString());
        }

        [Fact]
        public void Clear_ReturnsTheSameBuilderForChaining()
        {
            StringBuilder builder = new StringBuilder("some content");
            Assert.Same(builder, StringBuilderEx.Clear(builder));
            Assert.Equal("appended", StringBuilderEx.Clear(builder).Append("appended").ToString());
        }

        [Fact]
        public void Clear_OnAnAlreadyEmptyBuilder_IsANoOp()
        {
            StringBuilder builder = new StringBuilder();
            StringBuilderEx.Clear(builder);
            Assert.Equal(0, builder.Length);
        }

        [Fact]
        public void Clear_KeepsTheBuilderUsable()
        {
            StringBuilder builder = new StringBuilder("first");
            StringBuilderEx.Clear(builder);
            builder.Append("second");
            Assert.Equal("second", builder.ToString());
        }

        [Fact]
        public void Clear_DoesNotShrinkTheCapacity()
        {
            StringBuilder builder = new StringBuilder(256);
            builder.Append(new string ('x', 200));
            int capacityBefore = builder.Capacity;
            StringBuilderEx.Clear(builder);
            Assert.Equal(0, builder.Length);
            Assert.Equal(capacityBefore, builder.Capacity);
        }

        [Fact]
        public void Clear_WorksForALargeBuilder()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                builder.Append("0123456789");
            }

            Assert.Equal(10000, builder.Length);
            StringBuilderEx.Clear(builder);
            Assert.Equal(0, builder.Length);
        }
    }
}