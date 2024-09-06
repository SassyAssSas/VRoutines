using System;
using System.Collections.Generic;
using System.Threading;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitWhile(Func<bool> predicate, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.PreUpdate, () => WaitWhileRoutine(predicate, cancellationToken));
        }

        private static IEnumerator<Routine> WaitWhileRoutine(Func<bool> predicate, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested && predicate.Invoke()) {
                yield return WaitForPlayerLoopTiming(PlayerLoopTiming.PreUpdate);
            }
        }
    }
}