using System.Collections.Generic;
using UnityEngine;

public class TweenDemo : MonoBehaviour
{
    [SerializeField] private TweenableDemoObject _tweenedObject;
    [SerializeField] private TweenDemoConfig _fromConfig;
    [SerializeField] private TweenDemoConfig _toConfig;
    [SerializeField] private float _tweenDuration = 2f;

    private TweenDemoConfig _currentFromConfig;
    private TweenDemoConfig _currentToConfig;
    private List<ITween> _activeTweens = new List<ITween>();
    private int _completedTweens = 0;

    private void Start()
    {
        StartTweening();
    }

    private void StartTweening()
    {
        _completedTweens = 0;

        AssignFromToConfigs();

        var tweens = new List<ITween>()
        {
            CreateMoveTween(),
            CreateScaleTween(),
            CreateColorTween()
        };

        for (int i = 0; i < tweens.Count - 1; i++)
        {
            tweens[i].Start();
        }

        _activeTweens = tweens;
    }

    private void OnTweenFinished(ITween tween)
    {
        _completedTweens++;

        Debug.Log($"{tween.GetType()} finished");

        if (_completedTweens >= _activeTweens.Count)
        {
            AllTweensFinished();
        }
    }

    private void AllTweensFinished()
    {
        Debug.Log("All tweens finished! Restarting...");
        StartTweening();
    }

    private MoveTween CreateMoveTween()
    {
        return new MoveTween(
                    _tweenedObject,
                    _currentFromConfig.TargetPosition,
                    _currentToConfig.TargetPosition,
                    _tweenDuration,
                    EasingFunctions.EaseInQuad,
                    OnTweenFinished);
    }

    private ScaleTween CreateScaleTween()
    {
        return new ScaleTween(
                _tweenedObject,
                _currentFromConfig.TargetScale,
                _currentToConfig.TargetScale,
                _tweenDuration,
                EasingFunctions.EaseInQuad,
                OnTweenFinished);
    }

    private ColorTween CreateColorTween()
    {
        return new ColorTween(
                _tweenedObject,
                _currentFromConfig.TargetColor,
                _currentToConfig.TargetColor,
                _tweenDuration,
                EasingFunctions.Linear,
                OnTweenFinished);
    }

    private void AssignFromToConfigs()
    {
        _currentFromConfig = _currentFromConfig != _fromConfig ? _fromConfig : _toConfig;
        _currentToConfig = _currentToConfig != _toConfig ? _toConfig : _fromConfig;
    }
}