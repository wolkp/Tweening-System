using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TweenableDemoObject : MonoBehaviour, ITransform, IColorable
{
    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Vector3 Scale
    {
        get => transform.localScale;
        set => transform.localScale = value;
    }

    public Color Color
    {
        get => _renderer.material.color;
        set => _renderer.material.color = value;
    }
}