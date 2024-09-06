using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        internal readonly PlayerLoopTiming Timing { get; }

        private readonly IEnumerator<Routine> _innerEnumerator;

        private Routine(PlayerLoopTiming timing) {
            Timing = timing;
            _innerEnumerator = null;
        }

        private Routine(PlayerLoopTiming timing, IEnumerator<Routine> innerRoutine) {
            Timing = timing;
            _innerEnumerator = innerRoutine;
        }

        internal bool TryGetInternalRoutine(out IEnumerator<Routine> routine) {
            routine = _innerEnumerator;

            return routine != null;
        } 

        public static RoutineAwaiter Run(IEnumerator<Routine> routine, PauseToken pauseToken = default) {
            return RoutineRunner.Run(routine, pauseToken);
        }

        public static RoutineAwaiter Run(Func<IEnumerator<Routine>> routineCallback) => Run(routineCallback.Invoke());

        public static Routine SubRoutine(IEnumerator<Routine> routine) {
            return new Routine(PlayerLoopTiming.Initialization, routine);
        }
    }
}