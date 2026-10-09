// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Net35.MsCorLib.Tests
{
    public sealed class EnvironmentExTests
    {
        [Fact]
        public void Is64BitProcess_MatchesThePointerWidth()
        {
            Assert.Equal(IntPtr.Size == 8, EnvironmentEx.Is64BitProcess);
        }

        [Fact]
        public void Is64BitOperatingSystem_IsTrueWheneverTheProcessIs64Bit()
        {
            // A 64-bit process implies a 64-bit OS; the converse is not true.
            if (EnvironmentEx.Is64BitProcess)
            {
                Assert.True(EnvironmentEx.Is64BitOperatingSystem);
            }
        }

        [WindowsOnlyFact]
        public void Is64BitOperatingSystem_AgreesWithTheProcessorArchitectureVariables()
        {
            // On Windows a WOW64 process reports the real machine architecture in
            // PROCESSOR_ARCHITEW6432, and 32-bit-only machines set neither to a 64-bit value.
            string architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            string architectureWow = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
            bool looksSixtyFourBit = Is64Bit(architecture) || Is64Bit(architectureWow);
            Assert.Equal(looksSixtyFourBit, EnvironmentEx.Is64BitOperatingSystem);
        }

        [Fact]
        public void Is64BitFlags_AreStable()
        {
            Assert.Equal(EnvironmentEx.Is64BitProcess, EnvironmentEx.Is64BitProcess);
            Assert.Equal(EnvironmentEx.Is64BitOperatingSystem, EnvironmentEx.Is64BitOperatingSystem);
        }

        private static bool Is64Bit(string architecture)
        {
            if (string.IsNullOrEmpty(architecture))
            {
                return false;
            }

            return architecture.Equals("AMD64", StringComparison.OrdinalIgnoreCase) || architecture.Equals("IA64", StringComparison.OrdinalIgnoreCase) || architecture.Equals("ARM64", StringComparison.OrdinalIgnoreCase);
        }
    }
}