using System;
using UnityEngine;

public class MoveTween : TweenBase<Vector3>
{
    protected readonly ITransform target;

    public MoveTween(ITransform target, Vector3 start, Vector3 end, float duration, Func<float, float> easing, Action<ITween> onComplete)
        : base(start, end, duration, easing, onComplete)
    {
        this.target = target;
    }

    protected override bool CanUpdate()
    {
        return base.CanUpdate() && target != null;
    }

    protected override void ApplyEasedValue(float t)
    {
        target.Position = Vector3.Lerp(startValue, endValue, t);
    }
}