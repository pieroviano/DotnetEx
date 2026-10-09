// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Buffers;
using Xunit.Abstractions;

namespace Net35.Buffers.Tests
{
    public sealed class ArrayPoolTests
    {
        public ArrayPoolTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;
        [Fact]
        public void Shared_ReturnsSameInstanceEveryTime()
        {
            ArrayPool<byte> first = ArrayPool<byte>.Shared;
            ArrayPool<byte> second = ArrayPool<byte>.Shared;
            Assert.NotNull(first);
            Assert.Same(first, second);
        }

        [Fact]
        public void Shared_IsSeparatePerElementType()
        {
            Assert.NotSame(ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
        }

        [Fact]
        public void Create_ReturnsANewPoolEachCall()
        {
            ArrayPool<byte> first = ArrayPool<byte>.Create();
            ArrayPool<byte> second = ArrayPool<byte>.Create();
            Assert.NotNull(first);
            Assert.NotSame(first, second);
        }

        [Fact]
        public void Create_WithNonPositiveMaxArrayLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                ArrayPool<byte>.Create(0, 4);
            });
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                ArrayPool<byte>.Create(-1, 4);
            });
        }

        [Fact]
        public void Create_WithNonPositiveMaxArraysPerBucket_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                ArrayPool<byte>.Create(1024, 0);
            });
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                ArrayPool<byte>.Create(1024, -1);
            });
        }

        [Fact]
        public void Rent_NegativeLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                ArrayPool<byte>.Create().Rent(-1);
            });
        }

        [Fact]
        public void Rent_ZeroLength_ReturnsSharedEmptyArray()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            byte[] first = pool.Rent(0);
            byte[] second = pool.Rent(0);
            Assert.NotNull(first);
            Assert.Equal(0, first.Length);
            Assert.Same(first, second);
        }

        [Fact]
        public void Rent_RoundsUpToTheBucketSize()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            try
            {
                Assert.Equal(16, pool.Rent(1).Length);
            }
            catch
            {
                _output.WriteLine("Rent(1)");
                throw;
            }

            try
            {
                Assert.Equal(16, pool.Rent(15).Length);
            }
            catch
            {
                _output.WriteLine("Rent(15)");
                throw;
            }

            try
            {
                Assert.Equal(16, pool.Rent(16).Length);
            }
            catch
            {
                _output.WriteLine("Rent(16)");
                throw;
            }

            try
            {
                Assert.Equal(32, pool.Rent(17).Length);
            }
            catch
            {
                _output.WriteLine("Rent(17)");
                throw;
            }

            try
            {
                Assert.Equal(32, pool.Rent(32).Length);
            }
            catch
            {
                _output.WriteLine("Rent(32)");
                throw;
            }

            try
            {
                Assert.Equal(64, pool.Rent(33).Length);
            }
            catch
            {
                _output.WriteLine("Rent(33)");
                throw;
            }

            try
            {
                Assert.Equal(1024, pool.Rent(1000).Length);
            }
            catch
            {
                _output.WriteLine("Rent(1000)");
                throw;
            }
        }

        [Fact]
        public void Rent_AtTheDefaultMaximum_UsesTheLargestBucket()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            Assert.Equal(1024 * 1024, pool.Rent(1024 * 1024).Length);
        }

        [Fact]
        public void Rent_LargerThanTheDefaultMaximum_AllocatesTheExactLength()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            Assert.Equal((1024 * 1024) + 1, pool.Rent((1024 * 1024) + 1).Length);
        }

        [Fact]
        public void Rent_LargerThanACustomMaximum_AllocatesTheExactLength()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(16, 4);
            Assert.Equal(100, pool.Rent(100).Length);
        }

        [Fact]
        public void Create_ClampsMaxArrayLengthToTheMinimumBucketSize()
        {
            // Anything below 16 is raised to 16 rather than rejected.
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1, 4);
            Assert.Equal(16, pool.Rent(1).Length);
        }

        [Fact]
        public void ReturnThenRent_HandsBackTheSameBuffer()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);
            byte[] rented = pool.Rent(16);
            pool.Return(rented);
            Assert.Same(rented, pool.Rent(16));
        }

        [Fact]
        public void Return_WithClearArray_ZeroesTheContents()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);
            byte[] rented = pool.Rent(16);
            rented[0] = 42;
            rented[15] = 42;
            pool.Return(rented, true);
            byte[] again = pool.Rent(16);
            Assert.Same(rented, again);
            Assert.Equal((byte)0, again[0]);
            Assert.Equal((byte)0, again[15]);
        }

        [Fact]
        public void Return_WithoutClearArray_LeavesTheContentsAlone()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);
            byte[] rented = pool.Rent(16);
            rented[0] = 42;
            pool.Return(rented, false);
            byte[] again = pool.Rent(16);
            Assert.Same(rented, again);
            Assert.Equal((byte)42, again[0]);
        }

        [Fact]
        public void Return_NullArray_Throws()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            Assert.Throws<ArgumentNullException>(delegate
            {
                pool.Return(null);
            });
        }

        [Fact]
        public void Return_EmptyArray_IsIgnored()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            pool.Return(new byte[0]);
        }

        [Fact]
        public void Return_ArrayWhoseLengthIsNotABucketSize_Throws()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create();
            // 20 maps to the 32-byte bucket, which only accepts arrays of exactly 32.
            Assert.Throws<ArgumentException>(delegate
            {
                pool.Return(new byte[20]);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                pool.Return(new byte[1]);
            });
        }

        [Fact]
        public void Return_ArrayTooLargeForThePool_IsDropped()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(16, 4);
            // Beyond the last bucket the pool silently drops the array instead of validating its length.
            pool.Return(new byte[4096]);
        }

        [Fact]
        public void Rent_WhenTheBucketIsExhausted_FallsBackToTheNextBucket()
        {
            // Two buckets (16 and 32), one buffer each.
            ArrayPool<byte> pool = ArrayPool<byte>.Create(32, 1);
            byte[] first = pool.Rent(16);
            byte[] second = pool.Rent(16);
            try
            {
                Assert.Equal(16, first.Length);
            }
            catch
            {
                _output.WriteLine("first rent uses the 16-byte bucket");
                throw;
            }

            try
            {
                Assert.Equal(32, second.Length);
            }
            catch
            {
                _output.WriteLine("the exhausted 16-byte bucket falls through to the 32-byte one");
                throw;
            }

            Assert.NotSame(first, second);
        }

        [Fact]
        public void Rent_WhenEveryCandidateBucketIsExhausted_AllocatesAtTheRequestedBucketSize()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(32, 1);
            pool.Rent(16);
            pool.Rent(16);
            byte[] third = pool.Rent(16);
            Assert.Equal(16, third.Length);
        }

        [Fact]
        public void Rent_ReturnsDistinctBuffersWhileTheyAreOutstanding()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);
            byte[] first = pool.Rent(64);
            byte[] second = pool.Rent(64);
            Assert.NotSame(first, second);
            Assert.Equal(64, first.Length);
            Assert.Equal(64, second.Length);
        }

        [Fact]
        public void Pool_WorksForReferenceElementTypes()
        {
            ArrayPool<string> pool = ArrayPool<string>.Create(1024, 4);
            string[] rented = pool.Rent(10);
            Assert.Equal(16, rented.Length);
            rented[0] = "value";
            pool.Return(rented, true);
            string[] again = pool.Rent(10);
            Assert.Same(rented, again);
            Assert.Null(again[0]);
        }

        [Fact]
        public void Return_ToABucketNothingWasRentedFrom_DropsTheBuffer()
        {
            // Buffers are not stamped with their owning pool, so a correctly sized array is accepted
            // anywhere - but a bucket only stores a buffer to fill a slot it previously handed out.
            ArrayPool<byte> source = ArrayPool<byte>.Create(1024, 4);
            ArrayPool<byte> destination = ArrayPool<byte>.Create(1024, 4);
            byte[] rented = source.Rent(16);
            destination.Return(rented);
            Assert.NotSame(rented, destination.Rent(16));
        }

        [Fact]
        public void Return_MoreBuffersThanWereRented_KeepsOnlyTheRentedCount()
        {
            ArrayPool<byte> pool = ArrayPool<byte>.Create(1024, 4);
            byte[] first = pool.Rent(16);
            byte[] extra = new byte[16];
            pool.Return(first);
            // The bucket is back to zero outstanding buffers, so this one has nowhere to go.
            pool.Return(extra);
            Assert.Same(first, pool.Rent(16));
        }
    }
}