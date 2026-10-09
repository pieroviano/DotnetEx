// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Collections.Generic;
using System.Threading;
using shimsys = shim::System;

namespace Net35.MsCorLib.Tests
{
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

        [Fact]
        public void Constructor_NullHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new shimsys.Progress<int>(null);
            });
        }

        [Fact]
        public void Report_InvokesTheConstructorHandler()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                int received = 0;
                shimsys.Progress<int> progress = new shimsys.Progress<int>(delegate (int value)
                {
                    received = value;
                });
                ((shimsys.IProgress<int>)progress).Report(42);
                Assert.Equal(42, received);
                Assert.Equal(1, context.PostCount);
            });
        }

        [Fact]
        public void Report_RaisesTheProgressChangedEvent()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                object sender = null;
                int received = 0;
                shimsys.Progress<int> progress = new shimsys.Progress<int>();
                progress.ProgressChanged += delegate (object s, int value)
                {
                    sender = s;
                    received = value;
                };
                ((shimsys.IProgress<int>)progress).Report(7);
                Assert.Same(progress, sender);
                Assert.Equal(7, received);
            });
        }

        [Fact]
        public void Report_InvokesBothTheHandlerAndTheEvent()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                List<string> calls = new List<string>();
                shimsys.Progress<int> progress = new shimsys.Progress<int>(delegate
                {
                    calls.Add("handler");
                });
                progress.ProgressChanged += delegate
                {
                    calls.Add("event");
                };
                ((shimsys.IProgress<int>)progress).Report(1);
                Assert.Equal(new string[] { "handler", "event" }, calls);
            });
        }

        [Fact]
        public void Report_WithNoHandlerAndNoSubscriber_DoesNotPost()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                shimsys.Progress<int> progress = new shimsys.Progress<int>();
                ((shimsys.IProgress<int>)progress).Report(1);
                Assert.Equal(0, context.PostCount);
            });
        }

        [Fact]
        public void Report_AfterUnsubscribing_OnlyCallsTheRemainingHandler()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                int eventCalls = 0;
                int handlerCalls = 0;
                shimsys.Progress<int> progress = new shimsys.Progress<int>(delegate
                {
                    handlerCalls++;
                });
                EventHandlerEx<int> subscriber = delegate
                {
                    eventCalls++;
                };
                progress.ProgressChanged += subscriber;
                ((shimsys.IProgress<int>)progress).Report(1);
                progress.ProgressChanged -= subscriber;
                ((shimsys.IProgress<int>)progress).Report(2);
                Assert.Equal(1, eventCalls);
                Assert.Equal(2, handlerCalls);
            });
        }

        [Fact]
        public void Report_MultipleTimes_DeliversEveryValueInOrder()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                List<int> received = new List<int>();
                shimsys.IProgress<int> progress = new shimsys.Progress<int>(delegate (int value)
                {
                    received.Add(value);
                });
                progress.Report(1);
                progress.Report(2);
                progress.Report(3);
                Assert.Equal(new int[] { 1, 2, 3 }, received);
            });
        }

        [Fact]
        public void Report_WithAReferenceTypePayload_PassesTheInstanceThrough()
        {
            WithInlineContext(delegate (InlineSynchronizationContext context)
            {
                string sent = "payload";
                string received = null;
                shimsys.IProgress<string> progress = new shimsys.Progress<string>(delegate (string value)
                {
                    received = value;
                });
                progress.Report(sent);
                Assert.Same(sent, received);
            });
        }

        [Fact]
        public void Report_WithoutASynchronizationContext_RunsOnTheThreadPool()
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                using (ManualResetEvent signal = new ManualResetEvent(false))
                {
                    int received = 0;
                    shimsys.IProgress<int> progress = new shimsys.Progress<int>(delegate (int value)
                    {
                        received = value;
                        signal.Set();
                    });
                    progress.Report(5);
                    Assert.True(signal.WaitOne(10000, false), "the progress callback was never invoked");
                    Assert.Equal(5, received);
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        [Fact]
        public void Report_UsesTheContextCapturedAtConstruction()
        {
            InlineSynchronizationContext constructionContext = new InlineSynchronizationContext();
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(constructionContext);
                int received = 0;
                shimsys.IProgress<int> progress = new shimsys.Progress<int>(delegate (int value)
                {
                    received = value;
                });
                // Swap the ambient context after construction; the captured one must still be used.
                InlineSynchronizationContext laterContext = new InlineSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(laterContext);
                progress.Report(11);
                Assert.Equal(11, received);
                Assert.Equal(1, constructionContext.PostCount);
                Assert.Equal(0, laterContext.PostCount);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}