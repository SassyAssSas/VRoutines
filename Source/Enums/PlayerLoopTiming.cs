using UnityEngine;

namespace Violoncello.Routines {
    public enum PlayerLoopTiming {
        Update = 0,

        Initialization,
        EarlyUpdate,
        FixedUpdate,
        PreUpdate,
        PreLateUpdate,
        PostLateUpdate 
    }
}
