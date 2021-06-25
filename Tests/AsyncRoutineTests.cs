using AIR.Async;
using AIR.Async.Routines;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class AsyncRoutineTests
{
    private const int EXPECTED_ITERATIONS = 10;

    private const int TIMEOUT = 2000;
    private const float WAIT_TIME = (TIMEOUT / 1000f) * 0.8f;

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator AsyncRoutine_WithRoutine_RoutineWasRun()
    {
        // Arrange
        var called = 0;

        // Act
        new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called++));

        // Assert
        yield return new WaitUntil(() => called == EXPECTED_ITERATIONS);
        Assert.Pass("Routine ran to completion.");
    }

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator AsyncRoutine_WithThenMethod_ThenCalledAfterRoutine()
    {
        // Arrange
        var iterationCounter = 0;
        var counterWhenCalled = 0;

        // Act
        var asyncRoutine = new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => iterationCounter++));
        asyncRoutine.Then(() => counterWhenCalled = iterationCounter);

        // Assert
        yield return new WaitUntil(() => counterWhenCalled == EXPECTED_ITERATIONS);
        const string NO_RUN_MESSAGE = "The routine was not run.";
        Assert.That(counterWhenCalled, Is.Not.EqualTo(0), NO_RUN_MESSAGE);
        const string CALLBACK_MESSAGE = "The callback occured before routine completed.";
        Assert.That(counterWhenCalled, Is.EqualTo(EXPECTED_ITERATIONS), CALLBACK_MESSAGE);
    }

    [UnityTest]
    [Timeout(1000)]
    public IEnumerator AsyncRoutine_WithThenMethod_ThenMethodIsCalledAfter()
    {
        // Arrange
        var iterationCounter = 0;
        var thenCalled = false;

        // Act
        var asyncRoutine = new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => iterationCounter++));
        asyncRoutine.Then(() => thenCalled = true);

        // Assert
        yield return new WaitUntil(() => thenCalled);
        Assert.That(iterationCounter, Is.EqualTo(EXPECTED_ITERATIONS));
        Assert.That(thenCalled, Is.True, "Then was not called at end of coroutine.");
    }

    //TODO add test that uses a asyncunion to confirm these end and call the then union as expected
    [UnityTest]
    [Timeout(2000)]
    public IEnumerator AsyncRoutine_RunMultipleRoutinesSequential_AllRoutinesComplete()
    {
        // Arrange
        int called1 = 0, called2 = 0;

        // Act
        new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called1++));
        yield return new WaitUntil(() => called1 == EXPECTED_ITERATIONS);

        new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called2++));
        yield return new WaitUntil(() => called2 == EXPECTED_ITERATIONS);

        // Assert
        Assert.Pass("All routines ran to completion.");
    }

    [UnityTest]
    [Timeout(2000)]
    public IEnumerator AsyncRoutine_RunMultipleRoutinesViaUnion_AllRoutinesComplete()
    {
        // Arrange
        int called1 = 0, called2 = 0;
        var allComplete = false;

        // Act
        var a1 = new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called1++));
        var a2 = new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called2++));
        new AsyncHandleUnion(a1, a2).Then(() => allComplete = true);
        yield return new WaitUntil(() => allComplete);

        // Assert
        Assert.AreEqual(EXPECTED_ITERATIONS, called1);
        Assert.AreEqual(EXPECTED_ITERATIONS, called2);
    }

    [UnityTest]
    [Timeout(TIMEOUT)]
    public IEnumerator AsyncRoutine_RunMultipleRoutinesParallel_AllRoutinesCompleteInTime()
    {
        // Arrange
        bool called1 = false, called2 = false;

        // Act
        var routine1 = new AsyncRoutine(TimedCoroutine(WAIT_TIME, () => called1 = true));

        var routine2 = new AsyncRoutine(TimedCoroutine(WAIT_TIME, () => called2 = true));
        yield return new WaitUntil(() => called1 && called2);

        // Assert
        Assert.Pass("All routines ran to completion within fixed time limit, thus ran in parallel.");
    }

    [UnityTest]
    [Timeout(TIMEOUT)]
    public IEnumerator AsyncRoutine_PassYieldInstruction_RoutineRunsAsPerInstruction()
    {
        // Arrange
        var called = false;

        // Act
        new AsyncRoutine(new WaitForSeconds(WAIT_TIME)).Then(() => called = true);

        // Assert
        Assert.IsFalse(called, "Routine failed to yield as per instruction");
        yield return new WaitForSeconds(WAIT_TIME / 2);
        Assert.IsFalse(called, "Routine failed to yield as per instruction");
        yield return new WaitUntil(() => called);
        Assert.Pass("Routine ran to completion following the yield instruction.");
    }

    [UnityTest]
    [Timeout(TIMEOUT)]
    public IEnumerator Then_ExceptionThrownDuring_DoesNotInvoke()
    {
        // Arrange
        int iterationCounter = 0;
        bool thenCalled = false;

        // Act
        try
        {
            var asyncRoutine = new AsyncRoutine(ThrowingMockCoroutine(() => iterationCounter++));
            asyncRoutine.Then(() => thenCalled = true);
        }
        catch (Exception)
        {
            // Eat silently, we care about the correct flow in this test not the handling
        }

        // Assert
        yield return new WaitUntil(() => iterationCounter != 0);
        Assert.IsFalse(thenCalled, "Then should not have been invoked, but has been.");
        Assert.AreEqual(1, iterationCounter, "The inner coroutine should have been called exactly 1 time, but was not.");
    }

    [UnityTest]
    [Timeout(TIMEOUT)]
    public IEnumerator Catch_ExceptionThrown_Called()
    {
        // Arrange
        var called = 0;
        bool then1Called = false;
        bool then2Called = false;
        bool catchCalled = false;

        // Act
        var async = new AsyncRoutine(MockCoroutine(EXPECTED_ITERATIONS, () => called++));
        async.Then(() => then1Called = true);
        async.Then(() => throw new Exception());
        async.Then(() => then2Called = true);
        async.Catch((e) => catchCalled = true);

        // Assert
        yield return new WaitUntil(() => catchCalled == true);
        Assert.IsTrue(then1Called, "Then1 should have been called, but wasn't.");
        Assert.IsFalse(then2Called, "Then2 should not have been called, as a preceding then threw.");
        Assert.IsTrue(catchCalled, "Catch should have been called, but was not.");
    }

    private IEnumerator MockCoroutine(int expectedIterations, Action called)
    {
        for (int i = 0; i < expectedIterations; i++)
        {
            called?.Invoke();
            yield return null;
        }
    }

    private IEnumerator ThrowingMockCoroutine(Action called)
    {
        yield return null;
        called?.Invoke();
        yield return null;
        throw new Exception();
        yield return null;
        called?.Invoke();
        yield return null;
        called?.Invoke();
    }

    private IEnumerator TimedCoroutine(float waitTimeSeconds, Action called)
    {
        yield return new WaitForSeconds(waitTimeSeconds);
        called?.Invoke();
    }
}