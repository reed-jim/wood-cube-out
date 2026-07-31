using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using BirdSort;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObject/LevelTripleBird", order = 1)]
public class LevelTripleBird : SerializedScriptableObject
{
    public List<Color> KeyColor = new List<Color>() { Color.black, Color.white, Color.yellow };
    public List<Color[,]> lsArray = new List<Color[,]>() { new Color[10, 10] };
    [HideInInspector] public List<CountIdTile> countIdTiles;
    public List<Vector3> lsPos;
    public List<QualityOfID> lsQualityId;
    public List<int> lsIdLevel;
    public float ScaleObjtect;
    [Button]
    public void SpawnArray()
    {
        if (lsArray.Count <= 0)
            return;
        Color[,] lastArray = lsArray[lsArray.Count - 1];
        Color[,] newArray = new Color[lastArray.GetLength(0), lastArray.GetLength(1)];
        lsArray.Add(newArray);
    }
    [Button]
    public void LoadData()
    {
        lsIdLevel.Clear();
        countIdTiles.Clear();
        for (int i = 0; i < lsQualityId.Count; i++)
        {
            var tempCountID = new CountIdTile(lsQualityId[i].id);
            countIdTiles.Add(tempCountID);
            for (int j = 0; j < lsQualityId[i].numb; j++)
            {
                lsIdLevel.Add(lsQualityId[i].id);
            }
        }
        lsIdLevel = Helper.DisruptiveListObject(lsIdLevel);
    }
}
[Serializable]
public class QualityOfID
{
    public int id;
    public int numb;
}