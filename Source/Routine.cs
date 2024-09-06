using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public readonly partial struct Routine {
        internal readonly PlayerLoopTiming Timing { get; }

        private readonly Func<IEnumerator<Routine>> _innerEnumeratorCreator;

        private Routine(PlayerLoopTiming timing) {
            Timing = timing;
            _innerEnumeratorCreator = null;
        }

        private Routine(PlayerLoopTiming timing, Func<IEnumerator<Routine>> innerRoutineCreator) {
            Timing = timing;
            _innerEnumeratorCreator = innerRoutineCreator;
        }

        internal bool TryGetInternalRoutine(out IEnumerator<Routine> routine) {
            routine = _innerEnumeratorCreator?.Invoke();

            return routine != null;
        } 

        public static RoutineAwaiter Run(IEnumerator<Routine> routine, PauseToken pauseToken = default) {
            return RoutineRunner.Run(routine, pauseToken);
        }

        public static RoutineAwaiter Run(Func<IEnumerator<Routine>> routineCallback) => Run(routineCallback.Invoke());

        public static Routine SubRoutine(IEnumerator<Routine> routine) {
            return new Routine(PlayerLoopTiming.Update, () => routine);
        }
    }
}