// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Diagnostics;
using System.Threading;
using SpinWait = shim::System.Threading.SpinWait;
using Volatile = shim::System.Threading.Volatile;

namespace Net35.MsCorLib.Tests
{
    public sealed class SpinWaitTests
    {
        /// <summary>The threshold at which SpinOnce switches from busy-spinning to yielding.</summary>
        private const int YieldThreshold = 10;
        [Fact]
        public void NewInstance_HasNotSpun()
        {
            SpinWait spinner = new SpinWait();
            Assert.Equal(0, spinner.Count);
        }

        [Fact]
        public void SpinOnce_IncrementsTheCount()
        {
            SpinWait spinner = new SpinWait();
            spinner.SpinOnce();
            Assert.Equal(1, spinner.Count);
            spinner.SpinOnce();
            Assert.Equal(2, spinner.Count);
        }

        [Fact]
        public void Reset_ClearsTheCount()
        {
            SpinWait spinner = new SpinWait();
            for (int i = 0; i < 5; i++)
            {
                spinner.SpinOnce();
            }

            Assert.NotEqual(0, spinner.Count);
            spinner.Reset();
            Assert.Equal(0, spinner.Count);
            Assert.True(!spinner.NextSpinWillYield || Environment.ProcessorCount == 1);
        }

        [Fact]
        public void NextSpinWillYield_BecomesTrueOnceTheThresholdIsPassed()
        {
            SpinWait spinner = new SpinWait();
            for (int i = 0; i < YieldThreshold; i++)
            {
                spinner.SpinOnce();
            }

            Assert.True(spinner.NextSpinWillYield);
        }

        [Fact]
        public void NextSpinWillYield_IsFalseBeforeTheThresholdOnAMultiProcessorMachine()
        {
            if (Environment.ProcessorCount == 1)
            {
                // On a single-processor machine SpinOnce always yields, by design.
                return;
            }

            SpinWait spinner = new SpinWait();
            Assert.False(spinner.NextSpinWillYield);
        }

        [Fact]
        public void SpinOnce_WithSleep1ThresholdBelowMinusOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait spinner = new SpinWait();
                spinner.SpinOnce(-2);
            });
        }

        [Fact]
        public void SpinOnce_WithSleep1ThresholdOfMinusOne_IsAccepted()
        {
            SpinWait spinner = new SpinWait();
            spinner.SpinOnce(-1);
            Assert.Equal(1, spinner.Count);
        }

        [Fact]
        public void SpinOnce_WithAnExplicitThreshold_StillSpins()
        {
            SpinWait spinner = new SpinWait();
            spinner.SpinOnce(YieldThreshold * 2);
            Assert.Equal(1, spinner.Count);
        }

        [Fact]
        public void SpinOnce_ManyTimes_KeepsCounting()
        {
            SpinWait spinner = new SpinWait();
            // Walks past the yield threshold and the Sleep(1) threshold, exercising every branch
            // of SpinOnceCore.
            for (int i = 0; i < 25; i++)
            {
                spinner.SpinOnce();
            }

            Assert.Equal(25, spinner.Count);
            Assert.True(spinner.NextSpinWillYield);
        }

        [Fact]
        public void SpinUntil_ConditionAlreadyTrue_ReturnsImmediately()
        {
            Assert.True(SpinWait.SpinUntil(delegate
            {
                return true;
            }, 0));
        }

        [Fact]
        public void SpinUntil_ConditionNeverTrueWithZeroTimeout_ReturnsFalse()
        {
            Assert.False(SpinWait.SpinUntil(delegate
            {
                return false;
            }, 0));
        }

        [Fact]
        public void SpinUntil_ConditionNeverTrue_TimesOut()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool satisfied = SpinWait.SpinUntil(delegate
            {
                return false;
            }, 100);
            stopwatch.Stop();
            Assert.False(satisfied);
            Assert.True(stopwatch.ElapsedMilliseconds >= 50, "it should have spun for roughly the timeout");
        }

        [Fact]
        public void SpinUntil_ConditionBecomesTrue_ReturnsTrue()
        {
            int calls = 0;
            bool satisfied = SpinWait.SpinUntil(delegate
            {
                calls++;
                return calls >= 5;
            }, 10000);
            Assert.True(satisfied);
            Assert.True(calls >= 5);
        }

        [Fact]
        public void SpinUntil_WithoutTimeout_ReturnsOnceTheConditionHolds()
        {
            int calls = 0;
            SpinWait.SpinUntil(delegate
            {
                calls++;
                return calls >= 3;
            });
            Assert.True(calls >= 3);
        }

        [Fact]
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
            Assert.True(SpinWait.SpinUntil(delegate
            {
                return Volatile.Read(ref ready);
            }, 10000));
            worker.Join(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void SpinUntil_NullCondition_Throws()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                SpinWait.SpinUntil(null, 100);
            });
        }

        [Fact]
        public void SpinUntil_NegativeTimeout_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait.SpinUntil(delegate
                {
                    return true;
                }, -2);
            });
        }

        [Fact]
        public void SpinUntil_WithTimeSpan_Works()
        {
            Assert.True(SpinWait.SpinUntil(delegate
            {
                return true;
            }, TimeSpan.FromSeconds(1)));
            Assert.False(SpinWait.SpinUntil(delegate
            {
                return false;
            }, TimeSpan.Zero));
        }

        [Fact]
        public void SpinUntil_WithAnOutOfRangeTimeSpan_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait.SpinUntil(delegate
                {
                    return true;
                }, TimeSpan.FromMilliseconds(-2));
            });
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                SpinWait.SpinUntil(delegate
                {
                    return true;
                }, TimeSpan.FromDays(30));
            });
        }

        [Fact]
        public void SpinUntil_WithAnInfiniteTimeSpan_IsAccepted()
        {
            Assert.True(SpinWait.SpinUntil(delegate
            {
                return true;
            }, TimeSpan.FromMilliseconds(Timeout.Infinite)));
        }
    }
}