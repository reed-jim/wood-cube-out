using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class Snake : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShape;

    // Head -> Tail
    public List<Vector2> allPoints = new();

    [Range(0, 1)]
    public float shrink = 1f;
    public float width = 1f;

    void OnValidate()
    {
        RebuildSpline(shrink);
    }

    void Update()
    {
        RebuildSpline(shrink);
    }

    public void RebuildSpline(float t)
    {
        if (allPoints.Count < 2)
        {
            spriteShape.enabled = false;
            return;
        }

        float totalLength = GetTotalLength(allPoints);
        float visibleLength = totalLength * t;

        if (visibleLength <= 0.001f)
        {
            spriteShape.enabled = false;
            return;
        }

        spriteShape.enabled = true;

        List<Vector2> visible = new();

        float travelled = 0f;

        // Start from tail and move backwards
        visible.Add(allPoints[^1]);

        for (int i = allPoints.Count - 1; i > 0; i--)
        {
            float d = Vector2.Distance(allPoints[i], allPoints[i - 1]);

            if (travelled + d <= visibleLength)
            {
                visible.Add(allPoints[i - 1]);
                travelled += d;
            }
            else
            {
                float remain = visibleLength - travelled;
                float u = remain / d;

                visible.Add(Vector2.Lerp(
                    allPoints[i],
                    allPoints[i - 1],
                    u
                ));

                break;
            }
        }

        // Reverse because SpriteShape wants head -> tail order
        visible.Reverse();

        var spline = spriteShape.spline;
        spline.Clear();

        for (int i = 0; i < visible.Count; i++)
        {
            spline.InsertPointAt(i, visible[i]);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            spline.SetHeight(i, width);
        }
    }

    float GetTotalLength(List<Vector2> pts)
    {
        float len = 0;

        for (int i = 1; i < pts.Count; i++)
            len += Vector2.Distance(pts[i - 1], pts[i]);

        return len;
    }
}

[CustomEditor(typeof(Snake))]
public class SnakeEditor : Editor
{
    float spacing = 0.5f;
    float step = 0.5f;
    int loops = 10;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        spacing = EditorGUILayout.FloatField("Point Spacing", spacing);
        step = EditorGUILayout.FloatField("Spiral Step", step);
        loops = EditorGUILayout.IntField("Loops", loops);

        if (GUILayout.Button("Generate Spiral"))
        {
            Generate((Snake)target);
        }
    }

    void Generate(Snake snake)
    {
        snake.allPoints = new List<Vector2>();

        Vector2 pos = Vector2.zero;

        Vector2[] dirs =
        {
            Vector2.right,
            Vector2.up,
            Vector2.left,
            Vector2.down
        };

        int dir = 0;
        float length = step;

        int index = 0;

        for (int ring = 0; ring < loops; ring++)
        {
            for (int side = 0; side < 2; side++)
            {
                int count = Mathf.CeilToInt(length / spacing);

                for (int i = 0; i < count; i++)
                {
                    snake.allPoints.Insert(index, pos);

                    pos += dirs[dir] * spacing;
                    index++;
                }

                dir = (dir + 1) % 4;
            }

            length += step;
        }
    }
}
