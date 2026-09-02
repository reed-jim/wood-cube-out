using UnityEngine;

public class PlanetSkinRandomizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer starSpriteRenderer;

    [Header("Customize")]
    [SerializeField] private bool isStar = false;

    private void Start()
    {
        RandomizeSkin();
    }

    [ContextMenu("RandomizeSkin")]
    public void RandomizeSkin()
    {
        // Create an instance of the material property block to avoid leaking instances
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        starSpriteRenderer.GetPropertyBlock(block);

        // Assign a random seed
        block.SetFloat("_Seed", Random.Range(0f, 100f));

        // Randomize base colors for variety
        if (!isStar)
        {
            block.SetColor("_BaseColor", Random.ColorHSV(0f, 1f, 0.4f, 0.8f, 0.2f, 0.8f));
            block.SetColor("_DetailColor", Random.ColorHSV(0f, 1f, 0.4f, 0.8f, 0.2f, 0.8f));
        }

        starSpriteRenderer.SetPropertyBlock(block);
    }
}
