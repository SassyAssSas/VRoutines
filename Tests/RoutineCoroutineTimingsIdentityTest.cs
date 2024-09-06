using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Timers;

namespace Violoncello.Routines.Tests {
    public class RoutineCoroutineTimingsIdentityTest {
        private const int DefaultTimingTestIterations = 60;
        private const int UpdateTestIterations = 60;
        private const int FixedUpdateTestIterations = 60;

        [UnityTest]
        public IEnumerator DefaultTimingWithPrefixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();

            yield return RunTimingTest(null, () => default, DefaultTimingTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Prefix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("Default"));
        }
        
        [UnityTest]
        public IEnumerator DefaultTimingWithSuffixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();

            yield return RunTimingTest(null, () => default, DefaultTimingTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Suffix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("Default"));
        }

        [UnityTest]
        public IEnumerator UpdateTimingWithPrefixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();

            var routineTiming = new Func<Routine>(Routine.WaitForUpdate);

            yield return RunTimingTest(null, routineTiming, UpdateTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Prefix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("Update"));
        }

        [UnityTest]
        public IEnumerator UpdateTimingWithSuffixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();
            var routineTiming = new Func<Routine>(Routine.WaitForUpdate);

            yield return RunTimingTest(null, routineTiming, UpdateTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Suffix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("Update"));
        }

        [UnityTest]
        public IEnumerator FixedUpdateTimingWithPrefixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();

            var coroutineTiming = new WaitForFixedUpdate();
            var routineTiming = new Func<Routine>(Routine.WaitForFixedUpdate);

            yield return RunTimingTest(coroutineTiming, routineTiming, FixedUpdateTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Prefix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("FixedUpdate"));
        }

        [UnityTest]
        public IEnumerator FixedUpdateTimingWithSuffixYieldInstruction() {
            var resultBuffer = new HeapContainer<bool>();

            var coroutineTiming = new WaitForFixedUpdate();
            var routineTiming = new Func<Routine>(Routine.WaitForFixedUpdate);

            yield return RunTimingTest(coroutineTiming, routineTiming, FixedUpdateTestIterations, MonoTimingIdentityTest.YieldInstructionPlacement.Suffix, resultBuffer);

            Assert.IsTrue(resultBuffer, GetErrorMessageFor("FixedUpdate"));
        }

        private string GetErrorMessageFor(string timingName) {
            return $"Routine's {timingName} timing doesn't act the same as Coroutine's analog.";
        }

        private IEnumerator RunTimingTest(YieldInstruction coroutineTiming, Func<Routine> routineTimingCreator, int iterations, MonoTimingIdentityTest.YieldInstructionPlacement yieldInstructionPlacement, HeapContainer<bool> resultBuffer) {
            var monoTest = new MonoBehaviourTest<MonoTimingIdentityTest>();

            monoTest.component.Initialize(coroutineTiming, routineTimingCreator, iterations, yieldInstructionPlacement);

            yield return monoTest;

            resultBuffer.Value = monoTest.component.Success;
        }

        private class MonoTimingIdentityTest : MonoBehaviour, IMonoBehaviourTest {
            public bool Success { get; private set; }
            public bool IsTestFinished { get; private set; }

            private YieldInstruction _coroutineTiming;
            private Func<Routine> _routineTimingCreator;

            private int _iterationsAmount = 30;

            private YieldInstructionPlacement _selectedTests;

            public void Initialize(YieldInstruction coroutineTiming, Func<Routine> routineTimingCreator, int iterationsAmount, YieldInstructionPlacement selectedTests) {
                _coroutineTiming = coroutineTiming;
                _routineTimingCreator = routineTimingCreator;
                _selectedTests = selectedTests;
                _iterationsAmount = iterationsAmount;
            }

            private IEnumerator Start() {
                Success = true;

                if (_selectedTests.HasFlag(YieldInstructionPlacement.Prefix)) {
                    var result = new HeapContainer<bool>();

                    yield return CompareRoutinesBehaviour(TestCoroutinePrependYield, TestRoutinePrependYield, result);

                    Success &= result.Value;
                }

                if (_selectedTests.HasFlag(YieldInstructionPlacement.Suffix)) {
                    var result = new HeapContainer<bool>();

                    yield return CompareRoutinesBehaviour(TestCoroutineAppendYield, TestRoutineAppendYield, result);

                    Success &= result.Value;
                }

                IsTestFinished = true;
            }

            private IEnumerator CompareRoutinesBehaviour(Func<YieldInstruction, float[], HeapContainer<bool>, IEnumerator> coroutine, Func<Func<Routine>, float[], HeapContainer<bool>, IEnumerator<Routine>> routine, HeapContainer<bool> resultBuffer) {
                var coroutineComplete = new HeapContainer<bool>();
                var routineComplete = new HeapContainer<bool>();

                float[] coroutineTimestampsBuffer = new float[_iterationsAmount];
                float[] routineTimestampsBuffer = new float[_iterationsAmount];

                StartCoroutine(coroutine.Invoke(_coroutineTiming, coroutineTimestampsBuffer, coroutineComplete));
                Routine.Run(routine.Invoke(_routineTimingCreator, routineTimestampsBuffer, routineComplete));

                yield return new WaitUntil(() => coroutineComplete && routineComplete);

                resultBuffer.Value = Enumerable.SequenceEqual(coroutineTimestampsBuffer, routineTimestampsBuffer);

                if (!resultBuffer.Value) {
                    using var sr = File.CreateText(Path.Combine(Application.persistentDataPath, "Logs.txt"));

                    for (int i = 0; i < _iterationsAmount; i++) {
                        sr.WriteLine($"C: {coroutineTimestampsBuffer[i]}");
                        sr.WriteLine($"R: {routineTimestampsBuffer[i]}");
                        sr.WriteLine();
                    }

                    sr.Close();
                }
            }

            private IEnumerator TestCoroutineAppendYield(YieldInstruction timing, float[] buffer, HeapContainer<bool> isComplete) {
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] = Time.time;

                    yield return timing;
                }

                isComplete.Value = true;
            }

            private IEnumerator<Routine> TestRoutineAppendYield(Func<Routine> timing, float[] buffer, HeapContainer<bool> isComplete) {
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] = Time.time;

                    yield return timing.Invoke();
                }

                isComplete.Value = true;
            }

            private IEnumerator TestCoroutinePrependYield(YieldInstruction timing, float[] buffer, HeapContainer<bool> isComplete) {
                for (int i = 0; i < buffer.Length; i++) {
                    yield return timing;

                    buffer[i] = Time.time;
                }

                isComplete.Value = true;
            }

            private IEnumerator<Routine> TestRoutinePrependYield(Func<Routine> timing, float[] buffer, HeapContainer<bool> isComplete) {
                for (int i = 0; i < buffer.Length; i++) {
                    yield return timing.Invoke();

                    buffer[i] = Time.time;
                }

                isComplete.Value = true;
            }

            [Flags]
            public enum YieldInstructionPlacement {
                Prefix,
                Suffix,
            }
        }
    }
}