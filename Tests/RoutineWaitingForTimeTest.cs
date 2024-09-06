using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Violoncello.Routines.Tests {
    public class RoutineWaitingForTimeTest {
        private const int IterationsAmount = 5;
        private const float WaitSeconds = 1f;

        private const float MaxErrorMargin = 0.15f;

        private const float DefaultTimeScale = 1f;
        private const float HalfTimeScale = DefaultTimeScale * 0.5f;
        private const float TwiceTimeScale = DefaultTimeScale * 2f;

        [UnityTest]
        public IEnumerator WaitForSecondsDefaultTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(() => Routine.WaitForSeconds(WaitSeconds), DefaultTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsHalfTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(() => Routine.WaitForSeconds(WaitSeconds), HalfTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsTwiceTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(() => Routine.WaitForSeconds(WaitSeconds), TwiceTimeScale, TimeMode.Scaled, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsRealtimeDefaultTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(() => Routine.WaitForSecondsRealtime(WaitSeconds), DefaultTimeScale, TimeMode.Real, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        [UnityTest]
        public IEnumerator WaitForSecondsRealtimeCustomTimeScale() {
            var resultBuffer = new HeapContainer<bool>();
            var errorMessageBuffer = new HeapContainer<string>();

            yield return RunWaitingAccuracyTest(() => Routine.WaitForSecondsRealtime(WaitSeconds), HalfTimeScale, TimeMode.Real, resultBuffer, errorMessageBuffer);

            Assert.IsTrue(resultBuffer, errorMessageBuffer);
        }

        private IEnumerator RunWaitingAccuracyTest(Func<Routine> timing, float timeScale, TimeMode timeMode, HeapContainer<bool> resultBuffer, HeapContainer<string> messageBuffer) {
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

            private Func<Routine> _timingCreator;
            private int _iterationsAmount;
            private float _waitSeconds;
            private float _maxErrorMarginSeconds;
            private float _timeScale;
            private TimeMode _timeMode;

            public void Initialize(Func<Routine> timing, int iterationsAmount, float waitSeconds, float maxErrorMargin, float timeScale, TimeMode timeMode) {
                _timingCreator = timing;
                _iterationsAmount = iterationsAmount;
                _waitSeconds = waitSeconds;
                _maxErrorMarginSeconds = maxErrorMargin;
                _timeScale = timeScale;
                _timeMode = timeMode;
            }

            public void Start() {
                Routine.Run(WaitTest);
            }

            private IEnumerator<Routine> WaitTest() {
                var timeScaleBuffer = Time.timeScale;
                Time.timeScale = _timeScale;

                var previousTime = Time.unscaledTime;

                for (int i = 0; i < _iterationsAmount; i++) {
                    yield return _timingCreator.Invoke();

                    var perfectElapsedTime = _timeMode switch {
                        TimeMode.Scaled => _waitSeconds / _timeScale,
                        TimeMode.Real => _waitSeconds,
                        _ => throw new NotImplementedException($"Unhandled case: {_timeMode}")
                    };

                    var currentTime = Time.unscaledTime;
                    var perfectTime = previousTime + perfectElapsedTime;

                    var difference = GetAbsoluteDifference(currentTime, perfectTime);

                    if (difference > _maxErrorMarginSeconds) {
                        Message = $"Error details:\n" +
                                  $" Current iteration: {i}\n" +
                                  $" TimeScale: {Time.timeScale}\n" +
                                  $" Previous Time: {previousTime}s\n" +
                                  $" Current Time: {currentTime}s\n" +
                                  $" Perfect time: {perfectTime}s\n" +
                                  $" Difference: {difference} > {MaxErrorMargin}s\n";

                        IsTestFinished = true;

                        yield break;
                    }

                    previousTime = currentTime;
                }

                Time.timeScale = timeScaleBuffer;

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
