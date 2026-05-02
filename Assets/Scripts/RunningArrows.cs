using UnityEngine;

public class ScrollingArrows : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private string texturePropertyName = "_BaseMap";

    private Material material;
    private float offset = 0f;

    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;

        // Если стрелки должны бежать снизу вверх увеличиваем Y
        // Если сверху вниз уменьшаем Y
        material.SetTextureOffset(texturePropertyName, new Vector2(0, offset));
    }

    void OnDestroy()
    {
        if (material != null)
            Destroy(material);
    }
}