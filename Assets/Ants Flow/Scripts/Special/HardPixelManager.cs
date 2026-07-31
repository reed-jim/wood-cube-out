using AntsFlow;
using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static GameEnum;

namespace AntsFlow
{
    public class HardPixelInstance
    {
        public HardPixelConfig config;
        public List<Node> nodes = new List<Node>();
        public List<GameObject> visualObjects = new List<GameObject>();
    }

    public class HardPixelManager : MonoBehaviour
    {
        private static HardPixelManager ins;
        public static HardPixelManager Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = FindObjectOfType<HardPixelManager>();
                    if (ins == null)
                    {
                        GameObject go = new GameObject("HardPixelManager");
                        ins = go.AddComponent<HardPixelManager>();
                    }
                }
                return ins;
            }
        }

        private void Awake()
        {
            if (ins != null && ins != this)
            {
                Destroy(gameObject);
            }
            else
            {
                ins = this;
                RevivePopup.reviveEvent += OnRevived;
            }
        }

        private void OnDestroy()
        {
            if (ins == this)
            {
                RevivePopup.reviveEvent -= OnRevived;
            }
        }

        private void OnRevived(BoosterType boosterType)
        {
            CheckBoosterClear();
        }
        public GameObject visualPrefab;
        public List<GameObject> visualPrefabs = new List<GameObject>();

        private List<HardPixelInstance> listHardPixels = new List<HardPixelInstance>();
        private Dictionary<int, int> idToRemainingEatCount = new Dictionary<int, int>();

        public void Setup(LevelItem levelItem)
        {
            foreach (var inst in listHardPixels)
            {
                foreach (var visual in inst.visualObjects)
                {
                    if (visual != null)
                    {
                        Destroy(visual);
                    }
                }
            }
            listHardPixels.Clear();
            idToRemainingEatCount.Clear();
            if (levelItem == null || levelItem.specialElementConfig == null || levelItem.specialElementConfig.hardPixelConfigs == null)
            {
                return;
            }

            var configs = levelItem.specialElementConfig.hardPixelConfigs;
            if (configs == null || configs.Count == 0) return;

            foreach (var config in configs)
            {
                if (!idToRemainingEatCount.ContainsKey(config.id))
                {
                    idToRemainingEatCount[config.id] = config.requiredEatCount;
                }
            }

            var groupedConfigs = configs.GroupBy(c => c.id);

            foreach (var group in groupedConfigs)
            {
                int hpId = group.Key;
                List<Node> allGroupNodes = new List<Node>();
                List<HardPixelInstance> groupInstances = new List<HardPixelInstance>();

                foreach (var config in group)
                {
                    var inst = new HardPixelInstance { config = config };
                    groupInstances.Add(inst);
                    listHardPixels.Add(inst);

                    if (config.startNodeId >= 0 && config.startNodeId < TileManager.Ins.nodes.Count)
                    {
                        Node startNode = TileManager.Ins.nodes[config.startNodeId];
                        int startX = startNode.x;
                        int startY = startNode.y;

                        for (int r = 0; r < config.numRows; r++)
                        {
                            for (int c = 0; c < config.numCols; c++)
                            {
                                int targetX = startX + c;
                                int targetY = startY + r;

                                Node node = TileManager.Ins.nodes.FirstOrDefault(n => n.x == targetX && n.y == targetY);
                                if (node != null)
                                {
                                    inst.nodes.Add(node);
                                    allGroupNodes.Add(node);
                                    node.isObstacle = true;

                                    if (node.tile != null)
                                    {
                                        node.tile.isLocked = true;
                                    }
                                }
                            }
                        }

                        if (inst.nodes.Count > 0)
                        {
                            CreateRegionBackgroundVisual(inst);
                        }
                    }
                }

                // Combined text visual is no longer needed since text is now displayed directly on the HardPixelVisual instance having isShowText = true
                /*
                if (allGroupNodes.Count > 0)
                {
                    CreateCombinedTextVisual(hpId, allGroupNodes, groupInstances);
                }
                */
            }

            CheckAndOpenCompletedGroups();
        }

        private void CreateRegionBackgroundVisual(HardPixelInstance inst)
        {
            GameObject prefab = null;
            if (visualPrefabs != null && inst.config.prefabIndex >= 0 && inst.config.prefabIndex < visualPrefabs.Count)
            {
                prefab = visualPrefabs[inst.config.prefabIndex];
            }

            if (prefab == null)
            {
                prefab = visualPrefab;
            }

            if (prefab == null) return;

            Vector3 averagePos = Vector3.zero;
            foreach (var node in inst.nodes)
            {
                averagePos += node.initialPosition;
            }
            averagePos /= inst.nodes.Count;
            averagePos.z = -5f;

            averagePos += new Vector3(0, 0.05f, 0);
            averagePos += new Vector3(inst.config.offsetX, inst.config.offsetY, 0);

            GameObject visual = Instantiate(prefab, averagePos, Quaternion.Euler(0, 0, inst.config.rotationAngle));
            visual.name = "HardPixel_Bg_" + inst.config.id;

            float cellWidth = GamePersistentVariable.tileDistance.x > 0 ? GamePersistentVariable.tileDistance.x : GamePersistentVariable.tileMapScaleFactor;
            float cellHeight = GamePersistentVariable.tileDistance.y > 0 ? GamePersistentVariable.tileDistance.y : GamePersistentVariable.tileMapScaleFactor;

            float scaleW = inst.config.scaleWidth <= 0 ? 1.01f : inst.config.scaleWidth;
            float scaleH = inst.config.scaleHeight <= 0 ? 1.055f : inst.config.scaleHeight;

            float width = inst.config.numCols * cellWidth * scaleW;
            float height = inst.config.numRows * cellHeight * scaleH;

            RectTransform rectTransform = visual.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(width, height);
            }
            else
            {
                visual.transform.localScale = new Vector3(width, height, 1f);
            }

            var hpVisual = visual.GetComponent<HardPixelVisual>();
            if (hpVisual != null)
            {
                hpVisual.ResetState();
                int remaining = idToRemainingEatCount.ContainsKey(inst.config.id) ? idToRemainingEatCount[inst.config.id] : inst.config.requiredEatCount;
                hpVisual.SetText(remaining.ToString());
                hpVisual.SetTextActive(inst.config.isShowText);
                hpVisual.UpdateState(remaining, inst.config.requiredEatCount);

                if (inst.config.isShowText)
                {
                    var canvas = visual.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        canvas.overrideSorting = true;
                        canvas.sortingOrder = 2;
                    }
                }
            }
            else
            {
                var tmp = visual.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.gameObject.SetActive(false);
                var tm = visual.GetComponentInChildren<TextMesh>();
                if (tm != null) tm.gameObject.SetActive(false);
            }

            inst.visualObjects.Add(visual);
        }

        private void CreateCombinedTextVisual(int id, List<Node> allGroupNodes, List<HardPixelInstance> groupInstances)
        {
            if (visualPrefab == null) return;

            Vector3 combinedAveragePos = Vector3.zero;
            foreach (var node in allGroupNodes)
            {
                combinedAveragePos += node.initialPosition;
            }
            combinedAveragePos /= allGroupNodes.Count;
            combinedAveragePos.z = -5.1f;

            GameObject visual = Instantiate(visualPrefab, combinedAveragePos, Quaternion.identity);
            visual.name = "HardPixel_Text_" + id;

            var img = visual.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.enabled = false;
            var r = visual.GetComponent<Renderer>();
            if (r != null) r.enabled = false;

            if (groupInstances.Count > 0)
            {
                groupInstances[0].visualObjects.Add(visual);
            }

            UpdateGroupText(visual, id);
        }

        private void UpdateGroupText(GameObject textVisual, int id)
        {
            if (textVisual == null) return;

            int count = idToRemainingEatCount[id];

            Transform textTransform = null;

            var tmp = textVisual.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = count.ToString();
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 0;
                tmp.fontSizeMax = 1;
                tmp.alignment = TextAlignmentOptions.Center;

                RectTransform tmpRect = tmp.GetComponent<RectTransform>();
                if (tmpRect != null)
                {
                    tmpRect.anchorMin = Vector2.zero;
                    tmpRect.anchorMax = Vector2.one;
                    tmpRect.offsetMin = Vector2.zero;
                    tmpRect.offsetMax = Vector2.zero;
                }
                textTransform = tmp.transform;
            }


            if (textTransform != null)
            {
                Tween.StopAll(textTransform);
                textTransform.localScale = Vector3.one;
                Tween.Scale(textTransform, endValue: Vector3.one * 1.3f, duration: 0.12f, cycles: 2, cycleMode: PrimeTween.CycleMode.Yoyo);
            }
        }

        private void UpdateVisualText(int id)
        {
            var matchingInstances = listHardPixels.Where(inst => inst.config.id == id).ToList();
            foreach (var inst in matchingInstances)
            {
                foreach (var visual in inst.visualObjects)
                {
                    if (visual != null)
                    {
                        var hpVisual = visual.GetComponent<HardPixelVisual>();
                        if (hpVisual != null)
                        {
                            int currentCount = idToRemainingEatCount[id];
                            if (inst.config.isShowText)
                            {
                                hpVisual.SetText(currentCount.ToString());
                                hpVisual.PlayTextTween();
                            }
                            hpVisual.UpdateState(currentCount, inst.config.requiredEatCount);
                        }
                        else if (visual.name == "HardPixel_Text_" + id)
                        {
                            UpdateGroupText(visual, id);
                        }
                    }
                }
            }
        }

        public void OnTileCompleted()
        {
            if (idToRemainingEatCount == null || idToRemainingEatCount.Count == 0)
            {
                return;
            }

            List<int> idsToDecrement = idToRemainingEatCount.Keys.ToList();
            foreach (int id in idsToDecrement)
            {
                if (idToRemainingEatCount[id] > 0)
                {
                    idToRemainingEatCount[id]--;
                    UpdateVisualText(id);
                }
            }

            CheckAndOpenCompletedGroups();
            CheckBoosterClear();
        }

        public void OnBoosterClearTiles(int count)
        {
            if (idToRemainingEatCount == null || idToRemainingEatCount.Count == 0)
            {
                return;
            }

            List<int> idsToDecrement = idToRemainingEatCount.Keys.ToList();
            foreach (int id in idsToDecrement)
            {
                if (idToRemainingEatCount[id] > 0)
                {
                    idToRemainingEatCount[id] = Mathf.Max(0, idToRemainingEatCount[id] - count);
                    UpdateVisualText(id);
                }
            }

            CheckAndOpenCompletedGroups();
        }

        public void CheckBoosterClear()
        {
            if (idToRemainingEatCount == null || idToRemainingEatCount.Count == 0)
            {
                return;
            }

            var activeInstances = listHardPixels.Where(inst => idToRemainingEatCount.ContainsKey(inst.config.id) && idToRemainingEatCount[inst.config.id] > 0).ToList();

            foreach (var inst in activeInstances)
            {
                bool allTilesCleared = inst.nodes.All(node => node.tile == null || node.tile.isDone);
                if (allTilesCleared)
                {
                    int hpId = inst.config.id;
                    int startCount = idToRemainingEatCount[hpId];
                    if (startCount <= 0) continue;

                    idToRemainingEatCount[hpId] = 0;

                    Tween.Custom(startCount, 0, duration: 0.5f, onValueChange: (val) =>
                    {
                        UpdateVisualText(hpId);
                    }).OnComplete(() =>
                    {
                        CheckAndOpenCompletedGroups();
                    });
                }
            }
        }

        private void CheckAndOpenCompletedGroups()
        {
            List<int> completedIds = idToRemainingEatCount.Where(x => x.Value <= 0).Select(x => x.Key).ToList();

            foreach (int completedId in completedIds)
            {
                var matchingInstances = listHardPixels.Where(inst => inst.config.id == completedId).ToList();
                foreach (var inst in matchingInstances)
                {
                    foreach (var node in inst.nodes)
                    {
                        node.isObstacle = false;
                        if (node.tile != null)
                        {
                            node.tile.isLocked = false;
                        }
                    }

                    foreach (var visual in inst.visualObjects)
                    {
                        if (visual != null)
                        {
                            var hpVisual = visual.GetComponent<HardPixelVisual>();
                            if (hpVisual != null)
                            {
                                hpVisual.PlayHitFX();
                            }

                            Tween.ShakeScale(visual.transform, strength: Vector3.one * 0.15f, duration: 0.3f).OnComplete(() => Destroy(visual));
                        }
                    }
                    inst.visualObjects.Clear();
                }
            }

            if (completedIds.Count > 0)
            {
                List<WashingMachineSlot> waitingSlots = WashingMachineManager.Ins.GetAvailableWashingMachineSlots();
                for (int i = 0; i < waitingSlots.Count; i++)
                {
                    var slot = waitingSlots[i];
                    if (slot != null && slot.washingMachineItem != null)
                    {
                        slot.washingMachineItem.CheckAntsPath();
                    }
                }
            }
        }
    }
}
