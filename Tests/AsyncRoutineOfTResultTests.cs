using AIR.Async.Routines;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class AsyncRoutineOfTResultTests
{
    [UnityTest]
    [Timeout(1000)]
    public IEnumerator AsyncRoutine_WithRoutine_RoutineWasRun()
    {
        // Arrange
        var called = 0;
        var internalResult = false;
        IEnumerator MockCoroutine()
        {
            yield return null;
            called++;
            internalResult = true;
        }
        bool MockAccessor() => internalResult;
        var thenResultCapture = false;

        // Act
        new AsyncRoutine<bool>(MockCoroutine(), MockAccessor)
            .Then((x) => thenResultCapture = x);

        // Assert
        yield return new WaitUntil(() => called != 0);
        Assert.IsTrue(thenResultCapture);
    }
}