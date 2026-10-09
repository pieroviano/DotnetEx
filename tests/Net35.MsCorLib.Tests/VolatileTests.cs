// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Threading;
using Volatile = shim::System.Threading.Volatile;

namespace Net35.MsCorLib.Tests
{
    public sealed class VolatileTests
    {
        private sealed class Payload
        {
            public Payload(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }

        [Fact]
        public void Bool_RoundTrips()
        {
            bool location = false;
            Volatile.Write(ref location, true);
            Assert.True(Volatile.Read(ref location));
            Volatile.Write(ref location, false);
            Assert.False(Volatile.Read(ref location));
        }

        [Fact]
        public void Byte_RoundTripsAtTheBoundaries()
        {
            byte location = 0;
            Volatile.Write(ref location, byte.MaxValue);
            Assert.Equal(byte.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, byte.MinValue);
            Assert.Equal(byte.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void SByte_RoundTripsAtTheBoundaries()
        {
            sbyte location = 0;
            Volatile.Write(ref location, sbyte.MaxValue);
            Assert.Equal(sbyte.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, sbyte.MinValue);
            Assert.Equal(sbyte.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void Short_RoundTripsAtTheBoundaries()
        {
            short location = 0;
            Volatile.Write(ref location, short.MaxValue);
            Assert.Equal(short.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, short.MinValue);
            Assert.Equal(short.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void UShort_RoundTripsAtTheBoundaries()
        {
            ushort location = 0;
            Volatile.Write(ref location, ushort.MaxValue);
            Assert.Equal(ushort.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, ushort.MinValue);
            Assert.Equal(ushort.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void Int_RoundTripsAtTheBoundaries()
        {
            int location = 0;
            Volatile.Write(ref location, int.MaxValue);
            Assert.Equal(int.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, int.MinValue);
            Assert.Equal(int.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void UInt_RoundTripsAtTheBoundaries()
        {
            uint location = 0;
            Volatile.Write(ref location, uint.MaxValue);
            Assert.Equal(uint.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, uint.MinValue);
            Assert.Equal(uint.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void Long_RoundTripsAtTheBoundaries()
        {
            long location = 0;
            Volatile.Write(ref location, long.MaxValue);
            Assert.Equal(long.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, long.MinValue);
            Assert.Equal(long.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void ULong_RoundTripsAtTheBoundaries()
        {
            ulong location = 0;
            Volatile.Write(ref location, ulong.MaxValue);
            Assert.Equal(ulong.MaxValue, Volatile.Read(ref location));
            Volatile.Write(ref location, ulong.MinValue);
            Assert.Equal(ulong.MinValue, Volatile.Read(ref location));
        }

        [Fact]
        public void Float_RoundTrips()
        {
            float location = 0f;
            Volatile.Write(ref location, 3.5f);
            Assert.Equal(3.5f, Volatile.Read(ref location));
            Volatile.Write(ref location, float.MaxValue);
            Assert.Equal(float.MaxValue, Volatile.Read(ref location));
        }

        [Fact]
        public void Double_RoundTrips()
        {
            double location = 0d;
            Volatile.Write(ref location, 3.5d);
            Assert.Equal(3.5d, Volatile.Read(ref location));
            Volatile.Write(ref location, double.MaxValue);
            Assert.Equal(double.MaxValue, Volatile.Read(ref location));
        }

        [Fact]
        public void IntPtr_RoundTrips()
        {
            IntPtr location = IntPtr.Zero;
            Volatile.Write(ref location, new IntPtr(1234));
            Assert.Equal(new IntPtr(1234), Volatile.Read(ref location));
            Volatile.Write(ref location, IntPtr.Zero);
            Assert.Equal(IntPtr.Zero, Volatile.Read(ref location));
        }

        [Fact]
        public void UIntPtr_RoundTrips()
        {
            UIntPtr location = UIntPtr.Zero;
            Volatile.Write(ref location, new UIntPtr(1234));
            Assert.Equal(new UIntPtr(1234), Volatile.Read(ref location));
            Volatile.Write(ref location, UIntPtr.Zero);
            Assert.Equal(UIntPtr.Zero, Volatile.Read(ref location));
        }

        [Fact]
        public void Reference_RoundTrips()
        {
            Payload location = null;
            Payload written = new Payload(7);
            Volatile.Write<Payload>(ref location, written);
            Payload read = Volatile.Read<Payload>(ref location);
            Assert.Same(written, read);
            Assert.Equal(7, read.Value);
        }

        [Fact]
        public void Reference_CanBeSetBackToNull()
        {
            Payload location = new Payload(1);
            Volatile.Write<Payload>(ref location, null);
            Assert.Null(Volatile.Read<Payload>(ref location));
        }

        [Fact]
        public void Write_IsObservedByAnotherThread()
        {
            // A functional check that the write actually publishes: the reader spins on the flag and
            // must see both the flag and the payload written before it.
            int payload = 0;
            bool flag = false;
            int observedPayload = -1;
            Thread reader = new Thread(delegate ()
            {
                while (!Volatile.Read(ref flag))
                {
                    Thread.Sleep(0);
                }

                observedPayload = Volatile.Read(ref payload);
            });
            reader.IsBackground = true;
            reader.Start();
            Volatile.Write(ref payload, 99);
            Volatile.Write(ref flag, true);
            Assert.True(reader.Join(TimeSpan.FromSeconds(10)), "the reader thread did not observe the write");
            Assert.Equal(99, observedPayload);
        }
    }
}