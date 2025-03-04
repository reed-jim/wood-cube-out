using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class LevelLoader : MonoBehaviour
{
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private ObjectPartMaterialContainer objectPartMaterialContainer;

    private List<Tween> _tweens;
    private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    private int _maxLevel;

    public static event Action startLevelEvent;
    public static event Action setLevelNumberEvent;
    public static event Action<int> setLevelScrewNumberEvent;
    public static event Action<int> setMultiPhaseLevelScrewNumberEvent;
    public static event Action showSceneTransitionEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    private void Awake()
    {
        PausePopup.replayLevelEvent += Replay;
        LosePopup.replayLevelEvent += Replay;
        DebugPopup.toLevelEvent += GoLevel;
        DebugPopup.nextLevelEvent += NextLevel;
        DebugPopup.prevLevelEvent += PrevLevel;
        WinPopup.nextLevelEvent += NextLevel;
        WinPopup.goLevelEvent += GoLevel;
        LosePopup.skipLevelEvent += NextLevel;

        _tweens = new List<Tween>();

        currentLevel.Load();

        _maxLevel = levelDataContainer.MaxLevel;

        GoLevel(currentLevel.Value);
    }

    void OnDestroy()
    {
        PausePopup.replayLevelEvent -= Replay;
        LosePopup.replayLevelEvent -= Replay;
        DebugPopup.toLevelEvent -= GoLevel;
        DebugPopup.nextLevelEvent -= NextLevel;
        DebugPopup.prevLevelEvent -= PrevLevel;
        WinPopup.nextLevelEvent -= NextLevel;
        WinPopup.goLevelEvent -= GoLevel;
        LosePopup.skipLevelEvent -= NextLevel;

        CommonUtil.StopAllTweens(_tweens);
    }

    private async Task LoadLevel()
    {
        int nextLevel = 1;

        bool isLoopLevel = false;

        if (!UserData.IsLoopLevel)
        {
            if (currentLevel.Value <= _maxLevel)
            {
                nextLevel = currentLevel.Value;
            }
            else
            {
                nextLevel = UnityEngine.Random.Range(6, _maxLevel);

                await UserData.SetIsLoopLevel(true);
                await UserData.SetIsLastLevelBeforeLoop(_maxLevel);
            }
        }
        else
        {
            if (currentLevel.Value - UserData.NumLevelLoop >= _maxLevel)
            {
                nextLevel = UnityEngine.Random.Range(6, _maxLevel);

                // await UserData.SetNumLevelLoop(UserData.NumLevelLoop + 1);
            }
            else
            {
                nextLevel = currentLevel.Value - UserData.NumLevelLoop;
            }

            isLoopLevel = true;
        }

        // if (isLoopLevel)
        // {
        //     switchRouteEvent?.Invoke(ScreenRoute.LevelComingSoon);

        //     return;
        // }

        nextLevel = Mathf.Clamp(nextLevel, 1, _maxLevel);

        GamePersistentVariable.levelPrefab = nextLevel;

        setLevelNumberEvent?.Invoke();

        string levelPrefab = $"Level {nextLevel}";

        // A/B TEST LEVEL 4
        if (nextLevel == 4)
        {
            int level4Variant = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_4_VARIANT, 0);

            if (level4Variant == 1)
            {
                levelPrefab = $"Level {nextLevel} - Variant 1";
            }
            else if (level4Variant == 2)
            {
                levelPrefab = $"Level {nextLevel} - Variant 2";
            }
        }

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(levelPrefab);

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = Instantiate(op.Result, transform);

                if (level.GetComponent<LevelTransformManualModifier>() == null)
                {
                    level.transform.position = new Vector3(0, -0.03f * GamePersistentVariable.screenSizeWorld.y, 10);
                    level.transform.rotation = Quaternion.Euler(new Vector3(-30, -45, 30));
                }

                _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);

                if (currentLevel.Value == 1)
                {
                    AutoAssignScrewFactionLevelManualGroup(level);
                }
                else
                {
                    AutoAssignScrewFaction(level);
                }

                AutoAssignScrewFactionMultiPhaseLevel(level);

                if (isLoopLevel)
                {
                    RandomizeLevelModelColor(level);
                }
                else
                {
                    SaferioTracking.TrackLevelStart(currentLevel.Value);
                }

                if (IsShowBoosterSuggestion(nextLevel))
                {
                    DelayShowBoosterSuggest();
                }
            }
        };

        showSceneTransitionEvent?.Invoke();
    }

    private async void GoLevel(int level)
    {
        await Task.Delay(500);

        if (gameObject.transform.childCount > 0)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        currentLevel.Value = level;

        currentLevel.Save();

        levelBoosterObserver.Reset();

        startLevelEvent?.Invoke();

        LoadLevel();
    }

    private void NextLevel()
    {
        GoLevel(currentLevel.Value + 1);
    }

    private void PrevLevel()
    {
        GoLevel(currentLevel.Value - 1);
    }

    private void Replay()
    {
        GoLevel(currentLevel.Value);
    }

    private void AutoAssignScrewFactionMultiPhaseLevel(GameObject level)
    {
        MultiPhaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<MultiPhaseScrew>(level.transform).ToArray();

        Dictionary<int, List<GameFaction>> remainingFactionForScrewsByPhase = new Dictionary<int, List<GameFaction>>();

        Dictionary<int, int> currentFactionByPhase = new Dictionary<int, int>();

        for (int i = 0; i < screws.Length; i++)
        {
            int phase = screws[i].Phase;

            if (!remainingFactionForScrewsByPhase.ContainsKey(phase))
            {
                currentFactionByPhase.Add(phase, 0);

                remainingFactionForScrewsByPhase.Add(phase, new List<GameFaction>() { GameConstants.SCREW_FACTION[currentFactionByPhase[phase]] });
            }
            else
            {
                remainingFactionForScrewsByPhase[phase].Add(GameConstants.SCREW_FACTION[currentFactionByPhase[phase]]);

                if (i > 0 && (i + 1) % 3 == 0)
                {
                    currentFactionByPhase[phase]++;

                    if (currentFactionByPhase[phase] >= GameConstants.SCREW_FACTION.Length)
                    {
                        currentFactionByPhase[phase] = 0;
                    }
                }
            }
        }

        for (int i = 0; i < screws.Length; i++)
        {
            int phase = screws[i].Phase;

            int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrewsByPhase[phase].Count);

            screws[i].ScrewId = i;
            screws[i].Faction = remainingFactionForScrewsByPhase[phase][randomIndex];

            screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            remainingFactionForScrewsByPhase[phase].RemoveAt(randomIndex);
        }

        setMultiPhaseLevelScrewNumberEvent?.Invoke(screws.Length);
    }

    private void AutoAssignScrewFactionLevelManualGroup(GameObject level)
    {
        BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int numScrew = screws.Length;

        int numTripleScrews = numScrew / 3;

        List<GameFaction> factionPool = new List<GameFaction>();

        for (int i = 0; i < numTripleScrews; i++)
        {
            GameFaction randomFaction = GameConstants.SCREW_FACTION[UnityEngine.Random.Range(0, GameConstants.SCREW_FACTION.Length)];

            factionPool.Add(randomFaction);
        }

        ScrewFactionGroupManual[] screwFactionGroups = new ScrewFactionGroupManual[numScrew];

        for (int i = 0; i < numScrew; i++)
        {
            screwFactionGroups[i] = screws[i].GetComponent<ScrewFactionGroupManual>();
        }

        // screwFactionGroups = screwFactionGroups.OrderBy(item => item.Group).ToArray();

        for (int i = 0; i < numScrew; i++)
        {
            screws[i].ScrewId = i;
            screws[i].Faction = factionPool[screwFactionGroups[i].Group];

            screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();
        }

        setLevelScrewNumberEvent?.Invoke(numScrew);
    }

    private void AutoAssignScrewFaction(GameObject level)
    {
        BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int numScrew = screws.Length;

        List<BaseScrew> noBlockedScrews = new List<BaseScrew>();
        List<BaseScrew> blockedScrews = new List<BaseScrew>();

        for (int i = 0; i < screws.Length; i++)
        {
            int numBlockedObject = screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);

            if (numBlockedObject == 0)
            {
                noBlockedScrews.Add(screws[i]);
            }
            else
            {
                blockedScrews.Add(screws[i]);
            }
        }

        // SHUFFLE SCREW LISTS
        ShuffleList(noBlockedScrews);
        ShuffleList(blockedScrews);

        List<BaseScrew> shuffledScrews = new List<BaseScrew>();

        float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame = _currentLevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

        // float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame =
        //     levelDataContainer.LevelsData[currentLevel.Value].LevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

        int numNoBlockedScrewsToGetSameFaction = (int)(ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame * noBlockedScrews.Count);
        int numRemainingScrews = numScrew - numNoBlockedScrewsToGetSameFaction;

        // EASY SCREWS
        for (int i = 0; i < numNoBlockedScrewsToGetSameFaction; i++)
        {
            BaseScrew nextScrew = noBlockedScrews.First();

            shuffledScrews.Add(nextScrew);

            noBlockedScrews.Remove(nextScrew);
        }

        // REMAINING SCREWS
        int currentIndex = 0;

        for (int i = 0; i < numRemainingScrews; i++)
        {
            BaseScrew nextScrew;

            if (currentIndex % (GameConstants.NUMBER_SLOT_PER_SCREW_BOX - 1) == 0)
            {
                if (noBlockedScrews.Count > 0)
                {
                    nextScrew = noBlockedScrews.First();

                    noBlockedScrews.Remove(nextScrew);
                }
                else
                {
                    nextScrew = blockedScrews.First();

                    blockedScrews.Remove(nextScrew);
                }
            }
            else
            {
                if (blockedScrews.Count > 0)
                {
                    nextScrew = blockedScrews.First();

                    blockedScrews.Remove(nextScrew);
                }
                else
                {
                    nextScrew = noBlockedScrews.First();

                    noBlockedScrews.Remove(nextScrew);
                }
            }

            shuffledScrews.Add(nextScrew);

            currentIndex++;
        }

        int currentFaction = 0;

        List<GameFaction> remainingFactionForBlockedScrews = new List<GameFaction>();

        for (int i = 0; i < shuffledScrews.Count; i++)
        {
            remainingFactionForBlockedScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

            if (i > 0 && (i + 1) % 3 == 0)
            {
                currentFaction++;

                if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                {
                    currentFaction = 0;
                }
            }
        }

        for (int i = 0; i < shuffledScrews.Count; i++)
        {
            GameFaction nextFaction = remainingFactionForBlockedScrews.First();

            shuffledScrews[i].ScrewId = i;
            shuffledScrews[i].Faction = nextFaction;

            shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            remainingFactionForBlockedScrews.Remove(nextFaction);
        }

        // RAMDOMIZE
        // for (int i = 0; i < shuffledScrews.Count; i++)
        // {
        //     int randomIndex = UnityEngine.Random.Range(0, remainingFactionForBlockedScrews.Count);

        //     shuffledScrews[i].ScrewId = i;
        //     shuffledScrews[i].Faction = remainingFactionForBlockedScrews[randomIndex];

        //     shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

        //     remainingFactionForBlockedScrews.RemoveAt(randomIndex);
        // }

        setLevelScrewNumberEvent?.Invoke(shuffledScrews.Count);
    }

    #region BOOSTER SUGGESTION
    private bool IsShowBoosterSuggestion(int level)
    {
        bool remoteConfigIsShowBoosterSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_BOOSTER_SUGGESTION, true);
        int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 2);

        if (!remoteConfigIsShowBoosterSuggestion)
        {
            return false;
        }

        if (level <= 3 || level % numLevelShowBoosterSuggestion == 0)
        {
            return false;
        }

        return true;
    }

    private void DelayShowBoosterSuggest()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.BoosterSuggest);
        }));
    }
    #endregion

    #region UTIL
    private void RandomizeLevelModelColor(GameObject level)
    {
        BasicObjectPart[] objectParts = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(level.transform).ToArray();
        Renderer[] objectPartRenderers = new Renderer[objectParts.Length];

        // MATERIALS
        List<Material> materialPool = objectPartMaterialContainer.Materials.ToList();

        List<Material> piecesMaterials = new List<Material>();

        for (int i = 0; i < objectParts.Length; i++)
        {
            objectPartRenderers[i] = objectParts[i].GetComponent<MeshRenderer>();
        }

        Material material = null;

        for (int i = 0; i < objectParts.Length; i++)
        {
            if (!materialPool.Contains(objectPartRenderers[i].sharedMaterial))
            {
                return;
            }

            if (!piecesMaterials.Contains(objectPartRenderers[i].sharedMaterial))
            {
                piecesMaterials.Add(objectPartRenderers[i].sharedMaterial);
            }
        }

        Dictionary<Material, int> newMaterialIndexes = new Dictionary<Material, int>();

        const int NUM_GRADIENT_MATERIAL = 9;

        // Random new material for each material index
        foreach (var pieceMaterial in piecesMaterials)
        {
            newMaterialIndexes.Add(
                pieceMaterial, UnityEngine.Random.Range(0, materialPool.Count - NUM_GRADIENT_MATERIAL));
        }

        for (int i = 0; i < objectParts.Length; i++)
        {
            int newMaterialIndex = newMaterialIndexes[objectPartRenderers[i].sharedMaterial];

            objectPartRenderers[i].sharedMaterial = materialPool[newMaterialIndex];
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        System.Random rand = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion
}
