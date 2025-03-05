using System;
using UnityEngine;

public class ScaleTween : TweenBase<Vector3>
{
    private readonly ITransform _target;

    public ScaleTween(ITransform target, Vector3 startScale, Vector3 endScale, float duration, Func<float, float> easing, Action<ITween> onComplete)
        : base(startScale, endScale, duration, easing, onComplete)
    {
        this._target = target;
    }

    protected override bool CanUpdate()
    {
        return base.CanUpdate() && _target != null;
    }

    protected override void ApplyEasedValue(float t)
    {
        _target.Scale = Vector3.Lerp(startValue, endValue, t);
    }
}