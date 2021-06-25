using System.Collections;
using UnityEngine;

namespace AIR.Async.Routines
{
    public class AsyncRoutine : AsyncHandle
    {
        public AsyncRoutine(IEnumerator routine) =>
            RoutineRunner.Schedule(routine, Complete, DoCatch);

        public AsyncRoutine(YieldInstruction yieldInstruction) =>
            RoutineRunner.Schedule(yieldInstruction, Complete, DoCatch);
    }
}