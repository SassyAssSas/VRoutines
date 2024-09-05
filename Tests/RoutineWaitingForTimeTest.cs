using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Violoncello.Routines.Tests {
    public class RoutineWaitingForTimeTest {
        private const int IterationsAmount = 15;
        private const float WaitSeconds = 0.35f;

        private const float MaxErrorMargin = 0.15f;

        private const float DefaultTimeScale = 1f;
        private const float HalfTimeScale = DefaultTimeScale * 0.5f;
        private const float TwiceTimeScale = DefaultTimeScale * 2f;

        [UnityTest]
        public IEnumerator WaitForSecondsDefaultTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            var timing = Routine.WaitForSeconds(WaitSeconds);

            yield return RunWaitingAccuracyTest(timing, DefaultTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsHalfTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(Routine.WaitForSeconds(WaitSeconds), HalfTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsTwiceTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(Routine.WaitForSeconds(WaitSeconds), TwiceTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsRealtimeDefaultTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(Routine.WaitForSecondsRealtime(WaitSeconds), DefaultTimeScale, TimeMode.Real, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsRealtimeCustomTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(Routine.WaitForSecondsRealtime(WaitSeconds), HalfTimeScale, TimeMode.Real, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        private IEnumerator RunWaitingAccuracyTest(Routine timing, float timeScale, TimeMode timeMode, HeapContainer<bool> resultBuffer, HeapContainer<string> messageBuffer) {
            var monoTest = new MonoBehaviourTest<MonoWaitingAccuracyTest>();

            monoTest.component.Initialize(timing, IterationsAmount, WaitSeconds, MaxErrorMargin, timeScale, timeMode);

            yield return monoTest;

            resultBuffer.Value = monoTest.component.Success;
            messageBuffer.Value = monoTest.component.Message;
        }

        private class MonoWaitingAccuracyTest : MonoBehaviour, IMonoBehaviourTest {
            public bool IsTestFinished { get; private set; }
            public bool Success { get; set; }
            public string Message { get; private set; }

            private Routine _timing;
            private int _iterationsAmount;
            private float _rawWaitSeconds;
            private float _maxErrorMarginSeconds;
            private float _timeScale;
            private TimeMode _timeMode;

            private bool isInitialized;

            public void Initialize(Routine timing, int iterationsAmount, float waitSeconds, float maxErrorMargin, float timeScale, TimeMode timeMode) {
                _timing = timing;
                _iterationsAmount = iterationsAmount;
                _rawWaitSeconds = waitSeconds;
                _maxErrorMarginSeconds = maxErrorMargin;
                _timeScale = timeScale;
                _timeMode = timeMode;
            }

            public void Start() {
                Routine.Run(NotMoron);
            }

            private IEnumerator<Routine> NotMoron() {
                var previousTime = Time.time;
                var stopwatch = new Stopwatch();

                for (int i = 0; i < _iterationsAmount; i++) {
                    stopwatch.Restart();

                    yield return _timing.Clone();

                    stopwatch.Stop();

                    var currentTime = Time.time;
                    var perfectTime = previousTime + _rawWaitSeconds;

                    var difference = GetAbsoluteDifference(currentTime, perfectTime);

                    if (difference > _maxErrorMarginSeconds) {
                        Message = $"TimeScale:{Time.timeScale}\n" +
                                    $"Previous Time: {previousTime}\n" +
                                    $"Current Time: {currentTime}\n" +
                                    $"Perfect time: {perfectTime}\n" +
                                    $"Difference: {difference}\n" +
                                    $"Stopwatch: {stopwatch.ElapsedMilliseconds}";

                        IsTestFinished = true;

                        yield break;
                    }

                    previousTime = currentTime;
                }

                Success = true;
                IsTestFinished = true;
            }

            private float GetAbsoluteDifference(float a, float b) {
                return Mathf.Abs(a - b);
            }
        }

        public enum TimeMode {
            Scaled,
            Real
        }
    }
}
