using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace Violoncello.Routines.Tests {
    public static class RoutineWaitForConditionTest {
        private const int IterationsAmount = 5;

        [UnityTest]
        public static IEnumerator WaitUntil() {
            var complete = new HeapContainer<bool>();
            var success = new HeapContainer<bool>();
            var message = new HeapContainer<string>();

            Routine.Run(WaitUntilRoutineTest(complete, success, message));

            yield return new WaitUntil(() => complete);

            Assert.IsTrue(success, message);
        }

        [UnityTest]
        public static IEnumerator WaitWhile() {
            var complete = new HeapContainer<bool>();
            var success = new HeapContainer<bool>();
            var message = new HeapContainer<string>();

            Routine.Run(WaitWhileRoutineTest(complete, success, message));

            yield return new WaitUntil(() => complete);

            Assert.IsTrue(success, message);
        }

        private static IEnumerator<Routine> WaitUntilRoutineTest(HeapContainer<bool> complete, HeapContainer<bool> success, HeapContainer<string> message) {
            success.Value = true;

            var waitSeconds = 0.2f;

            var stopwatch = new Stopwatch();

            for (int i = 1; i <= IterationsAmount; i++) {
                var thresholdMilliseconds = waitSeconds * 1000f * i;
                var nextThresholdMilliseconds = (waitSeconds + 0.2f) * 1000f * i;

                stopwatch.Restart();

                yield return Routine.WaitUntil(() => stopwatch.ElapsedMilliseconds > thresholdMilliseconds);

                stopwatch.Stop();

                success.Value &= stopwatch.ElapsedMilliseconds >= thresholdMilliseconds;
                success.Value &= stopwatch.ElapsedMilliseconds <= nextThresholdMilliseconds;

                if (!success) {
                    complete.Value = true;

                    message.Value = $"Left border: {thresholdMilliseconds}ms\n" +
                                    $"Right border: {nextThresholdMilliseconds}ms\n" +
                                    $"Elapsed: {stopwatch.ElapsedMilliseconds}ms\n";

                    yield break;
                }
            }

            complete.Value = true;
        }

        private static IEnumerator<Routine> WaitWhileRoutineTest(HeapContainer<bool> complete, HeapContainer<bool> success, HeapContainer<string> message) {
            success.Value = true;

            var waitSeconds = 0.2f;

            var stopwatch = new Stopwatch();

            for (int i = 1; i <= IterationsAmount; i++) {
                var thresholdMilliseconds = waitSeconds * 1000f * i;
                var nextThresholdMilliseconds = (waitSeconds + 0.2f) * 1000f * i;

                stopwatch.Restart();

                yield return Routine.WaitWhile(() => stopwatch.ElapsedMilliseconds < thresholdMilliseconds);

                stopwatch.Stop();

                success.Value &= stopwatch.ElapsedMilliseconds >= thresholdMilliseconds;
                success.Value &= stopwatch.ElapsedMilliseconds <= nextThresholdMilliseconds;

                if (!success) {
                    complete.Value = true;

                    message.Value = $"Left border: {thresholdMilliseconds}ms\n" +
                                    $"Right border: {nextThresholdMilliseconds}ms\n" +
                                    $"Elapsed: {stopwatch.ElapsedMilliseconds}ms\n";

                    yield break;
                }
            }

            complete.Value = true;
        }
    }
}
