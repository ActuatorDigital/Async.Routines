using System;
using System.Collections;

namespace AIR.Async.Routines
{
    public class AsyncRoutine<TResult> : AsyncHandle<TResult>
    {
        private readonly Func<TResult> _resultAccessor;

        public AsyncRoutine(IEnumerator routine, Func<TResult> resultAccessor)
        {
            _resultAccessor = resultAccessor;
            RoutineRunner.Schedule(routine, PassAlongComplete, DoCatch);
        }

        private void PassAlongComplete() => Complete(_resultAccessor());
    }
}