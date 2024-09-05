using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly struct Routine {
        internal readonly bool HasInnerEnumerator => InnerEnumerator != null;

        internal readonly PlayerLoopTiming Timing { get; }
        internal readonly IEnumerator<Routine> InnerEnumerator { get; }

        private readonly Func<IEnumerator<Routine>> _innerEnumeratorCreator;

        private Routine(PlayerLoopTiming timing, Func<IEnumerator<Routine>> innerRoutineCreator) {
            Timing = timing;
            _innerEnumeratorCreator = innerRoutineCreator;

            InnerEnumerator = innerRoutineCreator.Invoke();
        }

        public Routine Clone() {
            return new Routine(Timing, _innerEnumeratorCreator);
        }

        public static RoutineAwaiter Run(IEnumerator<Routine> routine, CancellationToken cancellationToken = default, PauseToken pauseToken = default) {
            return RoutineRunner.Run(routine, pauseToken);
        }

        public static RoutineAwaiter Run(Func<IEnumerator<Routine>> routineCallback) => Run(routineCallback.Invoke());

        public static Routine NextFrame() => Yield(PlayerLoopTiming.Update);

        public static Routine WaitForFixedUpdate() => Yield(PlayerLoopTiming.FixedUpdate);

        public static Routine WaitForLateUpdate() => Yield(PlayerLoopTiming.PreLateUpdate);

        public static Routine Yield(PlayerLoopTiming timing) {
            return new Routine(timing, () => null);
        }

        public static Routine WaitForSeconds(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, () => WaitForSecondsRoutine(value, cancellationToken));
        }

        public static Routine WaitForSecondsRealtime(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, () => WaitForSecondsRealtimeRoutine(value, cancellationToken));
        }

        public static Routine WaitForSecondsNoPause(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, () => WaitForSecondsNoPauseRoutine(value, cancellationToken));
        }

        public static Routine WaitForSecondsRealtimeNoPause(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, () => WaitForSecondsRealtimeNoPauseRoutine(value, cancellationToken));
        }

        public static Routine WaitUntil(Func<bool> predicate, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitUntilRoutine(predicate, cancellationToken));
        }

        public static Routine WaitWhile(Func<bool> predicate, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitWhileRoutine(predicate, cancellationToken));
        }

        public static Routine WaitForEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        public static Routine WaitForEvent<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        public static Routine WaitForEvent<T, T2>(Action<Action<T, T2>> subscribe, Action<Action<T, T2>> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        public static Routine SubRoutine(IEnumerator<Routine> routine) {
            return new Routine(PlayerLoopTiming.Update, () => routine);
        }

        private static IEnumerator<Routine> WaitForEventRoutine(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action(() => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }

        private static IEnumerator<Routine> WaitForEventRoutine<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action<T>((i) => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }

        private static IEnumerator<Routine> WaitForEventRoutine<T, T2>(Action<Action<T, T2>> subscribe, Action<Action<T, T2>> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action<T, T2>((i, i2) => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }

        private static IEnumerator<Routine> WaitForSecondsRoutine(float value, CancellationToken cancellationToken = default) {
            var elapsedTime = 0f;

            while (!cancellationToken.IsCancellationRequested && elapsedTime < value) {
                elapsedTime += Time.fixedDeltaTime;

                yield return WaitForFixedUpdate();
            }
        }

        private static IEnumerator<Routine> WaitForSecondsRealtimeRoutine(float value, CancellationToken cancellationToken = default) {
            var elapsedTime = 0f;

            while (!cancellationToken.IsCancellationRequested && elapsedTime < value) {
                elapsedTime += Time.fixedUnscaledDeltaTime;

                yield return WaitForFixedUpdate();
            }
        }

        private static IEnumerator<Routine> WaitForSecondsNoPauseRoutine(float value, CancellationToken cancellationToken = default) {
            var waitTime = Time.time + value;

            while (!cancellationToken.IsCancellationRequested) {
                if (waitTime > Time.time) {
                    yield return WaitForFixedUpdate();
                }
                else {
                    break;
                }
            }
        }

        private static IEnumerator<Routine> WaitForSecondsRealtimeNoPauseRoutine(float value, CancellationToken cancellationToken = default) {
            var waitTime = Time.realtimeSinceStartup + value;

            while (!cancellationToken.IsCancellationRequested) {
                if (waitTime > Time.realtimeSinceStartup) {
                    yield return WaitForFixedUpdate();
                }
                else {
                    break;
                }
            }
        }

        private static IEnumerator<Routine> WaitUntilRoutine(Func<bool> predicate, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested && !predicate.Invoke()) {
                yield return Yield(PlayerLoopTiming.PreUpdate);
            }
        }

        private static IEnumerator<Routine> WaitWhileRoutine(Func<bool> predicate, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested && predicate.Invoke()) {
                yield return Yield(PlayerLoopTiming.PreUpdate);
            }
        }
    }
}