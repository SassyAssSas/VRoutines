using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForSecondsRealtimeNoPause(float value, CancellationToken cancellationToken = default) {
            return new Routine(PlayerLoopTiming.FixedUpdate, WaitForSecondsRealtimeNoPauseRoutine(value, cancellationToken));
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
    }
}
