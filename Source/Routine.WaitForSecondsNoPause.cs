using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForSecondsNoPause(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, WaitForSecondsNoPauseRoutine(value, cancellationToken));
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
    }
}
