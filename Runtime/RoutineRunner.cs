using System;
using System.Collections;
using UnityEngine;

namespace AIR.Async.Routines
{
    [AddComponentMenu("")]
    internal class RoutineRunner : MonoBehaviour
    {
        private static RoutineRunner _instance;

        internal static void Schedule(
            IEnumerator routine,
            Action onComplete,
            Action<Exception> onExceptionThrown)
        {
            var instance = TryInit();
            instance.StartRoutine(routine, onComplete, onExceptionThrown);
        }

        internal static void Schedule(
            YieldInstruction yieldInstruction,
            Action onComplete,
            Action<Exception> onExceptionThrown)
        {
            var instance = TryInit();
            instance.StartRoutine(_instance.RunYieldInstruction(yieldInstruction), onComplete, onExceptionThrown);
        }

        private IEnumerator RunYieldInstruction(YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;
        }

        private static RoutineRunner TryInit()
        {
            if (_instance == null)
            {
                _instance = new GameObject(nameof(RoutineRunner))
                    .AddComponent<RoutineRunner>();

                DontDestroyOnLoad(_instance.gameObject);
                _instance.hideFlags = HideFlags.HideAndDontSave;
            }

            return _instance;
        }

        private void StartRoutine(
            IEnumerator routine,
            Action onComplete,
            Action<Exception> onExceptionThrown)
        {
            StartCoroutine(RunRoutine(routine, onComplete, onExceptionThrown));
        }

        //We have play the coroutine dance to get exception handling in.
        //  For more detail see https://www.jacksondunstan.com/articles/3718
        //  or https://gist.github.com/japhib/04d1443ef4ad879e5f822e058c2e4752
        private static IEnumerator RunRoutine(
            IEnumerator routine,
            Action onComplete,
            Action<Exception> onExceptionThrown)
        {
            object current = null;
            while (true)
            {
                try
                {
                    if (routine.MoveNext() == false)
                    {
                        break;
                    }
                    current = routine.Current;
                }
                catch (Exception e)
                {
                    if (onExceptionThrown != null)
                        onExceptionThrown?.Invoke(e);
                    else
                        throw;
                }
                yield return current;
            }
            onComplete?.Invoke();
        }
    }
}