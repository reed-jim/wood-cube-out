using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.Common;
using NamPhuThuy.Tutorial;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    public class TutorialAdapter : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Flags")]
        [SerializeField] private bool isTutorialLevel;
        public bool IsTutorialLevel => isTutorialLevel;
        
        [Header("Stats")] 
        [SerializeField] private int levelId;
        
        [Header("Components")]
        [SerializeField] private List<BasePiece> pieceList;
        [SerializeField] private List<GameEnum.GameFaction> factionList;
        [SerializeField] private TutorialRecord tutorialRecord;

        #endregion

        #region Private Fields
        
        private Dictionary<GameEnum.GameFaction, List<BasePiece>> _basePieceByFaction = new Dictionary<GameEnum.GameFaction, List<BasePiece>>();

        #endregion
        
        

        #region Public Methods
        
        public void ResetState()
        {
            DebugLogger.Log();
            pieceList.Clear();
            factionList.Clear();
            _basePieceByFaction.Clear();
            TutorialManager.Ins.TutorialHand.DisableHand();
        }

        public void InitData(Transform levelTransform, int levelId)
        {
            DebugLogger.Log();
            if (levelTransform == null)
            {
                return;
            }

            // Set Values
            this.levelId = levelId;
            tutorialRecord = TutorialManager.Ins.Data.GetTutRecord(levelId);
            if (tutorialRecord == null)
            {
                DebugLogger.LogWarning(message: $"No tutorial record found for levelId: {levelId}");
                isTutorialLevel = false;
                return;
            }

            isTutorialLevel = true;

            foreach (BasePiece basePiece in levelTransform.GetComponentsArrayFromAllChildren<BasePiece>())
            {
                pieceList.Add(basePiece);
                GameEnum.GameFaction faction = basePiece.Faction;
                if (!_basePieceByFaction.TryGetValue(faction, out var list))
                {
                    list = new List<BasePiece>();
                    _basePieceByFaction[faction] = list;
                }
                list.Add(basePiece);
            }
            
            factionList = _basePieceByFaction.Keys.ToList();
        }

        public void ActiveCurrentTut()
        {
            DebugLogger.Log();
            GameEnum.GameFaction randomFaction = factionList[Random.Range(0, factionList.Count)];
            List<BasePiece> piecesOfFaction = _basePieceByFaction[randomFaction];
            BasePiece randomPiece = piecesOfFaction[Random.Range(0, piecesOfFaction.Count)];
            
            DebugLogger.Log(message: $"randomFaction: {randomFaction}, randomPiece: {randomPiece.name}");
            
            TutorialManager.Ins.TutorialHand.EnableHand();
            TutorialManager.Ins.TutorialHand.PlayAnimation(TutorialManager.Ins.TutorialHand.AnimAction2, true);
            TutorialManager.Ins.MoveHandToScreenPointFromWorld(randomPiece.transform.position);
        }
        
        #endregion

        #region LEVEL 1 TUTORIAL

        private IEnumerator ActiveTutLevel1()
        {
            yield return null;
        }
        

        #endregion
        
        #region Private Methods

       
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(TutorialAdapter))]
    [CanEditMultipleObjects]
    public class TutorialAdapterEditor : Editor
    {
        private TutorialAdapter script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (TutorialAdapter)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif*/
}