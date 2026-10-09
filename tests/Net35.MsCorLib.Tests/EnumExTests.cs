// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Net35.MsCorLib.Tests
{
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

        [Fact]
        public void TryParse_ExactName_Succeeds()
        {
            Colour result;
            Assert.True(EnumEx.TryParse<Colour>("Green", out result));
            Assert.Equal(Colour.Green, result);
        }

        [Fact]
        public void TryParse_ZeroValuedName_Succeeds()
        {
            Colour result;
            Assert.True(EnumEx.TryParse<Colour>("None", out result));
            Assert.Equal(Colour.None, result);
        }

        [Fact]
        public void TryParse_UnknownName_ReturnsFalseAndDefault()
        {
            Colour result;
            Assert.False(EnumEx.TryParse<Colour>("Purple", out result));
            Assert.Equal(Colour.None, result);
        }

        [Fact]
        public void TryParse_EmptyString_ReturnsFalse()
        {
            Colour result;
            Assert.False(EnumEx.TryParse<Colour>(string.Empty, out result));
        }

        [Fact]
        public void TryParse_DifferentCase_SucceedsViaTheCaseInsensitiveFallback()
        {
            Colour result;
            Assert.True(EnumEx.TryParse<Colour>("green", out result));
            Assert.Equal(Colour.Green, result);
        }

        [Fact]
        public void TryParse_WithIgnoreCase_SucceedsForMixedCase()
        {
            Colour result;
            Assert.True(EnumEx.TryParse<Colour>("dArKbLuE", true, out result));
            Assert.Equal(Colour.DarkBlue, result);
        }

        [Fact]
        public void TryParse_ReplacesSpacesWithUnderscores()
        {
            Access result;
            Assert.False(EnumEx.TryParse<Access>("Read Write", out result));
            Assert.Equal(Access.None, result);
        }

        [Fact]
        public void TryParse_FlagsEnumSingleMember_Succeeds()
        {
            Access result;
            Assert.True(EnumEx.TryParse<Access>("Write", out result));
            Assert.Equal(Access.Write, result);
        }

        [Fact, Trait("Description", "Documents a divergence: Enum.TryParse in the BCL accepts the numeric form.")]
        public void TryParse_NumericString_ReturnsFalse()
        {
            Colour result;
            Assert.False(EnumEx.TryParse<Colour>("1", out result));
            Assert.Equal(Colour.None, result);
        }

        [Fact]
        public void TryParse_BclEnumType_Succeeds()
        {
            DayOfWeek result;
            Assert.True(EnumEx.TryParse<DayOfWeek>("Wednesday", out result));
            Assert.Equal(DayOfWeek.Wednesday, result);
        }

        [Fact, Trait("Description", "Guards the documented null-handling gap in EnumEx.TryParse.")]
        public void TryParse_NullValue_ThrowsNullReference()
        {
            // value.Replace(...) runs before any null check, so a null argument faults rather than
            // returning false the way Enum.TryParse does. Pinned so a fix is a deliberate change.
            Colour result = Colour.None;
            Assert.Throws<NullReferenceException>(delegate
            {
                EnumEx.TryParse<Colour>(null, out result);
            });
        }
    }
}