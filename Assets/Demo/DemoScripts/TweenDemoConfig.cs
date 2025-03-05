using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="TweenDemoConfig", menuName="TweenDemo/Create TweenDemoConfig")]
public class TweenDemoConfig : ScriptableObject
{
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private Vector3 _targetScale;
    [SerializeField] private Color _targetColor;

    public Vector3 TargetPosition => _targetPosition;
    public Vector3 TargetScale => _targetScale;
    public Color TargetColor => _targetColor;
}