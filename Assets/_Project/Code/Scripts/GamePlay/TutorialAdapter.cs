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
        [SerializeField] private bool isTutorialLevel = false;
        public bool IsTutorialLevel => isTutorialLevel;
        
        [Header("Stats")] 
        [SerializeField] private int levelId;
        
        [Header("Components")]
        [SerializeField] private List<BasePiece> pieceList;
        [SerializeField] private List<GameEnum.GameFaction> factionList;
        [SerializeField] private TutorialRecord tutorialRecord;
        [SerializeField] private TutorialStepRecord currentStepRecord;

        #endregion

        #region Private Fields
        
        private Dictionary<GameEnum.GameFaction, List<BasePiece>> _basePieceByFaction = new Dictionary<GameEnum.GameFaction, List<BasePiece>>();

        private Coroutine _tutorialRoutine;
        private int _currentStepIndex;
        private bool _isCurrentStepCompleted;
        #endregion
        
        

        #region Public Methods
        
        public void ResetState()
        {
            DebugLogger.Log();
            pieceList.Clear();
            factionList.Clear();
            _basePieceByFaction.Clear();
            
            if (_tutorialRoutine != null)
            {
                StopCoroutine(_tutorialRoutine);
                _tutorialRoutine = null;
            }

            if (TutorialManager.Ins != null && TutorialManager.Ins.TutorialHand != null)
            {
                TutorialManager.Ins.TutorialHand.DisableHand();
            }
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
            if (!isTutorialLevel || tutorialRecord == null || tutorialRecord.Steps == null ||
                tutorialRecord.Steps.Count == 0)
            {
                return;
            }

            if (_tutorialRoutine != null)
            {
                // already running
                return;
            }

            _currentStepIndex = 0;
            _tutorialRoutine = StartCoroutine(RunTutorialSequence());
            
            // OLD 
            /*GameEnum.GameFaction randomFaction = factionList[Random.Range(0, factionList.Count)];
            List<BasePiece> piecesOfFaction = _basePieceByFaction[randomFaction];
            BasePiece randomPiece = piecesOfFaction[Random.Range(0, piecesOfFaction.Count)];
            
            DebugLogger.Log(message: $"randomFaction: {randomFaction}, randomPiece: {randomPiece.name}");
            
            TutorialManager.Ins.TutorialHand.EnableHand();
            TutorialManager.Ins.TutorialHand.PlayAnimation(TutorialManager.Ins.TutorialHand.AnimAction2, true);
            TutorialManager.Ins.MoveHandToScreenPointFromWorld(randomPiece.transform.position);*/
        }
        
        /// <summary>
        /// External call from gameplay when the current step is completed.
        /// For example: called from a listener when user taps correct piece, etc.
        /// </summary>
        public void OnStepCompletedFromGameplay()
        {
            DebugLogger.Log(message: $"Step completed from gameplay at index: {_currentStepIndex}");
            _isCurrentStepCompleted = true;
        }
        
        private IEnumerator RunTutorialSequence()
        {
            while (_currentStepIndex < tutorialRecord.Steps.Count)
            {
                currentStepRecord = tutorialRecord.Steps[_currentStepIndex];
                DebugLogger.Log(message: $"Starting step index: {_currentStepIndex}");

                _isCurrentStepCompleted = false;
                StartStep(currentStepRecord);

                // Wait until gameplay notifies completion
                yield return new WaitUntil(() => _isCurrentStepCompleted);

                DebugLogger.Log(message: $"Finished step index: {_currentStepIndex}");
                _currentStepIndex++;
            }

            DebugLogger.Log(message: "All tutorial steps completed.");
            EndTutorialSequence();

            _tutorialRoutine = null;
        }

        /// <summary>
        /// Start a single step: show hand, highlight piece, lock input, etc.
        /// Customize this logic for each step type.
        /// </summary>
        private void StartStep(TutorialStepRecord step)
        {
            switch (step.Type)
            {
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK:
                    GameEnum.GameFaction randomFaction = factionList[Random.Range(0, factionList.Count)];
                    List<BasePiece> piecesOfFaction = _basePieceByFaction[randomFaction];
                    BasePiece randomPiece = piecesOfFaction[Random.Range(0, piecesOfFaction.Count)];
                    
                    DebugLogger.Log(message: $"StartStep - stepIndex: {_currentStepIndex}, randomFaction: {randomFaction}, randomPiece: {randomPiece.name}");
                    
                    TutorialManager.Ins.TutorialHand.EnableHand();
                    TutorialManager.Ins.TutorialHand.PlayAnimation(TutorialManager.Ins.TutorialHand.AnimAction2, true);
                    TutorialManager.Ins.MoveHandToScreenPointFromWorld(randomPiece.transform.position);
                    break;
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_HOLD:
                    
                    
                    break;
            }
        }

        private void EndTutorialSequence()
        {
            if (TutorialManager.Ins != null && TutorialManager.Ins.TutorialHand != null)
            {
                TutorialManager.Ins.TutorialHand.DisableHand();
            }

            // Optionally trigger an "all steps done" event here.
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