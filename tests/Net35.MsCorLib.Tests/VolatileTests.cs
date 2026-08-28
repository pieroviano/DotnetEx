// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Net35.MsCorLib.Tests
{
    /// <summary>
    /// Round-trip tests for every <see cref="Volatile"/> overload. The memory-barrier semantics cannot be
    /// asserted directly, so these pin down that each overload reads back exactly what was written,
    /// including at the boundaries of each type where a wrong-width barrier would corrupt the value.
    /// </summary>
    [TestFixture]
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

        [Test]
        public void Bool_RoundTrips()
        {
            bool location = false;

            Volatile.Write(ref location, true);
            Assert.IsTrue(Volatile.Read(ref location));

            Volatile.Write(ref location, false);
            Assert.IsFalse(Volatile.Read(ref location));
        }

        [Test]
        public void Byte_RoundTripsAtTheBoundaries()
        {
            byte location = 0;

            Volatile.Write(ref location, byte.MaxValue);
            Assert.AreEqual(byte.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, byte.MinValue);
            Assert.AreEqual(byte.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void SByte_RoundTripsAtTheBoundaries()
        {
            sbyte location = 0;

            Volatile.Write(ref location, sbyte.MaxValue);
            Assert.AreEqual(sbyte.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, sbyte.MinValue);
            Assert.AreEqual(sbyte.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void Short_RoundTripsAtTheBoundaries()
        {
            short location = 0;

            Volatile.Write(ref location, short.MaxValue);
            Assert.AreEqual(short.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, short.MinValue);
            Assert.AreEqual(short.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void UShort_RoundTripsAtTheBoundaries()
        {
            ushort location = 0;

            Volatile.Write(ref location, ushort.MaxValue);
            Assert.AreEqual(ushort.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, ushort.MinValue);
            Assert.AreEqual(ushort.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void Int_RoundTripsAtTheBoundaries()
        {
            int location = 0;

            Volatile.Write(ref location, int.MaxValue);
            Assert.AreEqual(int.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, int.MinValue);
            Assert.AreEqual(int.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void UInt_RoundTripsAtTheBoundaries()
        {
            uint location = 0;

            Volatile.Write(ref location, uint.MaxValue);
            Assert.AreEqual(uint.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, uint.MinValue);
            Assert.AreEqual(uint.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void Long_RoundTripsAtTheBoundaries()
        {
            long location = 0;

            Volatile.Write(ref location, long.MaxValue);
            Assert.AreEqual(long.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, long.MinValue);
            Assert.AreEqual(long.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void ULong_RoundTripsAtTheBoundaries()
        {
            ulong location = 0;

            Volatile.Write(ref location, ulong.MaxValue);
            Assert.AreEqual(ulong.MaxValue, Volatile.Read(ref location));

            Volatile.Write(ref location, ulong.MinValue);
            Assert.AreEqual(ulong.MinValue, Volatile.Read(ref location));
        }

        [Test]
        public void Float_RoundTrips()
        {
            float location = 0f;

            Volatile.Write(ref location, 3.5f);
            Assert.AreEqual(3.5f, Volatile.Read(ref location));

            Volatile.Write(ref location, float.MaxValue);
            Assert.AreEqual(float.MaxValue, Volatile.Read(ref location));
        }

        [Test]
        public void Double_RoundTrips()
        {
            double location = 0d;

            Volatile.Write(ref location, 3.5d);
            Assert.AreEqual(3.5d, Volatile.Read(ref location));

            Volatile.Write(ref location, double.MaxValue);
            Assert.AreEqual(double.MaxValue, Volatile.Read(ref location));
        }

        [Test]
        public void IntPtr_RoundTrips()
        {
            IntPtr location = IntPtr.Zero;

            Volatile.Write(ref location, new IntPtr(1234));
            Assert.AreEqual(new IntPtr(1234), Volatile.Read(ref location));

            Volatile.Write(ref location, IntPtr.Zero);
            Assert.AreEqual(IntPtr.Zero, Volatile.Read(ref location));
        }

        [Test]
        public void UIntPtr_RoundTrips()
        {
            UIntPtr location = UIntPtr.Zero;

            Volatile.Write(ref location, new UIntPtr(1234));
            Assert.AreEqual(new UIntPtr(1234), Volatile.Read(ref location));

            Volatile.Write(ref location, UIntPtr.Zero);
            Assert.AreEqual(UIntPtr.Zero, Volatile.Read(ref location));
        }

        [Test]
        public void Reference_RoundTrips()
        {
            Payload location = null;
            Payload written = new Payload(7);

            Volatile.Write<Payload>(ref location, written);

            Payload read = Volatile.Read<Payload>(ref location);
            Assert.AreSame(written, read);
            Assert.AreEqual(7, read.Value);
        }

        [Test]
        public void Reference_CanBeSetBackToNull()
        {
            Payload location = new Payload(1);

            Volatile.Write<Payload>(ref location, null);

            Assert.IsNull(Volatile.Read<Payload>(ref location));
        }

        [Test]
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

            Assert.IsTrue(reader.Join(TimeSpan.FromSeconds(10)), "the reader thread did not observe the write");
            Assert.AreEqual(99, observedPayload);
        }
    }
}
