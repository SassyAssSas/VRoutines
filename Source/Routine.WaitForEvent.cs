using System;
using System.Threading;
using System.Collections.Generic;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForEvent(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        private static IEnumerator<Routine> WaitForEventRoutine(Action<Action> subscribe, Action<Action> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action(() => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }

        public static Routine WaitForEvent<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        private static IEnumerator<Routine> WaitForEventRoutine<T>(Action<Action<T>> subscribe, Action<Action<T>> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action<T>((i) => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }

        public static Routine WaitForEvent<T, T2>(Action<Action<T, T2>> subscribe, Action<Action<T, T2>> unsubscribe, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, WaitForEventRoutine(subscribe, unsubscribe, cancellationToken));
        }

        private static IEnumerator<Routine> WaitForEventRoutine<T, T2>(Action<Action<T, T2>> subscribe, Action<Action<T, T2>> unsubscribe, CancellationToken cancellationToken = default) {
            var eventInvoked = false;

            var callback = new Action<T, T2>((i, i2) => eventInvoked = true);

            subscribe.Invoke(callback);

            yield return WaitUntil(() => eventInvoked, cancellationToken);

            unsubscribe.Invoke(callback);
        }
    }
}
