// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;

namespace Net35.Buffers.Tests
{
    /// <summary>
    /// Tests for <see cref="ArrayPool{T}"/> and its default bucketing implementation.
    /// Bucket sizes are powers of two starting at 16 (see Utilities.SelectBucketIndex).
    /// </summary>
    [TestFixture]
    public sealed class ArrayPoolTests
    {
        [Test]
        public void Shared_ReturnsSameInstanceEveryTime()
        {
            ArrayPool<byte> first = ArrayPool<byte>.Shared;
            ArrayPool<byte> second = ArrayPool<byte>.Shared;

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
        }

        [Test]
        public void Shared_IsSeparatePerElementType()
        {
            Assert.AreNotSame(ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
        }

        [Test]
        public void Create_ReturnsANewPoolEachCall()
        {
            ArrayPool<byte> first = ArrayPool<byte>.Create();
            ArrayPool<byte> second = ArrayPool<byte>.Create();

            Assert.IsNotNull(first);
            Assert.AreNotSame(first, second);
        }

        [Test]
        public void Create_WithNonPositiveMaxArrayLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ArrayPool<byte>.Create(0, 4); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ArrayPool<byte>.Create(-1, 4); });
        }

        [Test]
        public void Create_WithNonPositiveMaxArraysPerBucket_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ArrayPool<byte>.Create(1024, 0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ArrayPool<byte>.Create(1024, -1); });
        }

        [Test]
        public void Rent_NegativeLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ArrayPool<byte>.Create().Rent(-1); });
        }

        [Test]
        public void Rent_ZeroLength_ReturnsSharedEmptyArray()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            byte[] first = pool.Rent(0);
            byte[] second = pool.Rent(0);

            Assert.IsNotNull(first);
            Assert.AreEqual(0, first.Length);
            Assert.AreSame(first, second);
        }

        [Test]
        public void Rent_RoundsUpToTheBucketSize()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            // The smallest bucket is 16; sizes round up to the next power of two.
            Assert.AreEqual(16, pool.Rent(1).Length, "Rent(1)");
            Assert.AreEqual(16, pool.Rent(15).Length, "Rent(15)");
            Assert.AreEqual(16, pool.Rent(16).Length, "Rent(16)");
            Assert.AreEqual(32, pool.Rent(17).Length, "Rent(17)");
            Assert.AreEqual(32, pool.Rent(32).Length, "Rent(32)");
            Assert.AreEqual(64, pool.Rent(33).Length, "Rent(33)");
            Assert.AreEqual(1024, pool.Rent(1000).Length, "Rent(1000)");
        }

        [Test]
        public void Rent_AtTheDefaultMaximum_UsesTheLargestBucket()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            Assert.AreEqual(1024 * 1024, pool.Rent(1024 * 1024).Length);
        }

        [Test]
        public void Rent_LargerThanTheDefaultMaximum_AllocatesTheExactLength()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            // Too large for any bucket, so the pool allocates precisely what was asked for.
            Assert.AreEqual((1024 * 1024) + 1, pool.Rent((1024 * 1024) + 1).Length);
        }

        [Test]
        public void Rent_LargerThanACustomMaximum_AllocatesTheExactLength()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(16, 4);

            Assert.AreEqual(100, pool.Rent(100).Length);
        }

        [Test]
        public void Create_ClampsMaxArrayLengthToTheMinimumBucketSize()
        {
            // Anything below 16 is raised to 16 rather than rejected.
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1, 4);

            Assert.AreEqual(16, pool.Rent(1).Length);
        }

        [Test]
        public void ReturnThenRent_HandsBackTheSameBuffer()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);

            byte[] rented = pool.Rent(16);
            pool.Return(rented);

            Assert.AreSame(rented, pool.Rent(16));
        }

        [Test]
        public void Return_WithClearArray_ZeroesTheContents()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);

            byte[] rented = pool.Rent(16);
            rented[0] = 42;
            rented[15] = 42;
            pool.Return(rented, true);

            byte[] again = pool.Rent(16);
            Assert.AreSame(rented, again);
            Assert.AreEqual((byte)0, again[0]);
            Assert.AreEqual((byte)0, again[15]);
        }

        [Test]
        public void Return_WithoutClearArray_LeavesTheContentsAlone()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);

            byte[] rented = pool.Rent(16);
            rented[0] = 42;
            pool.Return(rented, false);

            byte[] again = pool.Rent(16);
            Assert.AreSame(rented, again);
            Assert.AreEqual((byte)42, again[0]);
        }

        [Test]
        public void Return_NullArray_Throws()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            Assert.Throws<ArgumentNullException>(delegate { pool.Return(null); });
        }

        [Test]
        public void Return_EmptyArray_IsIgnored()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            Assert.DoesNotThrow(delegate { pool.Return(new byte[0]); });
        }

        [Test]
        public void Return_ArrayWhoseLengthIsNotABucketSize_Throws()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();

            // 20 maps to the 32-byte bucket, which only accepts arrays of exactly 32.
            Assert.Throws<ArgumentException>(delegate { pool.Return(new byte[20]); });
            Assert.Throws<ArgumentException>(delegate { pool.Return(new byte[1]); });
        }

        [Test]
        public void Return_ArrayTooLargeForThePool_IsDropped()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(16, 4);

            // Beyond the last bucket the pool silently drops the array instead of validating its length.
            Assert.DoesNotThrow(delegate { pool.Return(new byte[4096]); });
        }

        [Test]
        public void Rent_WhenTheBucketIsExhausted_FallsBackToTheNextBucket()
        {
            // Two buckets (16 and 32), one buffer each.
            ArrayPool<byte> pool = ArrayPool<byte>.Create(32, 1);

            byte[] first = pool.Rent(16);
            byte[] second = pool.Rent(16);

            Assert.AreEqual(16, first.Length, "first rent uses the 16-byte bucket");
            Assert.AreEqual(32, second.Length, "the exhausted 16-byte bucket falls through to the 32-byte one");
            Assert.AreNotSame(first, second);
        }

        [Test]
        public void Rent_WhenEveryCandidateBucketIsExhausted_AllocatesAtTheRequestedBucketSize()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(32, 1);

            pool.Rent(16);
            pool.Rent(16);
            byte[] third = pool.Rent(16);

            // Both buckets are empty now, so a fresh 16-byte buffer is allocated.
            Assert.AreEqual(16, third.Length);
        }

        [Test]
        public void Rent_ReturnsDistinctBuffersWhileTheyAreOutstanding()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);

            byte[] first = pool.Rent(64);
            byte[] second = pool.Rent(64);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(64, first.Length);
            Assert.AreEqual(64, second.Length);
        }

        [Test]
        public void Pool_WorksForReferenceElementTypes()
        {
            ArrayPool<string> pool = ArrayPool<string>.Create(1024, 4);

            string[] rented = pool.Rent(10);
            Assert.AreEqual(16, rented.Length);

            rented[0] = "value";
            pool.Return(rented, true);

            string[] again = pool.Rent(10);
            Assert.AreSame(rented, again);
            Assert.IsNull(again[0]);
        }

        [Test]
        public void Return_ToABucketNothingWasRentedFrom_DropsTheBuffer()
        {
            // Buffers are not stamped with their owning pool, so a correctly sized array is accepted
            // anywhere - but a bucket only stores a buffer to fill a slot it previously handed out.
            ArrayPool<byte> source = ArrayPool<byte>.Create(1024, 4);
            ArrayPool<byte> destination = ArrayPool<byte>.Create(1024, 4);

            byte[] rented = source.Rent(16);

            Assert.DoesNotThrow(delegate { destination.Return(rented); });
            Assert.AreNotSame(rented, destination.Rent(16));
        }

        [Test]
        public void Return_MoreBuffersThanWereRented_KeepsOnlyTheRentedCount()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);

            byte[] first = pool.Rent(16);
            byte[] extra = new byte[16];

            pool.Return(first);

            // The bucket is back to zero outstanding buffers, so this one has nowhere to go.
            Assert.DoesNotThrow(delegate { pool.Return(extra); });
            Assert.AreSame(first, pool.Rent(16));
        }
    }
}
