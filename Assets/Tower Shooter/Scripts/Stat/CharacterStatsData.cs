using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Character")]
public class CharacterStatsData : ScriptableObject
{
    public List<StatValue> stats;
}
