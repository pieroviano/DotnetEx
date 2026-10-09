// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Net35.InteropServices.Tests
{
    /// <summary>
    /// A <see cref="FactAttribute"/> that skips the test when not running on Windows.
    /// xUnit has no equivalent of NUnit's Assert.Ignore, so the decision is made when the
    /// attribute is constructed rather than inside the test body.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class WindowsOnlyFactAttribute : FactAttribute
    {
        public WindowsOnlyFactAttribute()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform != PlatformID.Win32NT && platform != PlatformID.Win32S
                && platform != PlatformID.Win32Windows && platform != PlatformID.WinCE)
            {
                Skip = "Windows-only test.";
            }
        }
    }
}
