using System;
using UnityEngine;

public class ColorTween : TweenBase<Color>
{
    private readonly IColorable _targetRenderer;

    public ColorTween(IColorable targetRenderer, Color startColor, Color endColor, float duration, Func<float, float> easing, Action<ITween> onComplete)
        : base(startColor, endColor, duration, easing, onComplete) 
    {
        this._targetRenderer = targetRenderer;
    }

    protected override bool CanUpdate()
    {
        return base.CanUpdate() && _targetRenderer != null;
    }

    protected override void ApplyEasedValue(float t)
    {
        _targetRenderer.Color = Color.Lerp(startValue, endValue, t);
    }
}