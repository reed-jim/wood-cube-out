using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntsFlow
{
    public class LevelItem : MonoBehaviour
    {
        public List<Color> poolFaction;
        public bool isTutorialLevel;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        // Change these to match your enum values
        private int randomCount = 3;
        private int maxUniqueValueCount = 3;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            randomCount = EditorGUILayout.IntField("Random Count", randomCount);
            maxUniqueValueCount = EditorGUILayout.IntField("Max Unique Value Count", maxUniqueValueCount);

            if (GUILayout.Button("GetPoolFaction"))
            {
                GetPoolFaction(levelItem);
            }
        }

        private void GetPoolFaction(LevelItem levelItem)
        {
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(levelItem.transform);

            levelItem.poolFaction = tiles.Select(item => item.Faction).ToList();

            EditorUtility.SetDirty(levelItem);
        }

        // private void GeneratePoolFaction(LevelItem levelItem, int count, int maxUniqueValue)
        // {
        //     levelItem.poolFaction = new List<GameFaction>();

        //     List<GameFaction> validFactions = CommonUtil.GetRandomEnumValuePool<GameFaction>(maxUniqueValueCount: maxUniqueValue, numLastValuesToSkip: 2);

        //     for (int i = 0; i < count; i++)
        //     {
        //         levelItem.poolFaction.Add(validFactions[Random.Range(0, validFactions.Count)]);
        //     }

        //     EditorUtility.SetDirty(levelItem);
        // }
    }
#endif
}
