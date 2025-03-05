using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class TweenManager
{
    private static readonly HashSet<ITween> _activeTweens = new();
    private static readonly object _lock = new();

    private static bool _isRunning = false;
    static TweenManager()
    {
        Application.quitting += OnApplicationQuitting;
    }

    public static void AddTween(ITween tween)
    {
        lock (_lock)
        {
            _activeTweens.Add(tween);
            tween.Start();
            StartUpdating();
        }
    }

    private static async void StartUpdating()
    {
        if (_isRunning)
            return;

        _isRunning = true;

        Stopwatch stopwatch = new();
        stopwatch.Start();
        long lastFrameTime = stopwatch.ElapsedMilliseconds;

        while (_activeTweens.Count > 0)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            float deltaTime = TimeUtils.MilisecondsToSeconds(currentTime - lastFrameTime);
            lastFrameTime = currentTime;

            var completedTweens = new HashSet<ITween>();

            var tweensCopy = _activeTweens.ToList();

            foreach (var tween in tweensCopy)
            {
                tween.Update(deltaTime);

                if (tween.IsComplete)
                    _activeTweens.Remove(tween);
            }

            await Task.Yield();
        }

        _isRunning = false;
        Clear();
    }

    public static void Clear()
    {
        _activeTweens.Clear();
    }

    private static void OnApplicationQuitting()
    {
        Clear();
    }
}