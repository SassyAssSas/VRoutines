using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Violoncello.PlayerLoopExtensions;
using Violoncello.Routines.PlayerLoop;

namespace Violoncello.Routines {
    internal static class RoutineRunner {
        private static Dictionary<PlayerLoopTiming, LinkedList<RoutineTuple>> timingRoutineRunnerPairs;
        private static Dictionary<PlayerLoopTiming, Queue<RoutineTuple>> routinesToAdd;

        private static readonly object RoutineRunnerLock = new();

        public static RoutineAwaiter Run(IEnumerator<Routine> routine, PauseToken pauseToken = default) {
            lock (RoutineRunnerLock) {
                var awaiter = new RoutineAwaiter();

                var tuple = new RoutineTuple(awaiter, routine, pauseToken);

                if (tuple.TryTick(out Routine response)) {
                    routinesToAdd[response.Timing].Enqueue(tuple);
                }

                return awaiter;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnApplicationStarting() {
            timingRoutineRunnerPairs = new() {
                { PlayerLoopTiming.Initialization, new() },
                { PlayerLoopTiming.EarlyUpdate, new() },
                { PlayerLoopTiming.FixedUpdate, new() },
                { PlayerLoopTiming.PreUpdate, new() },
                { PlayerLoopTiming.Update, new() },
                { PlayerLoopTiming.PreLateUpdate, new() },
                { PlayerLoopTiming.PostLateUpdate, new() },
            };

            routinesToAdd = new() {
                { PlayerLoopTiming.Initialization, new() },
                { PlayerLoopTiming.EarlyUpdate, new() },
                { PlayerLoopTiming.FixedUpdate, new() },
                { PlayerLoopTiming.PreUpdate, new() },
                { PlayerLoopTiming.Update, new() },
                { PlayerLoopTiming.PreLateUpdate, new() },
                { PlayerLoopTiming.PostLateUpdate, new() },
            };

            PlayerLoopBuilder.FromCurrent()
                             .AddToSubSystem<Initialization, RoutinePlayerLoopSystem.Initialization>(Initialization)
                             .AddToSubSystem<EarlyUpdate, RoutinePlayerLoopSystem.EarlyUpdate>(EarlyUpdate)
                             .AddToSubSystem<FixedUpdate, RoutinePlayerLoopSystem.FixedUpdate>(FixedUpdate)
                             .AddToSubSystem<PreUpdate, RoutinePlayerLoopSystem.PreUpdate>(PreUpdate)
                             .AddToSubSystem<Update, RoutinePlayerLoopSystem.Update>(Update)
                             .AddToSubSystem<PreLateUpdate, RoutinePlayerLoopSystem.PreLateUpdate>(PreLateUpdate)
                             .AddToSubSystem<PostLateUpdate, RoutinePlayerLoopSystem.PostLateUpdate>(PostLateUpdate)
                             .SetPlayerLoop();
        }

        private static void Initialization() {
            AddEnqueuedRoutines();

            UpdateBehaviour(PlayerLoopTiming.Initialization);
        }

        private static void EarlyUpdate() => UpdateBehaviour(PlayerLoopTiming.EarlyUpdate);

        private static void FixedUpdate() => UpdateBehaviour(PlayerLoopTiming.FixedUpdate);

        private static void PreUpdate() => UpdateBehaviour(PlayerLoopTiming.PreUpdate);

        private static void Update() => UpdateBehaviour(PlayerLoopTiming.Update);

        private static void PreLateUpdate() => UpdateBehaviour(PlayerLoopTiming.PreLateUpdate);

        private static void PostLateUpdate() => UpdateBehaviour(PlayerLoopTiming.PostLateUpdate);

        private static void AddEnqueuedRoutines() {
            foreach (var pair in routinesToAdd) {
                var timing = pair.Key;
                var queue = pair.Value;

                while (queue.TryDequeue(out RoutineTuple tuple)) {
                    timingRoutineRunnerPairs[timing].AddLast(tuple);
                }
            }
        }

        private static void UpdateBehaviour(PlayerLoopTiming timing) {
            if (timingRoutineRunnerPairs.TryGetValue(timing, out LinkedList<RoutineTuple> routines)) {
                var node = routines.First;

                while (node != null) {
                    if (node.Value.TryTick(out Routine response)) {
                        if (response.Timing != timing) {
                            RedirectNodeAndGoToNext(ref node, timing, response.Timing);
                        }
                    }
                    else {
                        RemoveNodeAndGoToNext(routines, ref node);
                    }

                    node = node?.Next;
                }
            }
        }

        private static void RedirectNodeAndGoToNext(ref LinkedListNode<RoutineTuple> node, PlayerLoopTiming fromTiming, PlayerLoopTiming toTiming) {
            var buffer = node;

            node = node.Next;

            timingRoutineRunnerPairs[fromTiming].Remove(buffer);
            timingRoutineRunnerPairs[toTiming].AddLast(buffer);
        }

        private static void RemoveNodeAndGoToNext(LinkedList<RoutineTuple> list, ref LinkedListNode<RoutineTuple> node) {
            var buffer = node;

            node = node.Next;

            list.Remove(buffer);
        }

        private class RoutineTuple {
            public bool IsRoutinePaused => _pauseToken.Paused;

            private readonly RoutineAwaiter _awaiter;
            private readonly PauseToken _pauseToken;

            private readonly Stack<IEnumerator<Routine>> enumeratorsChain;

            private IEnumerator<Routine> currentEnumerator;

            public RoutineTuple(RoutineAwaiter awaiter, IEnumerator<Routine> enumerator, PauseToken pauseToken) {
                _awaiter = awaiter;
                _pauseToken = pauseToken;

                enumeratorsChain = new();

                currentEnumerator = enumerator;
            }

            public bool TryTick(out Routine response) {
                if (currentEnumerator == null) {
                    _awaiter.Complete();

                    response = default;

                    return false;
                }

                if (!_pauseToken.Paused) {
                    if (currentEnumerator.MoveNext()) {
                        response = currentEnumerator.Current;

                        if (response.HasInnerEnumerator) {
                            enumeratorsChain.Push(currentEnumerator);

                            currentEnumerator = response.InnerEnumerator;

                            return TryTick(out response);
                        }
                    }
                    else {
                        enumeratorsChain.TryPop(out currentEnumerator);

                        return TryTick(out response);
                    }
                }

                response = currentEnumerator.Current;

                return true;
            }

            [Obsolete]
            public bool TryStep(out Routine response) {
                if (currentEnumerator == null) {
                    _awaiter.Complete();

                    response = default;

                    return false;
                }

                if (!_pauseToken.Paused) {
                    if (!currentEnumerator.MoveNext()) {
                        enumeratorsChain.TryPop(out currentEnumerator);

                        return TryStep(out response);
                    }
                    else if (currentEnumerator.Current.HasInnerEnumerator) {
                        enumeratorsChain.Push(currentEnumerator);

                        currentEnumerator = currentEnumerator.Current.InnerEnumerator;

                        return TryStep(out response);
                    }
                }

                response = currentEnumerator.Current;

                return true;
            }
        }
    }
}
