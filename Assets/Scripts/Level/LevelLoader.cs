using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class LevelLoader : MonoBehaviour
{
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;

    public static event Action startLevelEvent;
    public static event Action<int> setLevelScrewNumberEvent;
    public static event Action showSceneTransitionEvent;

    private void Awake()
    {
        PausePopup.replayLevelEvent += Replay;
        LosePopup.replayLevelEvent += Replay;
        DebugPopup.toLevelEvent += GoLevel;
        GameplayScreen.nextLevelEvent += NextLevel;
        GameplayScreen.prevLevelEvent += PrevLevel;
        WinPopup.nextLevelEvent += NextLevel;
        WinPopup.goLevelEvent += GoLevel;

        currentLevel.Load();

        GoLevel(currentLevel.Value);
    }

    void OnDestroy()
    {
        PausePopup.replayLevelEvent -= Replay;
        LosePopup.replayLevelEvent -= Replay;
        DebugPopup.toLevelEvent -= GoLevel;
        GameplayScreen.nextLevelEvent -= NextLevel;
        GameplayScreen.prevLevelEvent -= PrevLevel;
        WinPopup.nextLevelEvent -= NextLevel;
        WinPopup.goLevelEvent -= GoLevel;
    }

    private void LoadLevel()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>($"Level {currentLevel.Value}");

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = Instantiate(op.Result, transform);

                AutoAssignScrewFaction(level);
                AutoAssignScrewFactionMultiPhaseLevel(level);
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

        if (screws.Length == 0)
        {
            return;
        }

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

        setLevelScrewNumberEvent?.Invoke(screws.Length);
    }

    private void AutoAssignScrewFaction(GameObject level)
    {
        BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int currentFaction = 0;

        List<GameFaction> remainingFactionForScrews = new List<GameFaction>();

        for (int i = 0; i < screws.Length; i++)
        {
            remainingFactionForScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

            if (i > 0 && (i + 1) % 3 == 0)
            {
                currentFaction++;

                if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                {
                    currentFaction = 0;
                }
            }
        }

        for (int i = 0; i < screws.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrews.Count);

            screws[i].ScrewId = i;
            screws[i].Faction = remainingFactionForScrews[randomIndex];

            screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            remainingFactionForScrews.RemoveAt(randomIndex);
        }

        setLevelScrewNumberEvent?.Invoke(screws.Length);
    }
}
