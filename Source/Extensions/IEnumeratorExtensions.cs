using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines {
    public static class IEnumeratorExtensions {
        public static Routine ToRoutine(this IEnumerator<Routine> enumerator) {
            return Routine.SubRoutine(enumerator);
        }
    }
}
