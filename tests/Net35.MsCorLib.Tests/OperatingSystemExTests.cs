// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Net35.MsCorLib.Tests
{
    public sealed class OperatingSystemExTests
    {
        [Fact]
        public void OSPlatform_IsOneOfTheKnownTokens()
        {
            string platform = OperatingSystemEx.OSPlatform;
            Assert.NotEmpty(platform);
            Assert.True(platform == "WINDOWS" || platform == "LINUX" || platform == "OSX" || platform == "UNKNOWN", "unexpected platform token: " + platform);
        }

        [Fact]
        public void IsOSPlatform_MatchesOSPlatformCaseInsensitively()
        {
            string platform = OperatingSystemEx.OSPlatform;
            Assert.True(OperatingSystemEx.IsOSPlatform(platform));
            Assert.True(OperatingSystemEx.IsOSPlatform(platform.ToLowerInvariant()));
        }

        [Fact]
        public void IsOSPlatform_ForAnotherPlatform_IsFalse()
        {
            Assert.False(OperatingSystemEx.IsOSPlatform("A-PLATFORM-THAT-DOES-NOT-EXIST"));
        }

        [Fact]
        public void IsOSPlatform_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                OperatingSystemEx.IsOSPlatform(null);
            });
        }

        [Fact]
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

            Assert.True(matches <= 1, "more than one platform predicate matched");
            Assert.Equal(OperatingSystemEx.OSPlatform != "UNKNOWN", matches == 1);
        }

        [Fact]
        public void PlatformPredicates_ForPlatformsThisPolyfillNeverReports_AreFalse()
        {
            Assert.False(OperatingSystemEx.IsBrowser());
            Assert.False(OperatingSystemEx.IsAndroid());
            Assert.False(OperatingSystemEx.IsIOS());
            Assert.False(OperatingSystemEx.IsTvOS());
            Assert.False(OperatingSystemEx.IsWatchOS());
            Assert.False(OperatingSystemEx.IsFreeBSD());
        }

        [Fact]
        public void IsMacCatalyst_TracksIsMacOS()
        {
            Assert.Equal(OperatingSystemEx.IsMacOS(), OperatingSystemEx.IsMacCatalyst());
        }

        [Fact]
        public void IsOSPlatformVersionAtLeast_ForVersionZero_IsTrueOnTheCurrentPlatform()
        {
            Assert.True(OperatingSystemEx.IsOSPlatformVersionAtLeast(OperatingSystemEx.OSPlatform, 0));
        }

        [Fact]
        public void IsOSPlatformVersionAtLeast_ForAnImpossibleVersion_IsFalse()
        {
            Assert.False(OperatingSystemEx.IsOSPlatformVersionAtLeast(OperatingSystemEx.OSPlatform, int.MaxValue));
        }

        [Fact]
        public void IsOSPlatformVersionAtLeast_ForAnotherPlatform_IsFalse()
        {
            Assert.False(OperatingSystemEx.IsOSPlatformVersionAtLeast("A-PLATFORM-THAT-DOES-NOT-EXIST", 0));
        }

        [Fact]
        public void IsOSPlatformVersionAtLeast_MatchesTheCurrentOSVersion()
        {
            Version current = Environment.OSVersion.Version;
            string platform = OperatingSystemEx.OSPlatform;
            Assert.True(OperatingSystemEx.IsOSPlatformVersionAtLeast(platform, current.Major, current.Minor), "the current version should satisfy itself");
            Assert.False(OperatingSystemEx.IsOSPlatformVersionAtLeast(platform, current.Major + 1), "a higher major version should not be satisfied");
        }

        [Fact]
        public void VersionAtLeastPredicates_AgreeWithTheirPlatformPredicate()
        {
            Assert.True(!OperatingSystemEx.IsAndroidVersionAtLeast(0) || OperatingSystemEx.IsAndroid());
            Assert.True(!OperatingSystemEx.IsFreeBSDVersionAtLeast(0) || OperatingSystemEx.IsFreeBSD());
            Assert.True(!OperatingSystemEx.IsIOSVersionAtLeast(0) || OperatingSystemEx.IsIOS());
            Assert.True(!OperatingSystemEx.IsMacOSVersionAtLeast(0) || OperatingSystemEx.IsMacOS());
            Assert.True(!OperatingSystemEx.IsWindowsVersionAtLeast(0) || OperatingSystemEx.IsWindows());
        }

        [Fact]
        public void Constructor_ExposesPlatformAndVersion()
        {
            Version version = new Version(10, 0, 19045);
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, version);
            Assert.Equal(PlatformID.Win32NT, os.Platform);
            Assert.Same(version, os.Version);
            Assert.Empty(os.ServicePack);
        }

        [Fact]
        public void Constructor_NullVersion_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new OperatingSystemEx(PlatformID.Win32NT, null);
            });
        }

        [Fact]
        public void Constructor_PlatformOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                new OperatingSystemEx((PlatformID)999, new Version(1, 0));
            });
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                new OperatingSystemEx((PlatformID)(-1), new Version(1, 0));
            });
        }

        [Fact]
        public void VersionString_ForWindowsNT()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(10, 0, 19045));
            Assert.Equal("Microsoft Windows NT 10.0.19045", os.VersionString);
        }

        [Fact]
        public void VersionString_ForWindows95AndWindows98()
        {
            OperatingSystemEx windows95 = new OperatingSystemEx(PlatformID.Win32Windows, new Version(4, 0));
            OperatingSystemEx windows98 = new OperatingSystemEx(PlatformID.Win32Windows, new Version(4, 10));
            Assert.Equal("Microsoft Windows 95 4.0", windows95.VersionString);
            Assert.Equal("Microsoft Windows 98 4.10", windows98.VersionString);
        }

        [Fact]
        public void VersionString_ForTheOtherPlatforms()
        {
            Assert.StartsWith("Microsoft Win32S ", new OperatingSystemEx(PlatformID.Win32S, new Version(1, 0)).VersionString);
            Assert.StartsWith("Microsoft Windows CE ", new OperatingSystemEx(PlatformID.WinCE, new Version(1, 0)).VersionString);
            Assert.StartsWith("Unix ", new OperatingSystemEx(PlatformID.Unix, new Version(1, 0)).VersionString);
            Assert.StartsWith("Xbox ", new OperatingSystemEx(PlatformID.Xbox, new Version(1, 0)).VersionString);
            Assert.StartsWith("Mac OS X ", new OperatingSystemEx(PlatformID.MacOSX, new Version(1, 0)).VersionString);
        }

        [Fact]
        public void VersionString_IsCached()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Unix, new Version(5, 4));
            Assert.Same(os.VersionString, os.VersionString);
        }

        [Fact]
        public void ToString_ReturnsVersionString()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));
            Assert.Equal(os.VersionString, os.ToString());
        }

        [Fact]
        public void Clone_ProducesAnEqualButDistinctInstance()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));
            OperatingSystemEx clone = (OperatingSystemEx)os.Clone();
            Assert.NotSame(os, clone);
            Assert.Equal(os.Platform, clone.Platform);
            Assert.Same(os.Version, clone.Version);
            Assert.Equal(os.ServicePack, clone.ServicePack);
            Assert.Equal(os.VersionString, clone.VersionString);
        }

        [Fact]
        public void GetObjectData_IsNotSupported()
        {
            OperatingSystemEx os = new OperatingSystemEx(PlatformID.Win32NT, new Version(6, 1));
            Assert.Throws<PlatformNotSupportedException>(delegate
            {
                os.GetObjectData(null, default(System.Runtime.Serialization.StreamingContext));
            });
        }
    }
}