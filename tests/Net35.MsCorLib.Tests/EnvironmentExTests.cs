// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="EnvironmentEx"/>.</summary>
    [TestFixture]
    public sealed class EnvironmentExTests
    {
        [Test]
        public void Is64BitProcess_MatchesThePointerWidth()
        {
            Assert.AreEqual(IntPtr.Size == 8, EnvironmentEx.Is64BitProcess);
        }

        [Test]
        public void Is64BitOperatingSystem_IsTrueWheneverTheProcessIs64Bit()
        {
            // A 64-bit process implies a 64-bit OS; the converse is not true.
            if (EnvironmentEx.Is64BitProcess)
            {
                Assert.IsTrue(EnvironmentEx.Is64BitOperatingSystem);
            }
        }

        [Test]
        public void Is64BitOperatingSystem_AgreesWithTheProcessorArchitectureVariables()
        {
            RequireWindows();

            // On Windows a WOW64 process reports the real machine architecture in
            // PROCESSOR_ARCHITEW6432, and 32-bit-only machines set neither to a 64-bit value.
            string architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            string architectureWow = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");

            bool looksSixtyFourBit = Is64Bit(architecture) || Is64Bit(architectureWow);

            Assert.AreEqual(looksSixtyFourBit, EnvironmentEx.Is64BitOperatingSystem);
        }

        [Test]
        public void Is64BitFlags_AreStable()
        {
            // They are static fields computed once at type initialisation, not recomputed per read.
            Assert.AreEqual(EnvironmentEx.Is64BitProcess, EnvironmentEx.Is64BitProcess);
            Assert.AreEqual(EnvironmentEx.Is64BitOperatingSystem, EnvironmentEx.Is64BitOperatingSystem);
        }

        private static bool Is64Bit(string architecture)
        {
            if (string.IsNullOrEmpty(architecture))
            {
                return false;
            }

            return architecture.Equals("AMD64", StringComparison.OrdinalIgnoreCase)
                || architecture.Equals("IA64", StringComparison.OrdinalIgnoreCase)
                || architecture.Equals("ARM64", StringComparison.OrdinalIgnoreCase);
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
