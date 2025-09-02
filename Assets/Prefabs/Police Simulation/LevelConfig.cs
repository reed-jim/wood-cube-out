using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/Police Simulation/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public SuspectResult[] suspectResults;
}

public class LevelDecision
{

}

[System.Serializable]
public class SuspectResult
{
    public List<DecisionIntPair> results;
}

[System.Serializable]
public class DecisionIntPair
{
    public Decision key;
    public int value;
}
