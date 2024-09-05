# ﻿THIS PROJECT IS CURRENTLY WIP

<!-- omit from toc -->
# VRoutine
Provides a superiour alternative to unity's default Coroutines and magic methods such as Update, Fixed Update etc.:
 - Can be used outside MonoBehaviour derieved classes;
 - Easy to pause and resume execution;
 - Can return values;
 - Runs on a single PlayerLoopSystem for each timing which results in a much better perfomance than when using built-in coroutines;
 - No runtime allocation.;

<!-- omit from toc -->
## Contents
- [Documentation](#documentation)
  - [Installation](#installation)
  - [Creating routines](#creating-routines)
  - [Running routines](#running-routines)
  - [Pausing and resuming routines](#pausing-and-resuming-routines)
  - [Returning values](#returning-values)
  - [Running sub-routines](#running-sub-routines)
- [Extras](#extras)
  - [Tests](#tests)
  - [Perfomance](#perfomance)


# Documentation

## Installation
Installing package


## Creating routines
To start using routines use the `Violoncello.Routines` namespace.

Similarly to Coroutines, to create a routine you need to write an iterator that returns `IEnumerator<Routine>`:
```csharp
using System.Collections.Generic;
using Violoncello.Routines;
    
private IEnumerator<Routine> ExampleRoutine() 
{
    
}
```
Just like Coroutines, VRoutines instructions might be delayed. To delay a VRoutine you need to `yield return` a `Routine` object. Here's a list of delays you can use:

```csharp
private IEnumerator<Routine> ExampleRoutine() 
{
    // Will wait until next time Update() is called:
    yield return Routine.NextFrame();

    // Will wait until next time FixedUpdate() is called:
    yield return Routine.WaitForFixedUpdate();
    
    // Will wait for 5 seconds (Scaled time):
    yield return Routine.WaitForSeconds(5f);

    // Will wait for 5 seconds (Real time):
    yield return Routine.WaitForSecondsRealtime(5f);

    // Will wait until the passed function retuns true
    yield return Routine.WaitUntil(() => true);

    // Will wait until next specified PlayerLoop timing, like PreUpdate, Update, PostLateUpdate etc.:
    yield return Routine.Yield(PlayerLoopTiming.PreLateUpdate);
}
```

##  Running routines
To run a routine you need to use a static `Routine.Run()` method like in the example below:
```csharp
private void Awake() 
{
    Routine.Run(MoveRoutine());
}

// Will move the gameObject right every frame
private IEnumerator<Routine> MoveRoutine() 
{
    while (true) 
    {
        yield return Routine.NextFrame();		
        
        transform.position += Time.deltaTime * Vector3.right;
    }
}
```
`Routine.Run()` returns a `RoutineAwaiter` object that might be used to execute code after the routine finishes:
```csharp
private void Awake()
{
    Routine.Run(ExampleRoutine())
           .Then(() => Debug.Log("Routine has finished!"));
}

private IEnumerator<Routine> ExampleRoutine() 
{
    for (int i = 1; i <= 5; i++) 
    {
        Debug.Log(i);
    }	

    yield break;
}

// The following code will output:

// 1
// 2
// 3
// 4
// 5
// Routine has finished!
```

## Pausing and resuming routines
The package adds 2 new types that might be used for pausing and resuming routines:
- PauseTokenSource;
- PauseToken.

PauseTokenSource object contains a `Token` property that might be passed into `Routine.Run()` method like in the example below:
```csharp
private PauseTokenSource pts;

private void Awake() 
{
    pts = new();

    Routine.Run(WalkRoutine(), pts.Token);
    Routine.Run(JumpRoutine(), pts.Token);
}
```
Then you can use your `PauseTokenSource` object's `Pause()` and `Resume()` methods whenever you want to pause or resume routines execution:
```csharp
private void OnPauseButtonPressed() 
{
    if (pts.Paused)
    {
        pts.Resume();
    }
    else 
    {
        pts.Pause();
    }
}
```
`Routine.WaitForSeconds()` and `Routine.WaitForSecondsRealtime()` are both affected by pausing and will stop counting time until resumed. If you want your timers to continue counting time even if the routine is paused, use `Routine.WaitForSecondsNoPause()` and `Routine.WaitForSecondsRealtimeNoPause()` instead.

However, even if a non-pausable timer finishes, the routine exectution will not be continued until it's unpaused.

## Returning values
If you want your routine to return a value after it finishes, you need to use `IEnumerator<Routine<T>>` as a return type of your iterator, where T is the return type.

To return a value use `Routine<T>.Return()` method like in the example below:
```csharp
private void IEnumerator<Routine<int>> GetRandomNumberRoutine()
{
    var number = Random.Range(0, 10);

    yield return Routine.Return(number);

    // This will not be executed
    Debug.Log("Hello, World!");
}
``` 
Note that using this method will stop the routine's execution like if you used `yield break`.

When you're running a routine that returns a value, the value will be put as an argument in the `RoutineAwaiter.Then()` method's delegate argument:
```csharp
private void Awake() {
    Routine.Run(GetHelloWorldStringRoutine())
           .Then((result) => Debug.Log(result));
}

private void IEnumerator<Routine<int>> GetHelloWorldStringRoutine()
{
    yield return Routine.Return("Hello, World!");
}

// The following code will output:

// Hello, World!
```

## Running sub-routines
You can start a routine inside another routine. The root routine execution will stop until the nested routine finishes:
```csharp
private IEnumerator<Routine> ExampleRoutine() 
{
    Debug.Log("Started couting!");

    yield return Routine.SubRoutine(ExampleSubRoutine());

    Debug.Log("Finished counting!");
} 

private IEnumerator<Routine> ExampleSubRoutine()
{
    for (int i = 1; i <= 5; i++) 
    {
        Debug.Log(i);
    }

    yield break;	
}

// The following code will output:

// Started couting!
// 0
// 1
// 2
// 3
// Finished counting!
```

# Extras
## Tests
## Perfomance
On the graph below are results of perfomance testing with comparison with Coroutines and Update callbacks. During the test were used 10000 objects with the same code, shown below:
```csharp

```
