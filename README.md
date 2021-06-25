# Async.Routines

Implements AIR's IAsync interface in front of a Unity Coroutine.

## Features

Provides a mechanism for scheduling a Unity Coroutine at runtime without the need of an owning MonoBehaviour and returns an `IAsync` allowing followup actions via `.Then`.

## Basic Usage

`Then` usage is unchanged from the underlying Async. Creating an AsyncRoutine simply requires:

```csharp
var asyncRoutine = new AsyncRoutine(MultiFrameCoroutine());
asyncRoutine.Then(() => Notification.Information("Process Complete."))

public IEnumerator MultiFrameCoroutine()
{
    //some
    yield return new WaitForEndOfFrame();

    //multi frame
    yield return new WaitForSeconds(1);

    //process
    yield return null;
}
```

## Installation

Add to unity via the package manager. Use add git package with `https://github.com/AnImaginedReality/Async.Routines.git`.
