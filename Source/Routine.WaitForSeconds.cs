using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForSeconds(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, WaitForSecondsRoutine(value, cancellationToken));
        }

        private static IEnumerator<Routine> WaitForSecondsRoutine(float value, CancellationToken cancellationToken = default) {
            var elapsedTime = 0f;

            while (!cancellationToken.IsCancellationRequested && elapsedTime < value) {
                elapsedTime += Time.fixedDeltaTime;

                yield return WaitForFixedUpdate();
            }
        }
    }
}
