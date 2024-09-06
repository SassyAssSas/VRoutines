using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForSecondsRealtime(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, WaitForSecondsRealtimeRoutine(value, cancellationToken));
        }

        private static IEnumerator<Routine> WaitForSecondsRealtimeRoutine(float value, CancellationToken cancellationToken = default) {
            var elapsedTime = 0f;

            while (!cancellationToken.IsCancellationRequested && elapsedTime < value) {
                elapsedTime += Time.fixedUnscaledDeltaTime;

                yield return WaitForFixedUpdate();
            }
        }
    }
}
