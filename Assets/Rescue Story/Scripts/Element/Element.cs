using UnityEngine;

public class Element : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Sprite[] spritePool;

    public Vector2 Size => spriteRenderer.bounds.size;

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        spriteRenderer.sprite = spritePool[Random.Range(0, spritePool.Length)];
    }
}
