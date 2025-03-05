using System;
using UnityEngine;

public abstract class TweenBase<T> : ITween
{
    protected readonly T startValue;
    protected readonly T endValue;
    protected readonly Func<float, float> easingFunction;
    protected readonly Action<ITween> onComplete;
    protected readonly float duration;

    private bool _isPaused = false;
    private float _elapsedTime = 0;

    public ITween NextTween { get; set; }
    public bool IsComplete => _elapsedTime >= duration;

    protected TweenBase(T start, T end, float duration, Func<float, float> easing, Action<ITween> onComplete)
    {
        this.startValue = start;
        this.endValue = end;
        this.duration = duration;
        this.easingFunction = easing ?? EasingFunctions.Linear;
        this.onComplete = onComplete;

        TweenManager.AddTween(this);
    }

    public void Start()
    {
        _elapsedTime = 0;
        _isPaused = false;
    }

    public void Update(float deltaTime)
    {
        if (_isPaused || IsComplete || !CanUpdate()) 
            return;

        _elapsedTime += deltaTime;
        float t = GetNormalizedTime();
        ApplyEasedValue(easingFunction(t));

        if (IsComplete)
        {
            onComplete?.Invoke(this);
            NextTween?.Start();
        }
    }

    public void Chain(params ITween[] nextTweens)
    {
        if (nextTweens == null || nextTweens.Length == 0) return;

        ITween currentTween = this;
        foreach (var tween in nextTweens)
        {
            currentTween.NextTween = tween;
            currentTween = tween;
        }
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        _isPaused = false;
    }

    public void Cancel()
    {
        _elapsedTime = duration;
        ApplyEasedValue(easingFunction(1f));
        onComplete?.Invoke(this);
    }

    public void JumpToTime(float time)
    {
        _elapsedTime = Mathf.Clamp(time, 0, duration);
        float t = GetNormalizedTime();
        ApplyEasedValue(easingFunction(t));
    }

    protected abstract void ApplyEasedValue(float t);

    protected virtual bool CanUpdate() => true;

    private float GetNormalizedTime()
    {
        return TimeUtils.GetNormalizedTime(_elapsedTime, duration);
    }
}