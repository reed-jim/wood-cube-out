using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using BirdSort;
[CreateAssetMenu(fileName = "SpriteCells", menuName = "ScriptableObject/Sprites", order = 2)]

public class DataSprites : SerializedScriptableObject
{
    public List<DataScripts> Data;
}
public class DataScripts
{
    public int ID;
    public Tile prefabTile;
}
