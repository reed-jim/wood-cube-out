using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfigContainer", menuName = "Scriptable Objects/Police Simulation/LevelConfigContainer")]
public class LevelConfigContainer : ScriptableObject
{
    public LevelConfig[] levelConfigs;
}
