using UnityEngine;
using Steamworks;
using System.Threading.Tasks;
using JetBrains.Annotations;
using System;
/// <summary>
/// A class to help with Steam related functions.
/// </summary>
public static class SteamHelper
{
    /// <summary>
    /// Creates an awaitable Task to fetch the result of a <see cref="SteamAPICall_t"/> without having to create many methods. <br></br>
    /// The <see cref="CallResult{T}"/> that is created with the <see cref="SteamAPICall_t"/> (see <see cref="CallResult{T}.Set(SteamAPICall_t, CallResult{T}.APIDispatchDelegate)"/>) will automatically be disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="steam_call"></param>
    /// <returns></returns>
    public static Task<T> CreateAwaitableFromSteamAPICall<T>(SteamAPICall_t steam_call) where T : struct
    {
        TaskCompletionSource<T> source = new TaskCompletionSource<T>();
        CallResult<T> callResult = null;
        callResult = CallResult<T>.Create((result, bIOFailure) =>
        {
            if (bIOFailure)
            {
                source.SetException(new Exception("bIOFailure when awaiting for SteamAPICall_t!"));
                callResult?.Dispose();
                return;
            }

            bool valid = source.TrySetResult(result);

            if (!valid)
            {
                source.SetException(new Exception("TaskCompletionSource.TrySetResult failed!"));
            }

            callResult?.Dispose();
        });

        callResult.Set(steam_call);
        return source.Task;
    }

    /// <summary>
    /// Creates an awaitable Task to fetch the result of a <see cref="Callback{T}"/> that matches the <paramref name="matchPredicate"/> without having to create many methods. <br></br>
    /// Since it is possible for this <see cref="Callback{T}"/> to fail the predicate, an optional timeout parameter is provided. <br></br>
    /// Timeouts will throw an exception for you to catch and handle such cases. <br></br>
    /// You should use <see cref="Callback{T}"/> normally if the <see cref="Callback{T}"/> is just one-shot event that isn't necessary to keep checking every frame.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="matchPredicate"></param>
    /// <param name="timeoutMs"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<T> CreateAwaitableFromCallback<T>(Func<T, bool> matchPredicate = null, int timeoutMs = 10000) where T : struct
    {
        TaskCompletionSource<T> source = new TaskCompletionSource<T>();

        Callback<T> callback = null;

        callback = Callback<T>.Create(result =>
        {
            if (matchPredicate != null && !matchPredicate.Invoke(result))
            {
                return;
            }

            bool valid = source.TrySetResult(result);

            if (!valid)
            {
                source.SetException(new Exception("TaskCompletionSource.TrySetResult failed!"));
            }
        });

        // since it's possible for a callback to fail (as opposed to CallResult<T>), we have to implement a timeout
        // if ANY one of the callback matches the predicate within the timeout, then we're good!

        Task timeoutTask = Task.Delay(timeoutMs);
        Task completedTask = await Task.WhenAny(source.Task, timeoutTask);

        callback?.Dispose();
        if (completedTask == source.Task)
        {
            return await source.Task;
        }
        else
        {
            throw new Exception($"Callback timed out after {timeoutMs} ms!");
        }
    }
}
