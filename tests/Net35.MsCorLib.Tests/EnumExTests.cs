// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="EnumEx.TryParse{TEnum}(string, out TEnum)"/>.</summary>
    [TestFixture]
    public sealed class EnumExTests
    {
        private enum Colour
        {
            None = 0,
            Red = 1,
            Green = 2,
            DarkBlue = 3
        }

        [Flags]
        private enum Access
        {
            None = 0,
            Read = 1,
            Write = 2
        }

        [Test]
        public void TryParse_ExactName_Succeeds()
        {
            Colour result;

            Assert.IsTrue(EnumEx.TryParse<Colour>("Green", out result));
            Assert.AreEqual(Colour.Green, result);
        }

        [Test]
        public void TryParse_ZeroValuedName_Succeeds()
        {
            Colour result;

            Assert.IsTrue(EnumEx.TryParse<Colour>("None", out result));
            Assert.AreEqual(Colour.None, result);
        }

        [Test]
        public void TryParse_UnknownName_ReturnsFalseAndDefault()
        {
            Colour result;

            Assert.IsFalse(EnumEx.TryParse<Colour>("Purple", out result));
            Assert.AreEqual(Colour.None, result);
        }

        [Test]
        public void TryParse_EmptyString_ReturnsFalse()
        {
            Colour result;

            Assert.IsFalse(EnumEx.TryParse<Colour>(string.Empty, out result));
        }

        [Test]
        public void TryParse_DifferentCase_SucceedsViaTheCaseInsensitiveFallback()
        {
            Colour result;

            // The default overload passes ignoreCase:false, but the name scan is ordinal-ignore-case,
            // so a differently cased name still resolves.
            Assert.IsTrue(EnumEx.TryParse<Colour>("green", out result));
            Assert.AreEqual(Colour.Green, result);
        }

        [Test]
        public void TryParse_WithIgnoreCase_SucceedsForMixedCase()
        {
            Colour result;

            Assert.IsTrue(EnumEx.TryParse<Colour>("dArKbLuE", true, out result));
            Assert.AreEqual(Colour.DarkBlue, result);
        }

        [Test]
        public void TryParse_ReplacesSpacesWithUnderscores()
        {
            Access result;

            // A space is normalised to '_' before matching; no member contains one, so this fails cleanly.
            Assert.IsFalse(EnumEx.TryParse<Access>("Read Write", out result));
            Assert.AreEqual(Access.None, result);
        }

        [Test]
        public void TryParse_FlagsEnumSingleMember_Succeeds()
        {
            Access result;

            Assert.IsTrue(EnumEx.TryParse<Access>("Write", out result));
            Assert.AreEqual(Access.Write, result);
        }

        [Test, Description("Documents a divergence: Enum.TryParse in the BCL accepts the numeric form.")]
        public void TryParse_NumericString_ReturnsFalse()
        {
            Colour result;

            // Enum.IsDefined only matches names for a string argument, and the fallback scan compares
            // against names too, so "1" never resolves to Colour.Red.
            Assert.IsFalse(EnumEx.TryParse<Colour>("1", out result));
            Assert.AreEqual(Colour.None, result);
        }

        [Test]
        public void TryParse_BclEnumType_Succeeds()
        {
            DayOfWeek result;

            Assert.IsTrue(EnumEx.TryParse<DayOfWeek>("Wednesday", out result));
            Assert.AreEqual(DayOfWeek.Wednesday, result);
        }

        [Test, Description("Guards the documented null-handling gap in EnumEx.TryParse.")]
        public void TryParse_NullValue_ThrowsNullReference()
        {
            // value.Replace(...) runs before any null check, so a null argument faults rather than
            // returning false the way Enum.TryParse does. Pinned so a fix is a deliberate change.
            Colour result = Colour.None;
            Assert.Throws<NullReferenceException>(delegate { EnumEx.TryParse<Colour>(null, out result); });
        }
    }
}
