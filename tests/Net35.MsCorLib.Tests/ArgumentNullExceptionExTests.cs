// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="ArgumentNullExceptionEx"/>.</summary>
    [TestFixture]
    public sealed class ArgumentNullExceptionExTests
    {
        [Test]
        public void ThrowIfNull_WithANonNullArgument_DoesNothing()
        {
            Assert.DoesNotThrow(delegate { ArgumentNullExceptionEx.ThrowIfNull("value"); });
            Assert.DoesNotThrow(delegate { ArgumentNullExceptionEx.ThrowIfNull(new object(), "parameter"); });
        }

        [Test]
        public void ThrowIfNull_WithANullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { ArgumentNullExceptionEx.ThrowIfNull((object)null); });
        }

        [Test]
        public void ThrowIfNull_CarriesTheParameterName()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                delegate { ArgumentNullExceptionEx.ThrowIfNull((object)null, "argument"); });

            Assert.AreEqual("argument", exception.ParamName);
        }

        [Test]
        public void ThrowIfNull_WithoutAParameterName_LeavesItUnset()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                delegate { ArgumentNullExceptionEx.ThrowIfNull((object)null); });

            Assert.IsNull(exception.ParamName);
        }

        [Test]
        public void ThrowIfNull_WithABoxedValueType_DoesNotThrow()
        {
            Assert.DoesNotThrow(delegate { ArgumentNullExceptionEx.ThrowIfNull(0); });
            Assert.DoesNotThrow(delegate { ArgumentNullExceptionEx.ThrowIfNull(false); });
        }

        [Test]
        public void InnerExceptionConstructor_KeepsTheMessageAndInnerException()
        {
            // The only constructor whose forwarding is correct: ArgumentNullException(message, innerException).
            InvalidOperationException inner = new InvalidOperationException("inner");
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("outer message", inner);

            Assert.AreEqual("outer message", exception.Message);
            Assert.AreSame(inner, exception.InnerException);
        }

        // ---------------------------------------------------------------------------------------
        // The three constructors below all forward to ArgumentNullException as if its signature
        // were (message, paramName). It is actually (paramName, message), so message and parameter
        // name come out transposed. See src/mscorlib.NET35/System/ArgumentNullException.cs:13,20,32.
        // These tests pin the current behaviour so that correcting it is a deliberate, visible change.
        // ---------------------------------------------------------------------------------------

        [Test, Description("Pins a transposed-argument defect in the parameterless constructor.")]
        public void DefaultConstructor_PutsTheGenericMessageIntoParamName()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx();

            Assert.IsNotEmpty(exception.Message);
            Assert.AreEqual("Value cannot be null.", exception.ParamName);
        }

        [Test, Description("Pins a transposed-argument defect in the (paramName) constructor.")]
        public void ParamNameConstructor_PutsTheParameterNameIntoTheMessage()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("argument");

            Assert.AreEqual("Value cannot be null.", exception.ParamName);
            StringAssert.Contains("argument", exception.Message);
        }

        [Test, Description("Pins a transposed-argument defect in the (paramName, message) constructor.")]
        public void ParamNameAndMessageConstructor_SwapsItsArguments()
        {
            ArgumentNullExceptionEx exception = new ArgumentNullExceptionEx("theParamName", "the message");

            Assert.AreEqual("the message", exception.ParamName);
            StringAssert.Contains("theParamName", exception.Message);
        }

        [Test]
        public void ThrowIfNull_IsUnaffectedByTheConstructorTransposition()
        {
            // ThrowIfNull bypasses the constructors above and throws the BCL type directly,
            // so its parameter name is correct.
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                delegate { ArgumentNullExceptionEx.ThrowIfNull((object)null, "wellNamed"); });

            Assert.AreEqual("wellNamed", exception.ParamName);
        }

        [Test]
        public void IsAnArgumentNullException()
        {
            Assert.IsTrue(new ArgumentNullExceptionEx() is ArgumentNullException);
            Assert.IsTrue(new ArgumentNullExceptionEx() is ArgumentException);
        }
    }
}
