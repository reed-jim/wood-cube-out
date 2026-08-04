using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace AntsFlow
{
    public class Snake : MonoBehaviour
    {
        [SerializeField] private SpriteShapeController spriteShape;
        [SerializeField] private Transform tail;
        public Transform Tail => tail;
        [SerializeField] private SkeletonAnimation skeletonAnimHead;
        [SpineAnimation] public string idleAnim = "idle";
        [SpineAnimation] public string actionAnim = "action";
        // Head -> Tail
        public List<Vector2> allPoints = new();

        [SerializeField] private Vector2 tailOffset;
        [SerializeField] private float tailAngle;

        [Range(0, 1)]
        public float snakeLength = 1f;
        public float width = 1f;

        [Header("Smooth Corner")]
        public float cornerSize = 0.3f;
        public int curveResolution = 4;
        [Space]

        public int maxHealth;
        public int health;
        public List<int> numAntPerBox;
        public BaseTile snakeNestTile;

        public List<Node> affectedNodes;
        List<int> allAffectedNodeIndexes;

        private Tween updateSnakeLengthTween;
        private Tween scaleTween;
        private PrimeTween.Sequence tailScaleSequence;

        void OnValidate()
        {
            RebuildSpline(snakeLength);
            if (tail != null && allPoints != null && allPoints.Count > 0)
            {
                tail.transform.position = allPoints[^1] + tailOffset;
                tail.transform.localPosition = tail.transform.position.ChangeZ(-0.2f);

                float baseAngle = 0f;
                if (allPoints.Count > 1)
                {
                    Vector2 dir = allPoints[^1] - allPoints[^2];
                    baseAngle = 90f - Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                }
                tail.transform.localRotation = Quaternion.Euler(0, 0, baseAngle + tailAngle);
            }
        }

        private void Awake()
        {
            LevelLoader.startLevelEvent += OnLevelReplayed;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
            TileManager.winLevelEvent += OnLevelReplayed;
        }

        private void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelReplayed;
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
            TileManager.winLevelEvent -= OnLevelReplayed;
        }

        private void OnLevelReplayed()
        {
            Reset();
            gameObject.SetActive(false);
        }

        public void Setup(SnakeConfig snakeConfig)
        {
            Reset();

            gameObject.SetActive(true);
            tail.gameObject.SetActive(true);
            skeletonAnimHead.gameObject.SetActive(true);

            this.tailOffset = snakeConfig.tailOffset;
            this.tailAngle = snakeConfig.tailAngle;
            if (snakeConfig.width > 0f)
            {
                this.width = snakeConfig.width;
            }
            List<int> indexes;

            if (snakeConfig.snakeSegmentIndexes.Count == 0)
            {
                indexes = GenerateSpiralIndexes(GamePersistentVariable.numColumn + 2, GamePersistentVariable.numRow + 2, 3);
                // remove first point (too short distance)
                indexes.RemoveAt(0);
            }
            else
            {
                indexes = snakeConfig.snakeSegmentIndexes;
                indexes.Reverse();
            }
            allAffectedNodeIndexes = ExpandIndexes(indexes, GamePersistentVariable.numColumn + 2);

            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < allAffectedNodeIndexes.Count; i++)
            {
                int index = allAffectedNodeIndexes[i];

                BaseTile tile = TileManager.Ins.nodes[index].tile;

                if (tile != null)
                {
                    tile.Hide();
                    tile.ActualComplete();
                }
            }

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

            transform.position = TileManager.Ins.GetTilemapPostionWithLowerZ();

            allPoints = new List<Vector2>();
            allPoints.AddRange(points);
            allPoints = SmoothCorners(allPoints, cornerSize, curveResolution);

            tail.transform.position = allPoints[^1] + tailOffset;
            tail.transform.localPosition = tail.transform.position.ChangeZ(-0.1f);

            float baseAngle = 0f;
            if (allPoints.Count > 1)
            {
                Vector2 dir = allPoints[^1] - allPoints[^2];
                baseAngle = 90f - Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            tail.transform.localRotation = Quaternion.Euler(0, 0, baseAngle + tailAngle);



            RebuildSpline(1);
            UpdateNodeObstacleState(1);

            maxHealth = snakeConfig.health;
            health = maxHealth;

            GenerateAntNumberPerBox(snakeConfig.spawnOrder.Count, maxHealth);
            ApplyScale(snakeConfig.scaleHead, snakeConfig.isFlipHead);
        }

        public void Reset()
        {
            if (scaleTween.isAlive)
            {
                scaleTween.Stop();
            }
            if (updateSnakeLengthTween.isAlive)
            {
                updateSnakeLengthTween.Stop();
            }
            if (tailScaleSequence.isAlive)
            {
                tailScaleSequence.Stop();
            }

            PrimeTween.Tween.StopAll(transform);
            transform.localScale = Vector3.one;

            if (skeletonAnimHead != null)
            {
                PrimeTween.Tween.StopAll(skeletonAnimHead.transform);
                skeletonAnimHead.gameObject.SetActive(true);
                skeletonAnimHead.transform.localScale = Vector3.one * 0.5f;
                skeletonAnimHead.transform.localRotation = Quaternion.identity;

                if (skeletonAnimHead.AnimationState != null)
                {
                    skeletonAnimHead.AnimationState.ClearTracks();
                    skeletonAnimHead.AnimationState.SetAnimation(0, idleAnim, true);
                }
            }

            if (tail != null)
            {
                PrimeTween.Tween.StopAll(tail.transform);
                tail.gameObject.SetActive(true);
                tail.transform.localScale = Vector3.one * 1.5f;
                tail.transform.localRotation = Quaternion.identity;
            }

            if (spriteShape != null)
            {
                spriteShape.enabled = true;
                if (spriteShape.spline != null)
                {
                    spriteShape.spline.Clear();
                }
            }

            snakeLength = 1f;
            health = 0;
            maxHealth = 0;
            snakeNestTile = null;

            if (allPoints != null)
            {
                allPoints.Clear();
            }
            if (allAffectedNodeIndexes != null)
            {
                allAffectedNodeIndexes.Clear();
            }
            if (affectedNodes != null)
            {
                affectedNodes.Clear();
            }
            if (numAntPerBox != null)
            {
                numAntPerBox.Clear();
            }
        }

        private void ApplyScale(float scaleHead, bool isFlipHead)
        {
            if (scaleHead > 0f)
            {
                skeletonAnimHead.transform.localScale = Vector3.one * scaleHead;
                if (isFlipHead)
                {
                    skeletonAnimHead.transform.localScale = new Vector3(skeletonAnimHead.transform.localScale.x, -skeletonAnimHead.transform.localScale.y, skeletonAnimHead.transform.localScale.z);
                }
            }
        }

        public void PunchTailScale()
        {
            if (tail != null)
            {
                if (tailScaleSequence.isAlive)
                {
                    tailScaleSequence.Stop();
                }

                Vector3 baseScale = Vector3.one * 1.5f;
                tailScaleSequence = PrimeTween.Sequence.Create()
                    .Chain(Tween.Scale(tail, endValue: baseScale * 1.2f, duration: 0.1f, ease: Ease.OutQuad))
                    .Chain(Tween.Scale(tail, endValue: baseScale, duration: 0.15f, ease: Ease.InQuad));
            }
        }

        public static List<Vector2> SmoothCorners(
            List<Vector2> points,
            float cornerSize = 0.3f,
            int curveResolution = 4)
        {
            List<Vector2> result = new List<Vector2>();

            if (points == null || points.Count < 3)
                return new List<Vector2>(points);

            result.Add(points[0]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 curr = points[i];
                Vector2 next = points[i + 1];

                Vector2 dirIn = (curr - prev).normalized;
                Vector2 dirOut = (next - curr).normalized;

                // If almost straight, don't smooth
                if (Vector2.Dot(dirIn, dirOut) > 0.99f)
                {
                    result.Add(curr);
                    continue;
                }

                float lenIn = Vector2.Distance(prev, curr);
                float lenOut = Vector2.Distance(curr, next);

                float radius = Mathf.Min(lenIn, lenOut) * cornerSize;

                Vector2 start = curr - dirIn * radius;
                Vector2 end = curr + dirOut * radius;

                // End previous straight segment
                result.Add(start);

                // Quadratic Bezier
                for (int j = 1; j < curveResolution; j++)
                {
                    float t = j / (float)curveResolution;

                    Vector2 p =
                        (1 - t) * (1 - t) * start +
                        2 * (1 - t) * t * curr +
                        t * t * end;

                    result.Add(p);
                }

                // Start next straight segment
                result.Add(end);
            }

            result.Add(points[points.Count - 1]);

            return result;
        }

        public void PlayAnimAction(string animName)
        {
            TrackEntry trackEntry = skeletonAnimHead.AnimationState.SetAnimation(0, animName, false);
            trackEntry.Complete += OnAnimationComplete;
        }
        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnAnimationComplete;
            skeletonAnimHead.AnimationState.SetAnimation(0, "idle", true);
        }

        public void Hit()
        {
            PlayAnimAction(actionAnim);
            health--;
            if (health > 0)
            {
                PunchTailScale();
            }
            else
            {
                if (scaleTween.isAlive)
                {
                    scaleTween.Stop();
                }
                scaleTween = Tween.Custom(transform.localScale, Vector3.zero, duration: 0.3f, onValueChange: newVal =>
                {
                    transform.localScale = newVal;
                });
            }
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

            int prevObstacleIndex = GetObstacleIndex(prevSnakeLength);

            UpdateNodeObstacleState(snakeLength);

            // Rắn co lại làm lộ thêm node/đường đi. Các hộp đang kẹt chờ trên slot phải được
            // check lại path ngay, nếu không chúng chỉ nhả kiến khi người chơi bấm hộp khác.
            if (GetObstacleIndex(snakeLength) > prevObstacleIndex)
            {
                RecheckInSlotAntBoxes();
            }
        }

        private int GetObstacleIndex(float snakeLengthRatio)
        {
            if (allAffectedNodeIndexes == null)
            {
                return 0;
            }

            return (int)(allAffectedNodeIndexes.Count * (1 - snakeLengthRatio));
        }

        private void RecheckInSlotAntBoxes()
        {
            var slots = WashingMachineManager.Ins.washingMachineSlots;

            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                WashingMachineItem item = slots[i].washingMachineItem;

                if (item != null)
                {
                    item.CheckAntsPath();
                }
            }
        }

        public void GenerateAntNumberPerBox(int totalBox, int maxHealth)
        {
            numAntPerBox = new List<int>();

            int totalUnits = maxHealth;

            // Base units per number
            int baseUnits = totalUnits / totalBox;
            int remainder = totalUnits % totalBox;

            for (int i = 0; i < totalBox; i++)
            {
                int units = baseUnits;

                // Distribute remaining units
                if (i < remainder)
                    units++;

                numAntPerBox.Add(units);
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
                if (spriteShape.enabled)
                {
                    SaferioTween.ScaleDownWithPunch(skeletonAnimHead.transform);
                    SaferioTween.ScaleDownWithPunch(tail);
                }

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
            skeletonAnimHead.transform.position = visible[0];

            if (visible.Count > 1)
            {
                Vector2 dir = visible[1] - visible[0];

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                skeletonAnimHead.transform.SetPositionAndRotation(
                    visible[0],
                    Quaternion.Euler(0, 0, angle)
                );

                skeletonAnimHead.transform.localPosition = skeletonAnimHead.transform.localPosition.ChangeZ(-0.1f);
            }
        }

        private void UpdateNodeObstacleState(float snakeLengthRatio)
        {
            int obstacleIndex = GetObstacleIndex(snakeLengthRatio);

            for (int i = 0; i < allAffectedNodeIndexes.Count; i++)
            {
                int index = allAffectedNodeIndexes[i];

                Node node = TileManager.Ins.nodes[index];

                if (i >= obstacleIndex)
                {
                    node.isObstacle = true;
                }
                else
                {
                    node.isObstacle = false;
                }
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

                for (int i = 1; i < indexes.Count - 1; i++)
                {
                    Vector2Int a = IndexToXY(indexes[i - 1], width);
                    Vector2Int b = IndexToXY(indexes[i], width);
                    Vector2Int c = IndexToXY(indexes[i + 1], width);

                    Vector2Int dir1 = Normalize(b - a);
                    Vector2Int dir2 = Normalize(c - b);

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

            Vector2Int Normalize(Vector2Int v)
            {
                return new Vector2Int(
                    System.Math.Sign(v.x),
                    System.Math.Sign(v.y)
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

                int stepX = System.Math.Sign(x2 - x1);
                int stepY = System.Math.Sign(y2 - y1);

                int x = x1;
                int y = y1;

                while (x != x2 || y != y2)
                {
                    if (x != x2) x += stepX;
                    if (y != y2) y += stepY;

                    result.Add(y * width + x);
                }
            }

            return result;
        }
    }

#if UNITY_EDITOR
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
#endif
}
