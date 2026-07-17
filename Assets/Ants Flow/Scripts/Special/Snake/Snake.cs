using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace AntsFlow
{
    public class Snake : MonoBehaviour
    {
        [SerializeField] private SpriteShapeController spriteShape;
        [SerializeField] private Transform head;

        // Head -> Tail
        public List<Vector2> allPoints = new();

        [Range(0, 1)]
        public float snakeLength = 1f;
        public float width = 1f;

        public int maxHealth;
        public int health;
        public List<int> numAntPerBox;
        public BaseTile snakeNestTile;

        public List<Node> affectedNodes;

        private Tween updateSnakeLengthTween;

        void OnValidate()
        {
            RebuildSpline(snakeLength);
        }

        public void Setup(SnakeConfig snakeConfig)
        {
            List<int> indexes = GenerateSpiralIndexes(GamePersistentVariable.numColumn + 2, GamePersistentVariable.numRow + 2, 3);
            List<int> allAffectedIndexes = ExpandIndexes(indexes, GamePersistentVariable.numColumn + 2);

            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < indexes.Count; i++)
            {
                int index = indexes[i];

                Node node = TileManager.Ins.nodes[index];

                points.Add(node.initialPosition);

                if (i == indexes.Count - 1)
                {
                    if (node.tile == null)
                    {
                        node.tile = new BaseTile()
                        {
                            tilemapPosition =
                                CommonUtil.GetVector3Int(
                                    (node.initialPosition - (GamePersistentVariable.tileMapScaleFactor / 2) * new Vector3(1, 1)) / GamePersistentVariable.tileMapScaleFactor),
                            isDone = true
                        };
                    }

                    snakeNestTile = node.tile;
                }
            }

            for (int i = 0; i < allAffectedIndexes.Count; i++)
            {
                Node node = TileManager.Ins.nodes[allAffectedIndexes[i]];

                node.isObstacle = true;
            }

            transform.position = TileManager.Ins.GetTilemapPostionWithLowerZ();

            allPoints = new List<Vector2>();

            allPoints.AddRange(points);

            RebuildSpline(1);

            maxHealth = snakeConfig.health;
            health = maxHealth;

            GenerateAntNumberPerBox(snakeConfig.spawnOrder.Count, maxHealth);
        }

        public void Hit()
        {
            health--;

            float prevSnakeLength = snakeLength;
            snakeLength = (float)health / maxHealth;

            if (updateSnakeLengthTween.isAlive)
            {
                updateSnakeLengthTween.Stop();
            }

            updateSnakeLengthTween = Tween.Custom(prevSnakeLength, snakeLength, duration: 0.3f, onValueChange: newVal =>
            {
                RebuildSpline(newVal);
            });
        }

        public void GenerateAntNumberPerBox(int totalBox, int maxHealth)
        {
            numAntPerBox = new List<int>();

            int totalUnits = maxHealth / 5;

            // Base units per number
            int baseUnits = totalUnits / totalBox;
            int remainder = totalUnits % totalBox;

            for (int i = 0; i < totalBox; i++)
            {
                int units = baseUnits;

                // Distribute remaining units
                if (i < remainder)
                    units++;

                numAntPerBox.Add(units * 5);
            }
        }

        public int GetNextAntNumberOfBox()
        {
            int antNumber = numAntPerBox[0];

            numAntPerBox.RemoveAt(0);

            return antNumber;
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

            // Head
            head.position = visible[0];

            if (visible.Count > 1)
            {
                Vector2 dir = visible[1] - visible[0];

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                head.SetPositionAndRotation(
                    visible[0],
                    Quaternion.Euler(0, 0, angle + 90f)
                );

                head.localPosition = head.localPosition.ChangeZ(-0.1f);
            }
        }

        float GetTotalLength(List<Vector2> pts)
        {
            float len = 0;

            for (int i = 1; i < pts.Count; i++)
                len += Vector2.Distance(pts[i - 1], pts[i]);

            return len;
        }

        List<int> GenerateSpiralIndexes(int width, int height, int unit)
        {
            List<int> result = new();

            int left = 0;
            int right = width - 1;
            int top = 0;
            int bottom = height - 1;

            bool isSkippedFirstRow = false;

            while (left <= right && top <= bottom)
            {
                // top row
                if (!isSkippedFirstRow)
                {
                    isSkippedFirstRow = true;
                }
                else
                {
                    for (int x = left; x <= right; x += unit)
                        result.Add(top * width + x);
                }

                // add missing right corner
                if ((right - left) % unit != 0)
                    result.Add(top * width + right);

                top += unit;

                // right column
                for (int y = top; y <= bottom; y += unit)
                    result.Add(y * width + right);

                // add missing bottom corner
                if ((bottom - top) % unit != 0)
                    result.Add(bottom * width + right);

                right -= unit;


                // bottom row
                if (top <= bottom)
                {
                    for (int x = right; x >= left; x -= unit)
                        result.Add(bottom * width + x);

                    // add missing left corner
                    if ((right - left) % unit != 0)
                        result.Add(bottom * width + left);

                    bottom -= unit;
                }


                // left column
                if (left <= right)
                {
                    for (int y = bottom; y >= top; y -= unit)
                        result.Add(y * width + left);

                    // add missing top corner
                    if ((bottom - top) % unit != 0)
                        result.Add(top * width + left);

                    left += unit;
                }
            }

            result = RemoveUnnecessaryPoints(result, width);
            result.Reverse();

            return result;


            List<int> RemoveUnnecessaryPoints(List<int> indexes, int width)
            {
                if (indexes.Count <= 2)
                    return indexes;

                List<int> result = new();

                result.Add(indexes[0]);

                Vector2Int oldDir = Vector2Int.zero;

                for (int i = 1; i < indexes.Count - 1; i++)
                {
                    Vector2Int a = IndexToXY(indexes[i - 1], width);
                    Vector2Int b = IndexToXY(indexes[i], width);
                    Vector2Int c = IndexToXY(indexes[i + 1], width);

                    Vector2Int dir1 = (b - a);
                    Vector2Int dir2 = (c - b);

                    // direction changed = keep corner
                    if (dir1 != dir2)
                    {
                        result.Add(indexes[i]);
                    }
                }

                result.Add(indexes[^1]);

                return result;
            }

            Vector2Int IndexToXY(int index, int width)
            {
                return new Vector2Int(
                    index % width,
                    index / width
                );
            }
        }

        List<int> ExpandIndexes(List<int> path, int width)
        {
            List<int> result = new();

            if (path.Count == 0)
                return result;

            result.Add(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                int from = path[i - 1];
                int to = path[i];

                int x1 = from % width;
                int y1 = from / width;

                int x2 = to % width;
                int y2 = to / width;

                int dx = Mathf.Clamp(x2 - x1, -1, 1);
                int dy = Mathf.Clamp(y2 - y1, -1, 1);

                int x = x1;
                int y = y1;

                while (x != x2 || y != y2)
                {
                    x += dx;
                    y += dy;

                    result.Add(y * width + x);
                }
            }

            return result;
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
}
