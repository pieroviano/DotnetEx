// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Net35.MsCorLib.Tests
{
    public sealed class ArgumentNullExceptionExTests
    {
        [Fact]
        public void ThrowIfNull_WithANonNullArgument_DoesNothing()
        {
            ArgumentNullExceptionEx.ThrowIfNull("value");
            ArgumentNullExceptionEx.ThrowIfNull(new object (), "parameter");
        }

        [Fact]
        public void ThrowIfNull_WithANullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                ArgumentNullExceptionEx.ThrowIfNull((object)null);
            });
        }

        [Fact]
        public void ThrowIfNull_CarriesTheParameterName()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(delegate
            {
                ArgumentNullExceptionEx.ThrowIfNull((object)null, "argument");
            });
            Assert.Equal("argument", exception.ParamName);
        }

        [Fact]
        public void ThrowIfNull_WithoutAParameterName_LeavesItUnset()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(delegate
            {
                ArgumentNullExceptionEx.ThrowIfNull((object)null);
            });
            Assert.Null(exception.ParamName);
        }

        [Fact]
        public void ThrowIfNull_WithABoxedValueType_DoesNotThrow()
        {
            ArgumentNullExceptionEx.ThrowIfNull(0);
            ArgumentNullExceptionEx.ThrowIfNull(false);
        }

        [Fact]
        public void InnerExceptionConstructor_KeepsTheMessageAndInnerException()
        {
            // The only constructor whose forwarding is correct: ArgumentNullException(message, innerException).
            InvalidOperationException inner = new InvalidOperationException("inner");
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("outer message", inner);
            Assert.Equal("outer message", exception.Message);
            Assert.Same(inner, exception.InnerException);
        }

        // ---------------------------------------------------------------------------------------
        // The three constructors below all forward to ArgumentNullException as if its signature
        // were (message, paramName). It is actually (paramName, message), so message and parameter
        // name come out transposed. See src/mscorlib.NET35/System/ArgumentNullException.cs:13,20,32.
        // These tests pin the current behaviour so that correcting it is a deliberate, visible change.
        // ---------------------------------------------------------------------------------------
        [Fact, Trait("Description", "Pins a transposed-argument defect in the parameterless constructor.")]
        public void DefaultConstructor_PutsTheGenericMessageIntoParamName()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx();
            Assert.NotEmpty(exception.Message);
            Assert.Equal("Value cannot be null.", exception.ParamName);
        }

        [Fact, Trait("Description", "Pins a transposed-argument defect in the (paramName) constructor.")]
        public void ParamNameConstructor_PutsTheParameterNameIntoTheMessage()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("argument");
            Assert.Equal("Value cannot be null.", exception.ParamName);
            Assert.Contains("argument", exception.Message);
        }

        [Fact, Trait("Description", "Pins a transposed-argument defect in the (paramName, message) constructor.")]
        public void ParamNameAndMessageConstructor_SwapsItsArguments()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("theParamName", "the message");
            Assert.Equal("the message", exception.ParamName);
            Assert.Contains("theParamName", exception.Message);
        }

        [Fact]
        public void ThrowIfNull_IsUnaffectedByTheConstructorTransposition()
        {
            // ThrowIfNull bypasses the constructors above and throws the BCL type directly,
            // so its parameter name is correct.
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(delegate
            {
                ArgumentNullExceptionEx.ThrowIfNull((object)null, "wellNamed");
            });
            Assert.Equal("wellNamed", exception.ParamName);
        }

        [Fact]
        public void IsAnArgumentNullException()
        {
            Assert.True(new ArgumentNullExceptionEx() is ArgumentNullException);
            Assert.True(new ArgumentNullExceptionEx() is ArgumentException);
        }
    }
}