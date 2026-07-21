using System.Collections.Generic;
using UnityEngine;

public class GrainPile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Color faction;

    #region Grain
    public Transform grainPrefab;
    public List<Transform> grains;
    public float radius = 0.35f;
    public int amount = 50;
    #endregion

    public List<Vector3> positions = new();

    void Awake()
    {
        SpawnGrains();
        GenerateTargets(Vector2.zero);
    }

    public void SetFaction(Color faction)
    {
        spriteRenderer.color = faction;

        this.faction = faction;
    }

    private void SpawnGrains()
    {
        grains = new List<Transform>();

        for (int i = 0; i < amount; i++)
        {
            Transform grain = Instantiate(grainPrefab, transform);

            grains.Add(grain);
        }
    }

    void GenerateTargets(Vector2 pileCenter)
    {
        List<Vector2> targets = new List<Vector2>();

        targets.Clear();

        for (int i = 0; i < amount; i++)
        {
            // Random point inside a circle
            Vector2 offset = Random.insideUnitCircle * radius;

            // Optional: flatten the top of the pile
            offset.y *= 0.6f;

            targets.Add(pileCenter + offset);

            grains[i].transform.position = pileCenter + offset;
        }
    }
}
