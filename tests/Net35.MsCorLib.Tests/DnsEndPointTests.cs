// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Net;
using System.Net.Sockets;
using DnsEndPoint = shim::System.Net.DnsEndPoint;

namespace Net35.MsCorLib.Tests
{
    public sealed class DnsEndPointTests
    {
        [Fact]
        public void Constructor_ExposesHostPortAndUnspecifiedFamily()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);
            Assert.Equal("example.com", endPoint.Host);
            Assert.Equal(443, endPoint.Port);
            Assert.Equal(AddressFamily.Unspecified, endPoint.AddressFamily);
        }

        [Fact]
        public void Constructor_WithAnExplicitAddressFamily_KeepsIt()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443, AddressFamily.InterNetworkV6);
            Assert.Equal(AddressFamily.InterNetworkV6, endPoint.AddressFamily);
        }

        [Fact]
        public void Constructor_NullHost_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new DnsEndPoint(null, 80);
            });
        }

        [Fact]
        public void Constructor_AcceptsTheFullPortRange()
        {
            Assert.Equal(IPEndPoint.MinPort, new DnsEndPoint("host", IPEndPoint.MinPort).Port);
            Assert.Equal(IPEndPoint.MaxPort, new DnsEndPoint("host", IPEndPoint.MaxPort).Port);
        }

        [Fact]
        public void Constructor_PortOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                new DnsEndPoint("host", IPEndPoint.MinPort - 1);
            });
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                new DnsEndPoint("host", IPEndPoint.MaxPort + 1);
            });
        }

        [Fact]
        public void Constructor_UnsupportedAddressFamily_Throws()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                new DnsEndPoint("host", 80, AddressFamily.AppleTalk);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new DnsEndPoint("host", 80, AddressFamily.Ipx);
            });
        }

        [Fact]
        public void Constructor_AcceptsAnEmptyHost()
        {
            // Only null is rejected; an empty host is passed through unchanged.
            Assert.Empty(new DnsEndPoint(string.Empty, 80).Host);
        }

        [Fact]
        public void ToString_CombinesFamilyHostAndPort()
        {
            Assert.Equal("Unspecified/example.com:443", new DnsEndPoint("example.com", 443).ToString());
            Assert.Equal("InterNetwork/example.com:80", new DnsEndPoint("example.com", 80, AddressFamily.InterNetwork).ToString());
        }

        [Fact]
        public void Equals_IsTrueForIdenticalEndPoints()
        {
            DnsEndPoint left = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            DnsEndPoint right = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            Assert.True(left.Equals(right));
            Assert.True(right.Equals(left));
        }

        [Fact]
        public void Equals_IsFalseWhenAnyComponentDiffers()
        {
            DnsEndPoint baseline = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            Assert.False(baseline.Equals(new DnsEndPoint("other.com", 443, AddressFamily.InterNetwork)), "host");
            Assert.False(baseline.Equals(new DnsEndPoint("example.com", 80, AddressFamily.InterNetwork)), "port");
            Assert.False(baseline.Equals(new DnsEndPoint("example.com", 443, AddressFamily.InterNetworkV6)), "family");
        }

        [Fact]
        public void Equals_HostComparisonIsCaseSensitive()
        {
            DnsEndPoint lower = new DnsEndPoint("example.com", 443);
            DnsEndPoint upper = new DnsEndPoint("EXAMPLE.COM", 443);
            Assert.False(lower.Equals(upper));
        }

        [Fact]
        public void Equals_IsFalseForNullAndForOtherTypes()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);
            Assert.False(endPoint.Equals(null));
            Assert.False(endPoint.Equals("example.com"));
            Assert.False(endPoint.Equals(new object ()));
        }

        [Fact]
        public void GetHashCode_IsEqualForEqualEndPoints()
        {
            DnsEndPoint left = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            DnsEndPoint right = new DnsEndPoint("example.com", 443, AddressFamily.InterNetwork);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IsStableAcrossCalls()
        {
            DnsEndPoint endPoint = new DnsEndPoint("example.com", 443);
            Assert.Equal(endPoint.GetHashCode(), endPoint.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IgnoresHostCasingEvenThoughEqualsDoesNot()
        {
            // The hash is computed over the case-insensitive form of ToString(), matching the BCL type.
            DnsEndPoint lower = new DnsEndPoint("example.com", 443);
            DnsEndPoint upper = new DnsEndPoint("EXAMPLE.COM", 443);
            Assert.Equal(lower.GetHashCode(), upper.GetHashCode());
            Assert.False(lower.Equals(upper));
        }

        [Fact]
        public void IsAnEndPoint()
        {
            Assert.True(new DnsEndPoint("example.com", 443) is EndPoint);
        }
    }
}