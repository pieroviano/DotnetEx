﻿using System.Linq;
using System.Reflection;

namespace System.Runtime.InteropServices
{
    public static partial class RuntimeInformation
    {
        private const string FrameworkName = ".NET";
        private static string? s_frameworkDescription;

        public static string FrameworkDescription
        {
            get
            {
                if (s_frameworkDescription == null)
                {
                    string versionString = ((AssemblyInformationalVersionAttribute)typeof(object).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).FirstOrDefault()).InformationalVersion;

                    // Strip the git hash if there is one
                    int plusIndex = versionString.IndexOf('+');
                    if (plusIndex != -1)
                    {
                        versionString = versionString.Substring(0, plusIndex);
                    }

                    s_frameworkDescription = !string.IsNullOrEmpty(versionString.Trim()) ? $"{FrameworkName} {versionString}" : FrameworkName;
                }

                return s_frameworkDescription;
            }
        }

        /// <summary>
        /// Returns an opaque string that identifies the platform on which an app is running.
        /// </summary>
        /// <remarks>
        /// The property returns a string that identifies the operating system, typically including version,
        /// and processor architecture of the currently executing process.
        /// Since this string is opaque, it is not recommended to parse the string into its constituent parts.
        ///
        /// For more information, see https://docs.microsoft.com/dotnet/core/rid-catalog.
        /// </remarks>
        public static string RuntimeIdentifier
        {
            get
            {
                return OperatingSystemEx.OSPlatform switch
                {
                    "WINDOWS" => $"win-{OSArchitecture.ToString().ToLower()}",
                    "LINUX" => $"linux-{OSArchitecture.ToString().ToLower()}",
                    "OSX" => $"osx-{OSArchitecture.ToString().ToLower()}",
                    _ => $"unknown-{OSArchitecture.ToString().ToLower()}",
                };
            }
        }

        /// <summary>
        /// Indicates whether the current application is running on the specified platform.
        /// </summary>
        public static bool IsOSPlatform(OSPlatform osPlatform) => OperatingSystemEx.IsOSPlatform(osPlatform.Name);
    }
}