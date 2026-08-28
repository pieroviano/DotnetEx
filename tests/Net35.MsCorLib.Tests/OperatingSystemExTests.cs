// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="OperatingSystemEx"/>.</summary>
    [TestFixture]
    public sealed class OperatingSystemExTests
    {
        [Test]
        public void OSPlatform_IsOneOfTheKnownTokens()
        {
            string platform = OperatingSystemEx.OSPlatform;

            Assert.IsNotEmpty(platform);
            Assert.IsTrue(
                platform == "WINDOWS" || platform == "LINUX" || platform == "OSX" || platform == "UNKNOWN",
                "unexpected platform token: " + platform);
        }

        [Test]
        public void IsOSPlatform_MatchesOSPlatformCaseInsensitively()
        {
            string platform = OperatingSystemEx.OSPlatform;

            Assert.IsTrue(OperatingSystemEx.IsOSPlatform(platform));
            Assert.IsTrue(OperatingSystemEx.IsOSPlatform(platform.ToLowerInvariant()));
        }

        [Test]
        public void IsOSPlatform_ForAnotherPlatform_IsFalse()
        {
            Assert.IsFalse(OperatingSystemEx.IsOSPlatform("A-PLATFORM-THAT-DOES-NOT-EXIST"));
        }

        [Test]
        public void IsOSPlatform_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { OperatingSystemEx.IsOSPlatform(null); });
        }

        [Test]
        public void PlatformPredicates_ExactlyOneMatchesTheCurrentPlatform()
        {
            int matches = 0;

            if (OperatingSystemEx.IsWindows())
            {
                matches++;
            }

            if (OperatingSystemEx.IsLinux())
            {
                matches++;
            }

            if (OperatingSystemEx.IsMacOS())
            {
                matches++;
            }

            // OSPlatform only ever reports WINDOWS, LINUX, OSX or UNKNOWN.
            Assert.IsTrue(matches <= 1, "more than one platform predicate matched");
            Assert.AreEqual(OperatingSystemEx.OSPlatform != "UNKNOWN", matches == 1);
        }

        [Test]
        public void PlatformPredicates_ForPlatformsThisPolyfillNeverReports_AreFalse()
        {
            Assert.IsFalse(OperatingSystemEx.IsBrowser());
            Assert.IsFalse(OperatingSystemEx.IsAndroid());
            Assert.IsFalse(OperatingSystemEx.IsIOS());
            Assert.IsFalse(OperatingSystemEx.IsTvOS());
            Assert.IsFalse(OperatingSystemEx.IsWatchOS());
            Assert.IsFalse(OperatingSystemEx.IsFreeBSD());
        }

        [Test]
        public void IsMacCatalyst_TracksIsMacOS()
        {
            // Both are mapped onto the OSX token by this polyfill.
            Assert.AreEqual(OperatingSystemEx.IsMacOS(), OperatingSystemEx.IsMacCatalyst());
        }

        [Test]
        public void IsOSPlatformVersionAtLeast_ForVersionZero_IsTrueOnTheCurrentPlatform()
        {
            Assert.IsTrue(OperatingSystemEx.IsOSPlatformVersionAtLeast(OperatingSystemEx.OSPlatform, 0));
        }

        [Test]
        public void IsOSPlatformVersionAtLeast_ForAnImpossibleVersion_IsFalse()
        {
            Assert.IsFalse(OperatingSystemEx.IsOSPlatformVersionAtLeast(OperatingSystemEx.OSPlatform, int.MaxValue));
        }

        [Test]
        public void IsOSPlatformVersionAtLeast_ForAnotherPlatform_IsFalse()
        {
            Assert.IsFalse(OperatingSystemEx.IsOSPlatformVersionAtLeast("A-PLATFORM-THAT-DOES-NOT-EXIST", 0));
        }

        [Test]
        public void IsOSPlatformVersionAtLeast_MatchesTheCurrentOSVersion()
        {
            Version current = Environment.OSVersion.Version;
            string platform = OperatingSystemEx.OSPlatform;

            Assert.IsTrue(
                OperatingSystemEx.IsOSPlatformVersionAtLeast(platform, current.Major, current.Minor),
                "the current version should satisfy itself");
            Assert.IsFalse(
                OperatingSystemEx.IsOSPlatformVersionAtLeast(platform, current.Major + 1),
                "a higher major version should not be satisfied");
        }

        [Test]
        public void VersionAtLeastPredicates_AgreeWithTheirPlatformPredicate()
        {
            // Each Is<Platform>VersionAtLeast is gated on Is<Platform>, so it can never be true
            // when the platform predicate is false.
            Assert.IsTrue(!OperatingSystemEx.IsAndroidVersionAtLeast(0) || OperatingSystemEx.IsAndroid());
            Assert.IsTrue(!OperatingSystemEx.IsFreeBSDVersionAtLeast(0) || OperatingSystemEx.IsFreeBSD());
            Assert.IsTrue(!OperatingSystemEx.IsIOSVersionAtLeast(0) || OperatingSystemEx.IsIOS());
            Assert.IsTrue(!OperatingSystemEx.IsMacOSVersionAtLeast(0) || OperatingSystemEx.IsMacOS());
            Assert.IsTrue(!OperatingSystemEx.IsWindowsVersionAtLeast(0) || OperatingSystemEx.IsWindows());
        }

        [Test]
        public void Constructor_ExposesPlatformAndVersion()
        {
            Version version = new Version(10, 0, 19045);
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, version);

            Assert.AreEqual(PlatformID.Win32NT, os.Platform);
            Assert.AreSame(version, os.Version);
            Assert.IsEmpty(os.ServicePack);
        }

        [Test]
        public void Constructor_NullVersion_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { new OperatingSystemEx(PlatformID.Win32NT, null); });
        }

        [Test]
        public void Constructor_PlatformOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new OperatingSystemEx((PlatformID)999, new Version(1, 0)); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new OperatingSystemEx((PlatformID)(-1), new Version(1, 0)); });
        }

        [Test]
        public void VersionString_ForWindowsNT()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(10, 0, 19045));

            Assert.AreEqual("Microsoft Windows NT 10.0.19045", os.VersionString);
        }

        [Test]
        public void VersionString_ForWindows95AndWindows98()
        {
            OperatingSystemEx windows95 = new OperatingSystemEx(PlatformID.Win32Windows, new Version(4, 0));
            OperatingSystemEx windows98 = new OperatingSystemEx(PlatformID.Win32Windows, new Version(4, 10));

            Assert.AreEqual("Microsoft Windows 95 4.0", windows95.VersionString);
            Assert.AreEqual("Microsoft Windows 98 4.10", windows98.VersionString);
        }

        [Test]
        public void VersionString_ForTheOtherPlatforms()
        {
            StringAssert.StartsWith("Microsoft Win32S ", new OperatingSystemEx(PlatformID.Win32S, new Version(1, 0)).VersionString);
            StringAssert.StartsWith("Microsoft Windows CE ", new OperatingSystemEx(PlatformID.WinCE, new Version(1, 0)).VersionString);
            StringAssert.StartsWith("Unix ", new OperatingSystemEx(PlatformID.Unix, new Version(1, 0)).VersionString);
            StringAssert.StartsWith("Xbox ", new OperatingSystemEx(PlatformID.Xbox, new Version(1, 0)).VersionString);
            StringAssert.StartsWith("Mac OS X ", new OperatingSystemEx(PlatformID.MacOSX, new Version(1, 0)).VersionString);
        }

        [Test]
        public void VersionString_IsCached()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Unix, new Version(5, 4));

            Assert.AreSame(os.VersionString, os.VersionString);
        }

        [Test]
        public void ToString_ReturnsVersionString()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));

            Assert.AreEqual(os.VersionString, os.ToString());
        }

        [Test]
        public void Clone_ProducesAnEqualButDistinctInstance()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));
            OperatingSystemEx clone = (OperatingSystemEx)os.Clone();

            Assert.AreNotSame(os, clone);
            Assert.AreEqual(os.Platform, clone.Platform);
            Assert.AreSame(os.Version, clone.Version);
            Assert.AreEqual(os.ServicePack, clone.ServicePack);
            Assert.AreEqual(os.VersionString, clone.VersionString);
        }

        [Test]
        public void GetObjectData_IsNotSupported()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));

            Assert.Throws<PlatformNotSupportedException>(delegate { os.GetObjectData(null, default(System.Runtime.Serialization.StreamingContext)); });
        }
    }
}
