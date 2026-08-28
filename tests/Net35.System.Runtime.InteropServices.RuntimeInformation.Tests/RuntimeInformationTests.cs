// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Net35.InteropServices.Tests
{
    /// <summary>Tests for <see cref="RuntimeInformation"/>, <see cref="OSPlatform"/> and <see cref="Architecture"/>.</summary>
    [TestFixture]
    public sealed class RuntimeInformationTests
    {
        [Test]
        public void FrameworkDescription_StartsWithTheFrameworkName()
        {
            string description = RuntimeInformation.FrameworkDescription;

            Assert.IsNotEmpty(description);
            StringAssert.StartsWith(".NET", description);
        }

        [Test]
        public void FrameworkDescription_IsCached()
        {
            Assert.AreSame(RuntimeInformation.FrameworkDescription, RuntimeInformation.FrameworkDescription);
        }

        [Test]
        public void FrameworkDescription_HasNoGitHashSuffix()
        {
            Assert.IsFalse(RuntimeInformation.FrameworkDescription.Contains("+"), "the git hash should have been stripped");
        }

        [Test]
        public void OSArchitecture_MatchesTheOperatingSystemBitness()
        {
            Architecture expected = EnvironmentEx.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;

            Assert.AreEqual(expected, RuntimeInformation.OSArchitecture);
        }

        [Test]
        public void OSDescription_DescribesWindows()
        {
            RequireWindows();

            string description = RuntimeInformation.OSDescription;

            Assert.IsNotEmpty(description);
            StringAssert.StartsWith("Microsoft Windows ", description);
        }

        [Test]
        public void OSDescription_CarriesTheOSVersionNumbers()
        {
            RequireWindows();

            Version version = Environment.OSVersion.Version;

            StringAssert.Contains(version.Major + "." + version.Minor + "." + version.Build, RuntimeInformation.OSDescription);
        }

        [Test]
        public void OSDescription_IsCached()
        {
            RequireWindows();

            Assert.AreSame(RuntimeInformation.OSDescription, RuntimeInformation.OSDescription);
        }

        [Test]
        public void RuntimeIdentifier_CombinesThePlatformAndArchitecture()
        {
            string identifier = RuntimeInformation.RuntimeIdentifier;
            string architecture = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();

            Assert.IsNotEmpty(identifier);
            Assert.IsTrue(identifier.EndsWith("-" + architecture, StringComparison.Ordinal), "unexpected identifier: " + identifier);

            switch (OperatingSystemEx.OSPlatform)
            {
                case "WINDOWS":
                    StringAssert.StartsWith("win-", identifier);
                    break;
                case "LINUX":
                    StringAssert.StartsWith("linux-", identifier);
                    break;
                case "OSX":
                    StringAssert.StartsWith("osx-", identifier);
                    break;
                default:
                    StringAssert.StartsWith("unknown-", identifier);
                    break;
            }
        }

        [Test]
        public void IsOSPlatform_MatchesTheCurrentPlatform()
        {
            bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool osx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            Assert.AreEqual(OperatingSystemEx.IsWindows(), windows);
            Assert.AreEqual(OperatingSystemEx.IsLinux(), linux);
            Assert.AreEqual(OperatingSystemEx.IsMacOS(), osx);
        }

        [Test]
        public void IsOSPlatform_FreeBSD_IsFalse()
        {
            // The polyfill never reports FREEBSD, so this is always false.
            Assert.IsFalse(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD));
        }

        [Test]
        public void OSPlatform_WellKnownValuesHaveTheExpectedNames()
        {
            Assert.AreEqual("WINDOWS", OSPlatform.Windows.ToString());
            Assert.AreEqual("LINUX", OSPlatform.Linux.ToString());
            Assert.AreEqual("OSX", OSPlatform.OSX.ToString());
            Assert.AreEqual("FREEBSD", OSPlatform.FreeBSD.ToString());
        }

        [Test]
        public void OSPlatform_Create_ProducesAnEquivalentValue()
        {
            OSPlatform created = OSPlatform.Create("WINDOWS");

            Assert.IsTrue(created.Equals(OSPlatform.Windows));
            Assert.IsTrue(created == OSPlatform.Windows);
            Assert.IsFalse(created != OSPlatform.Windows);
        }

        [Test]
        public void OSPlatform_Create_IsCaseInsensitiveForComparison()
        {
            OSPlatform created = OSPlatform.Create("windows");

            Assert.IsTrue(created == OSPlatform.Windows);
            Assert.AreEqual(OSPlatform.Windows.GetHashCode(), created.GetHashCode());
        }

        [Test]
        public void OSPlatform_Create_EmptyString_Throws()
        {
            Assert.Throws<ArgumentException>(delegate { OSPlatform.Create(string.Empty); });
        }

        [Test]
        public void OSPlatform_Create_Null_Throws()
        {
            Assert.Throws<NullReferenceException>(delegate { OSPlatform.Create(null); });
        }

        [Test]
        public void OSPlatform_DifferentPlatformsAreNotEqual()
        {
            Assert.IsFalse(OSPlatform.Windows == OSPlatform.Linux);
            Assert.IsTrue(OSPlatform.Windows != OSPlatform.Linux);
            Assert.IsFalse(OSPlatform.Windows.Equals(OSPlatform.Linux));
        }

        [Test]
        public void OSPlatform_EqualsObject()
        {
            Assert.IsTrue(OSPlatform.Windows.Equals((object)OSPlatform.Create("WINDOWS")));
            Assert.IsFalse(OSPlatform.Windows.Equals((object)"WINDOWS"));
            Assert.IsFalse(OSPlatform.Windows.Equals((object)null));
        }

        [Test]
        public void OSPlatform_GetHashCode_IsStable()
        {
            Assert.AreEqual(OSPlatform.Windows.GetHashCode(), OSPlatform.Windows.GetHashCode());
        }

        [Test]
        public void OSPlatform_Default_IsEmpty()
        {
            OSPlatform uninitialised = default(OSPlatform);

            Assert.IsEmpty(uninitialised.ToString());
            Assert.AreEqual(0, uninitialised.GetHashCode());
        }

        [Test]
        public void Architecture_HasTheExpectedMembers()
        {
            Assert.AreEqual(0, (int)Architecture.X86);
            Assert.AreEqual(1, (int)Architecture.X64);
            Assert.AreEqual(2, (int)Architecture.Arm);
            Assert.AreEqual(3, (int)Architecture.Arm64);
            Assert.AreEqual(4, (int)Architecture.Wasm);
            Assert.AreEqual(5, (int)Architecture.S390x);
        }

        /// <summary>Skips the test when not running on Windows.</summary>
        private static void RequireWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform != PlatformID.Win32NT && platform != PlatformID.Win32S
                && platform != PlatformID.Win32Windows && platform != PlatformID.WinCE)
            {
                Assert.Ignore("Windows-only test.");
            }
        }
    }
}
