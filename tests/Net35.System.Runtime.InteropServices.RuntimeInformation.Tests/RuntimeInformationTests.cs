// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Runtime.InteropServices;
using RuntimeInformation = shim::System.Runtime.InteropServices.RuntimeInformation;
using OSPlatform = shim::System.Runtime.InteropServices.OSPlatform;
using Architecture = shim::System.Runtime.InteropServices.Architecture;

namespace Net35.InteropServices.Tests
{
    public sealed class RuntimeInformationTests
    {
        [Fact]
        public void FrameworkDescription_StartsWithTheFrameworkName()
        {
            string description = RuntimeInformation.FrameworkDescription;
            Assert.NotEmpty(description);
            Assert.StartsWith(".NET", description);
        }

        [Fact]
        public void FrameworkDescription_IsCached()
        {
            Assert.Same(RuntimeInformation.FrameworkDescription, RuntimeInformation.FrameworkDescription);
        }

        [Fact]
        public void FrameworkDescription_HasNoGitHashSuffix()
        {
            Assert.False(RuntimeInformation.FrameworkDescription.Contains("+"), "the git hash should have been stripped");
        }

        [Fact]
        public void OSArchitecture_MatchesTheOperatingSystemBitness()
        {
            Architecture expected = EnvironmentEx.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
            Assert.Equal(expected, RuntimeInformation.OSArchitecture);
        }

        [WindowsOnlyFact]
        public void OSDescription_DescribesWindows()
        {
            string description = RuntimeInformation.OSDescription;
            Assert.NotEmpty(description);
            Assert.StartsWith("Microsoft Windows ", description);
        }

        [WindowsOnlyFact]
        public void OSDescription_CarriesTheOSVersionNumbers()
        {
            Version version = Environment.OSVersion.Version;
            Assert.Contains(version.Major + "." + version.Minor + "." + version.Build, RuntimeInformation.OSDescription);
        }

        [WindowsOnlyFact]
        public void OSDescription_IsCached()
        {
            Assert.Same(RuntimeInformation.OSDescription, RuntimeInformation.OSDescription);
        }

        [Fact]
        public void RuntimeIdentifier_CombinesThePlatformAndArchitecture()
        {
            string identifier = RuntimeInformation.RuntimeIdentifier;
            string architecture = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
            Assert.NotEmpty(identifier);
            Assert.True(identifier.EndsWith("-" + architecture, StringComparison.Ordinal), "unexpected identifier: " + identifier);
            switch (OperatingSystemEx.OSPlatform)
            {
                case "WINDOWS":
                    Assert.StartsWith("win-", identifier);
                    break;
                case "LINUX":
                    Assert.StartsWith("linux-", identifier);
                    break;
                case "OSX":
                    Assert.StartsWith("osx-", identifier);
                    break;
                default:
                    Assert.StartsWith("unknown-", identifier);
                    break;
            }
        }

        [Fact]
        public void IsOSPlatform_MatchesTheCurrentPlatform()
        {
            bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool osx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            Assert.Equal(OperatingSystemEx.IsWindows(), windows);
            Assert.Equal(OperatingSystemEx.IsLinux(), linux);
            Assert.Equal(OperatingSystemEx.IsMacOS(), osx);
        }

        [Fact]
        public void IsOSPlatform_FreeBSD_IsFalse()
        {
            Assert.False(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD));
        }

        [Fact]
        public void OSPlatform_WellKnownValuesHaveTheExpectedNames()
        {
            Assert.Equal("WINDOWS", OSPlatform.Windows.ToString());
            Assert.Equal("LINUX", OSPlatform.Linux.ToString());
            Assert.Equal("OSX", OSPlatform.OSX.ToString());
            Assert.Equal("FREEBSD", OSPlatform.FreeBSD.ToString());
        }

        [Fact]
        public void OSPlatform_Create_ProducesAnEquivalentValue()
        {
            OSPlatform created = OSPlatform.Create("WINDOWS");
            Assert.True(created.Equals(OSPlatform.Windows));
            Assert.True(created == OSPlatform.Windows);
            Assert.False(created != OSPlatform.Windows);
        }

        [Fact]
        public void OSPlatform_Create_IsCaseInsensitiveForComparison()
        {
            OSPlatform created = OSPlatform.Create("windows");
            Assert.True(created == OSPlatform.Windows);
            Assert.Equal(OSPlatform.Windows.GetHashCode(), created.GetHashCode());
        }

        [Fact]
        public void OSPlatform_Create_EmptyString_Throws()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                OSPlatform.Create(string.Empty);
            });
        }

        [Fact]
        public void OSPlatform_Create_Null_Throws()
        {
            Assert.Throws<NullReferenceException>(delegate
            {
                OSPlatform.Create(null);
            });
        }

        [Fact]
        public void OSPlatform_DifferentPlatformsAreNotEqual()
        {
            Assert.False(OSPlatform.Windows == OSPlatform.Linux);
            Assert.True(OSPlatform.Windows != OSPlatform.Linux);
            Assert.False(OSPlatform.Windows.Equals(OSPlatform.Linux));
        }

        [Fact]
        public void OSPlatform_EqualsObject()
        {
            Assert.True(OSPlatform.Windows.Equals((object)OSPlatform.Create("WINDOWS")));
            Assert.False(OSPlatform.Windows.Equals((object)"WINDOWS"));
            Assert.False(OSPlatform.Windows.Equals((object)null));
        }

        [Fact]
        public void OSPlatform_GetHashCode_IsStable()
        {
            Assert.Equal(OSPlatform.Windows.GetHashCode(), OSPlatform.Windows.GetHashCode());
        }

        [Fact]
        public void OSPlatform_Default_IsEmpty()
        {
            OSPlatform uninitialised = default(OSPlatform);
            Assert.Empty(uninitialised.ToString());
            Assert.Equal(0, uninitialised.GetHashCode());
        }

        [Fact]
        public void Architecture_HasTheExpectedMembers()
        {
            Assert.Equal(0, (int)Architecture.X86);
            Assert.Equal(1, (int)Architecture.X64);
            Assert.Equal(2, (int)Architecture.Arm);
            Assert.Equal(3, (int)Architecture.Arm64);
            Assert.Equal(4, (int)Architecture.Wasm);
            Assert.Equal(5, (int)Architecture.S390x);
        }
    }
}