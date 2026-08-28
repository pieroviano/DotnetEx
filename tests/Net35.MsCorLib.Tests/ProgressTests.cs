// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="Progress{T}"/> and <see cref="IProgress{T}"/>.</summary>
    [TestFixture]
    public sealed class ProgressTests
    {
        /// <summary>A synchronization context that runs posted callbacks inline, making reports deterministic.</summary>
        private sealed class InlineSynchronizationContext : SynchronizationContext
        {
            public int PostCount { get; private set; }

            public override void Post(SendOrPostCallback d, object state)
            {
                PostCount++;
                d(state);
            }
        }

        /// <summary>Installs a synchronization context for the duration of a block and restores the previous one.</summary>
        private static void WithInlineContext(Action<InlineSynchronizationContext> body)
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            InlineSynchronizationContext context = new InlineSynchronizationContext();

            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                body(context);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Test]
        public void Constructor_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Progress<int>(null); });
        }

        [Test]
        public void Report_InvokesTheConstructorHandler()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                int received = 0;
                Progress<int> progress = new Progress<int>(delegate (int value) { received = value; });

                ((IProgress<int>)progress).Report(42);

                Assert.AreEqual(42, received);
                Assert.AreEqual(1, context.PostCount);
            });
        }

        [Test]
        public void Report_RaisesTheProgressChangedEvent()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                object sender = null;
                int received = 0;

                Progress<int> progress = new Progress<int>();
                progress.ProgressChanged += delegate (object s, int value)
                {
                    sender = s;
                    received = value;
                };

                ((IProgress<int>)progress).Report(7);

                Assert.AreSame(progress, sender);
                Assert.AreEqual(7, received);
            });
        }

        [Test]
        public void Report_InvokesBothTheHandlerAndTheEvent()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                List<string> calls = new List<string>();

                Progress<int> progress = new Progress<int>(delegate { calls.Add("handler"); });
                progress.ProgressChanged += delegate { calls.Add("event"); };

                ((IProgress<int>)progress).Report(1);

                CollectionAssert.AreEqual(new string[] { "handler", "event" }, calls);
            });
        }

        [Test]
        public void Report_WithNoHandlerAndNoSubscriber_DoesNotPost()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                Progress<int> progress = new Progress<int>();

                ((IProgress<int>)progress).Report(1);

                // OnReport short-circuits when there is nothing to call.
                Assert.AreEqual(0, context.PostCount);
            });
        }

        [Test]
        public void Report_AfterUnsubscribing_OnlyCallsTheRemainingHandler()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                int eventCalls = 0;
                int handlerCalls = 0;

                Progress<int> progress = new Progress<int>(delegate { handlerCalls++; });
                EventHandlerEx<int> subscriber = delegate { eventCalls++; };

                progress.ProgressChanged += subscriber;
                ((IProgress<int>)progress).Report(1);

                progress.ProgressChanged -= subscriber;
                ((IProgress<int>)progress).Report(2);

                Assert.AreEqual(1, eventCalls);
                Assert.AreEqual(2, handlerCalls);
            });
        }

        [Test]
        public void Report_MultipleTimes_DeliversEveryValueInOrder()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                List<int> received = new List<int>();
                IProgress<int> progress = new Progress<int>(delegate (int value) { received.Add(value); });

                progress.Report(1);
                progress.Report(2);
                progress.Report(3);

                CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, received);
            });
        }

        [Test]
        public void Report_WithAReferenceTypePayload_PassesTheInstanceThrough()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                string sent = "payload";
                string received = null;

                IProgress<string> progress = new Progress<string>(delegate (string value) { received = value; });
                progress.Report(sent);

                Assert.AreSame(sent, received);
            });
        }

        [Test]
        public void Report_WithoutASynchronizationContext_RunsOnTheThreadPool()
        {
            SynchronizationContext previous = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(null);

                using (ManualResetEvent signal = new ManualResetEvent(false))
                {
                    int received = 0;

                    IProgress<int> progress = new Progress<int>(delegate (int value)
                    {
                        received = value;
                        signal.Set();
                    });

                    progress.Report(5);

                    Assert.IsTrue(signal.WaitOne(10000, false), "the progress callback was never invoked");
                    Assert.AreEqual(5, received);
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Test]
        public void Report_UsesTheContextCapturedAtConstruction()
        {
            InlineSynchronizationContext constructionContext = new InlineSynchronizationContext();
            SynchronizationContext previous = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(constructionContext);

                int received = 0;
                IProgress<int> progress = new Progress<int>(delegate (int value) { received = value; });

                // Swap the ambient context after construction; the captured one must still be used.
                InlineSynchronizationContext laterContext = new InlineSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(laterContext);

                progress.Report(11);

                Assert.AreEqual(11, received);
                Assert.AreEqual(1, constructionContext.PostCount);
                Assert.AreEqual(0, laterContext.PostCount);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
