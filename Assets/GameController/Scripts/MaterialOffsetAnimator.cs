using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialOffsetAnimator : MonoBehaviour
{
    [SerializeField] private Vector2 _speed; // —корость анимации задаетс€ в инспекторе
    [SerializeField] private int _materialID = 1; // —корость анимации задаетс€ в инспекторе

    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private Vector2 _currentOffset;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
        _currentOffset = Vector2.zero;
    }

    private void Update()
    {
        if (_meshRenderer == null)
            return;

        // ќбновл€ем смещение на основе скорости и времени
        _currentOffset += _speed * Time.deltaTime;

        // ѕолучаем материал через MaterialPropertyBlock и устанавливаем новое смещение
        _meshRenderer.GetPropertyBlock(_propertyBlock, _materialID);
        _propertyBlock.SetVector("_MainTex_ST", new Vector4(1, 1, _currentOffset.x, _currentOffset.y));
        _meshRenderer.SetPropertyBlock(_propertyBlock, _materialID);
    }
}
