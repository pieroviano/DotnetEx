// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;

namespace Net35.MsCorLib.Tests
{
    /// <summary>Tests for <see cref="SpinWait"/>.</summary>
    [TestFixture]
    public sealed class SpinWaitTests
    {
        /// <summary>The threshold at which SpinOnce switches from busy-spinning to yielding.</summary>
        private const int YieldThreshold = 10;

        [Test]
        public void NewInstance_HasNotSpun()
        {
            SpinWait spinner = new SpinWait();

            Assert.AreEqual(0, spinner.Count);
        }

        [Test]
        public void SpinOnce_IncrementsTheCount()
        {
            SpinWait spinner = new SpinWait();

            spinner.SpinOnce();
            Assert.AreEqual(1, spinner.Count);

            spinner.SpinOnce();
            Assert.AreEqual(2, spinner.Count);
        }

        [Test]
        public void Reset_ClearsTheCount()
        {
            SpinWait spinner = new SpinWait();

            for (int i = 0; i < 5; i++)
            {
                spinner.SpinOnce();
            }

            Assert.AreNotEqual(0, spinner.Count);

            spinner.Reset();
            Assert.AreEqual(0, spinner.Count);

            // After a reset the next spin only yields if the machine is single-processor.
            Assert.IsTrue(!spinner.NextSpinWillYield || Environment.ProcessorCount == 1);
        }

        [Test]
        public void NextSpinWillYield_BecomesTrueOnceTheThresholdIsPassed()
        {
            SpinWait spinner = new SpinWait();

            for (int i = 0; i < YieldThreshold; i++)
            {
                spinner.SpinOnce();
            }

            Assert.IsTrue(spinner.NextSpinWillYield);
        }

        [Test]
        public void NextSpinWillYield_IsFalseBeforeTheThresholdOnAMultiProcessorMachine()
        {
            if (Environment.ProcessorCount == 1)
            {
                // On a single-processor machine SpinOnce always yields, by design.
                return;
            }

            SpinWait spinner = new SpinWait();

            Assert.IsFalse(spinner.NextSpinWillYield);
        }

        [Test]
        public void SpinOnce_WithSleep1ThresholdBelowMinusOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait spinner = new SpinWait();
                spinner.SpinOnce(-2);
            });
        }

        [Test]
        public void SpinOnce_WithSleep1ThresholdOfMinusOne_IsAccepted()
        {
            SpinWait spinner = new SpinWait();

            Assert.DoesNotThrow(delegate { spinner.SpinOnce(-1); });
            Assert.AreEqual(1, spinner.Count);
        }

        [Test]
        public void SpinOnce_WithAnExplicitThreshold_StillSpins()
        {
            SpinWait spinner = new SpinWait();

            spinner.SpinOnce(YieldThreshold * 2);

            Assert.AreEqual(1, spinner.Count);
        }

        [Test]
        public void SpinOnce_ManyTimes_KeepsCounting()
        {
            SpinWait spinner = new SpinWait();

            // Walks past the yield threshold and the Sleep(1) threshold, exercising every branch
            // of SpinOnceCore.
            for (int i = 0; i < 25; i++)
            {
                spinner.SpinOnce();
            }

            Assert.AreEqual(25, spinner.Count);
            Assert.IsTrue(spinner.NextSpinWillYield);
        }

        [Test]
        public void SpinUntil_ConditionAlreadyTrue_ReturnsImmediately()
        {
            Assert.IsTrue(SpinWait.SpinUntil(delegate { return true; }, 0));
        }

        [Test]
        public void SpinUntil_ConditionNeverTrueWithZeroTimeout_ReturnsFalse()
        {
            Assert.IsFalse(SpinWait.SpinUntil(delegate { return false; }, 0));
        }

        [Test]
        public void SpinUntil_ConditionNeverTrue_TimesOut()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool satisfied = SpinWait.SpinUntil(delegate { return false; }, 100);

            stopwatch.Stop();
            Assert.IsFalse(satisfied);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 50, "it should have spun for roughly the timeout");
        }

        [Test]
        public void SpinUntil_ConditionBecomesTrue_ReturnsTrue()
        {
            int calls = 0;

            bool satisfied = SpinWait.SpinUntil(
                delegate
                {
                    calls++;
                    return calls >= 5;
                },
                10000);

            Assert.IsTrue(satisfied);
            Assert.IsTrue(calls >= 5);
        }

        [Test]
        public void SpinUntil_WithoutTimeout_ReturnsOnceTheConditionHolds()
        {
            int calls = 0;

            Assert.DoesNotThrow(delegate
            {
                SpinWait.SpinUntil(delegate
                {
                    calls++;
                    return calls >= 3;
                });
            });

            Assert.IsTrue(calls >= 3);
        }

        [Test]
        public void SpinUntil_WaitsForAnotherThread()
        {
            bool ready = false;

            Thread worker = new Thread(delegate ()
            {
                Thread.Sleep(50);
                Volatile.Write(ref ready, true);
            });

            worker.IsBackground = true;
            worker.Start();

            Assert.IsTrue(SpinWait.SpinUntil(delegate { return Volatile.Read(ref ready); }, 10000));
            worker.Join(TimeSpan.FromSeconds(10));
        }

        [Test]
        public void SpinUntil_NullCondition_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate { SpinWait.SpinUntil(null, 100); });
        }

        [Test]
        public void SpinUntil_NegativeTimeout_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { SpinWait.SpinUntil(delegate { return true; }, -2); });
        }

        [Test]
        public void SpinUntil_WithTimeSpan_Works()
        {
            Assert.IsTrue(SpinWait.SpinUntil(delegate { return true; }, TimeSpan.FromSeconds(1)));
            Assert.IsFalse(SpinWait.SpinUntil(delegate { return false; }, TimeSpan.Zero));
        }

        [Test]
        public void SpinUntil_WithAnOutOfRangeTimeSpan_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait.SpinUntil(delegate { return true; }, TimeSpan.FromMilliseconds(-2));
            });

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait.SpinUntil(delegate { return true; }, TimeSpan.FromDays(30));
            });
        }

        [Test]
        public void SpinUntil_WithAnInfiniteTimeSpan_IsAccepted()
        {
            Assert.IsTrue(SpinWait.SpinUntil(delegate { return true; }, TimeSpan.FromMilliseconds(Timeout.Infinite)));
        }
    }
}
