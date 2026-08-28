// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Net.Sockets;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="DnsEndPoint"/>.</summary>
    [TestFixture]
    public sealed class DnsEndPointTests
    {
        [Test]
        public void Constructor_ExposesHostPortAndUnspecifiedFamily()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);

            Assert.AreEqual("example.com", endPoint.Host);
            Assert.AreEqual(443, endPoint.Port);
            Assert.AreEqual(AddressFamily.Unspecified, endPoint.AddressFamily);
        }

        [Test]
        public void Constructor_WithAnExplicitAddressFamily_KeepsIt()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443, AddressFamily.InterNetworkV6);

            Assert.AreEqual(AddressFamily.InterNetworkV6, endPoint.AddressFamily);
        }

        [Test]
        public void Constructor_NullHost_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { new DnsEndPoint(null, 80); });
        }

        [Test]
        public void Constructor_AcceptsTheFullPortRange()
        {
            Assert.AreEqual(IPEndPoint.MinPort, new DnsEndPoint("host", IPEndPoint.MinPort).Port);
            Assert.AreEqual(IPEndPoint.MaxPort, new DnsEndPoint("host", IPEndPoint.MaxPort).Port);
        }

        [Test]
        public void Constructor_PortOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new DnsEndPoint("host", IPEndPoint.MinPort - 1); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new DnsEndPoint("host", IPEndPoint.MaxPort + 1); });
        }

        [Test]
        public void Constructor_UnsupportedAddressFamily_Throws()
        {
            Assert.Throws<ArgumentException>(delegate { new DnsEndPoint("host", 80, AddressFamily.AppleTalk); });
            Assert.Throws<ArgumentException>(delegate { new DnsEndPoint("host", 80, AddressFamily.Ipx); });
        }

        [Test]
        public void Constructor_AcceptsAnEmptyHost()
        {
            // Only null is rejected; an empty host is passed through unchanged.
            Assert.IsEmpty(new DnsEndPoint(string.Empty, 80).Host);
        }

        [Test]
        public void ToString_CombinesFamilyHostAndPort()
        {
            Assert.AreEqual("Unspecified/example.com:443", new DnsEndPoint("example.com", 443).ToString());
            Assert.AreEqual("InterNetwork/example.com:80", new DnsEndPoint("example.com", 80, AddressFamily.InterNetwork).ToString());
        }

        [Test]
        public void Equals_IsTrueForIdenticalEndPoints()
        {
            DnsEndPoint left = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            DnsEndPoint right = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);

            Assert.IsTrue(left.Equals(right));
            Assert.IsTrue(right.Equals(left));
        }

        [Test]
        public void Equals_IsFalseWhenAnyComponentDiffers()
        {
            DnsEndPoint baseline = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);

            Assert.IsFalse(baseline.Equals(new DnsEndPoint("other.com", 443, AddressFamily.InterNetwork)), "host");
            Assert.IsFalse(baseline.Equals(new DnsEndPoint("example.com", 80, AddressFamily.InterNetwork)), "port");
            Assert.IsFalse(baseline.Equals(new DnsEndPoint("example.com", 443, AddressFamily.InterNetworkV6)), "family");
        }

        [Test]
        public void Equals_HostComparisonIsCaseSensitive()
        {
            DnsEndPoint lower = new DnsEndPoint("example.com", 443);
            DnsEndPoint upper = new DnsEndPoint("EXAMPLE.COM", 443);

            Assert.IsFalse(lower.Equals(upper));
        }

        [Test]
        public void Equals_IsFalseForNullAndForOtherTypes()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);

            Assert.IsFalse(endPoint.Equals(null));
            Assert.IsFalse(endPoint.Equals("example.com"));
            Assert.IsFalse(endPoint.Equals(new object()));
        }

        [Test]
        public void GetHashCode_IsEqualForEqualEndPoints()
        {
            DnsEndPoint left = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            DnsEndPoint right = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);

            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Test]
        public void GetHashCode_IsStableAcrossCalls()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);

            Assert.AreEqual(endPoint.GetHashCode(), endPoint.GetHashCode());
        }

        [Test]
        public void GetHashCode_IgnoresHostCasingEvenThoughEqualsDoesNot()
        {
            // The hash is computed over the case-insensitive form of ToString(), matching the BCL type.
            DnsEndPoint lower = new DnsEndPoint("example.com", 443);
            DnsEndPoint upper = new DnsEndPoint("EXAMPLE.COM", 443);

            Assert.AreEqual(lower.GetHashCode(), upper.GetHashCode());
            Assert.IsFalse(lower.Equals(upper));
        }

        [Test]
        public void IsAnEndPoint()
        {
            Assert.IsTrue(new DnsEndPoint("example.com", 443) is EndPoint);
        }
    }
}
