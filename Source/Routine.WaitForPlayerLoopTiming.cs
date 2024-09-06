namespace Violoncello.Routines {
    public readonly partial struct Routine {
        public static Routine WaitForUpdate() => WaitForPlayerLoopTiming(PlayerLoopTiming.Update);
        public static Routine WaitForFixedUpdate() => WaitForPlayerLoopTiming(PlayerLoopTiming.FixedUpdate);
        public static Routine WaitForLateUpdate() => WaitForPlayerLoopTiming(PlayerLoopTiming.PreLateUpdate);

        public static Routine WaitForPlayerLoopTiming(PlayerLoopTiming timing) {
            return new Routine(timing);
        }
    }
}
