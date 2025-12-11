using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.Common;
using NamPhuThuy.Tutorial;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using Random = UnityEngine.Random;
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

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            YarnGameInput.PieceSelectedWithFactionEvent += OnPieceSelected;
            BasePiece.pieceMovedCompletedEvent += OnPieceMoveCompleted;
            SpoolManager.spoolBoxSpawnedEvent += OnSpoolBoxSpawned;
        }

      

        private void OnDestroy()
        {
            YarnGameInput.PieceSelectedWithFactionEvent -= OnPieceSelected;
            BasePiece.pieceMovedCompletedEvent -= OnPieceMoveCompleted;
            SpoolManager.spoolBoxSpawnedEvent -= OnSpoolBoxSpawned;
        }

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

            _currentStepIndex = 0;
            currentStepRecord = tutorialRecord.Steps[_currentStepIndex];

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
        }

        public void ActiveCurrentTut()
        {
            DebugLogger.Log();
            if (!isTutorialLevel || tutorialRecord == null || tutorialRecord.Steps == null ||
                tutorialRecord.Steps.Count == 0)
            {
                DebugLogger.Log(message: $"No tutorial to run for levelId: {levelId}");
                return;
            }

            if (_tutorialRoutine != null)
            {
                DebugLogger.Log(message: $"Tutorial already running.");
                return;
            }

            _currentStepIndex = 0;

            PrepareForLevel1Tutorial();
            
            _tutorialRoutine = StartCoroutine(RunTutorialSequence());
            
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
            DebugLogger.Log();
            
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
            DebugLogger.Log(message:$"Type: {step.Type}");
            switch (step.Type)
            {
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK:
                    targetPiece = piecesOfFaction[Random.Range(0, piecesOfFaction.Count)];
                    piecesOfFaction.Remove(targetPiece);
                    
                    DebugLogger.Log(message: $"StartStep - stepIndex: {_currentStepIndex}, randomFaction: {targetFaction}, randomPiece: {targetPiece.name}");
                    
                    TutorialManager.Ins.TutorialHand.EnableHand();
                    TutorialManager.Ins.TutorialHand.PlayAnimation(TutorialManager.Ins.TutorialHand.AnimAction2, true);
                    TutorialManager.Ins.TutorialHand.MoveHandToScreenPointFromWorld(targetPiece.transform.position);
                    break;
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_HOLD:
                    
                    break;
            }
        }

        private void EndTutorialSequence()
        {
            DebugLogger.Log();
            if (TutorialManager.Ins != null && TutorialManager.Ins.TutorialHand != null)
            {
                TutorialManager.Ins.TutorialHand.DisableHand();
            }

            // Optionally trigger an "all steps done" event here.
        }
        #endregion

        #region  Events Listen

        private void OnPieceSelected(GameEnum.GameFaction faction)
        {
            DebugLogger.Log();
            if (currentStepRecord.Type == TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK && faction == targetFaction)
            {
                _isCurrentStepCompleted = true;
            }
        }
        
        private void OnPieceMoveCompleted(BasePiece obj)
        {
            switch (currentStepRecord.Type)
            {
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK:
                    TutorialManager.Ins.TutorialHand.MoveHandToScreenPointFromWorld(targetPiece.transform.position);
                    break;
            }
        }

        private void OnSpoolBoxSpawned(SpoolBox spoolBox)
        {
            DebugLogger.Log(message: $"Type: {currentStepRecord.Type}" );
            switch (currentStepRecord.Type)
            {
                case TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK:
                    factionList.Add(spoolBox.Faction);
                    break;
            }
            
        }

        #endregion

        #region LEVEL 1 TUTORIAL

        private GameEnum.GameFaction targetFaction;
        List<BasePiece> piecesOfFaction;
        BasePiece targetPiece;
        private void PrepareForLevel1Tutorial()
        {
            DebugLogger.Log(message:$"faction count: {factionList.Count}");
            // factionList = _basePieceByFaction.Keys.ToList();
            /*factionList = SpoolManager.Instance.SpoolBoxes
                .Select(box => box.Faction)              // take the faction from each box
                .Distinct()                              // optional: remove duplicates
                .ToList();              // materialize as List<GameEnum.GameFaction>*/
            
            targetFaction = factionList[Random.Range(0, factionList.Count)];
            piecesOfFaction = _basePieceByFaction[targetFaction];
            
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