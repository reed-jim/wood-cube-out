using NamPhuThuy.GamePlay;
using UnityEngine;

public class WashingMachineStartSlot : MonoBehaviour
{
    public int row;

    public WashingMachineItem washingMachine;

    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelLoaded;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelLoaded;
    }

    private void OnLevelLoaded()
    {
        washingMachine = null;
    }
}
