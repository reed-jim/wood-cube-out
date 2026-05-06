using UnityEngine;

public class ElementSpawner : MonoBehaviour
{
    public Element elementPrefab;
    public int row;
    public int col;
    public float distanceMultiplier = 1;

    void Awake()
    {
        Spawn();
    }

    public void Spawn()
    {
        Vector2 elementSize = elementPrefab.Size;
        Vector2 distance = distanceMultiplier * elementSize;
        float fullSizeX = col * distance.x;
        float fullSizeY = row * distance.y;
        Vector3 center = transform.position;
        Vector3 topLeft = center + new Vector3(-fullSizeX / 2f, fullSizeY / 2f);

        Vector3 spawnPosition;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                spawnPosition = topLeft + new Vector3(j * distance.x, -i * distance.y);

                Element element = Instantiate(elementPrefab, transform);

                element.transform.position = spawnPosition;
                element.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
            }
        }
    }
}
