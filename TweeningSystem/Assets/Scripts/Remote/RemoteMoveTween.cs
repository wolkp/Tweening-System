using System;
using UnityEngine;

public class RemoteMoveTween : MoveTween
{
    private readonly float _smoothingFactor;

    public RemoteMoveTween(ITransform target, Vector3 start, Vector3 end, float duration, Func<float, float> easing, Action<ITween> onComplete, float smoothingFactor)
        : base(target, start, end, duration, easing, onComplete)
    {
        _smoothingFactor = smoothingFactor;
    }

    protected override void ApplyEasedValue(float t)
    {
        base.ApplyEasedValue(t);
        target.Position = Vector3.Lerp(target.Position, Vector3.Lerp(startValue, endValue, t), _smoothingFactor);
    }
}