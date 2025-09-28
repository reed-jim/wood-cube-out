using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{

    public class LockScrewBoxManager : BaseSpecialLevelManager
    {
        #region Private Serializable Fields

        [SerializeField] private List<ScrewBox> screwBoxes;

        #endregion

        #region Private Fields

        private Coroutine _createNewTaskCo;

        #endregion

        public static event Action<ScrewBox, SpecialLevelTaskRecord> requestLockedScrewBox;

        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();

            ScrewBoxManager.screwBoxSpawnedEvent += OnScrewBoxSpawned;
        }

        private void OnDestroy()
        {
            ScrewBoxManager.screwBoxSpawnedEvent -= OnScrewBoxSpawned;
        }

        #endregion

        #region Private Methods

        private void OnScrewBoxSpawned(ScrewBox screwbox)
        {
            screwBoxes.Add(screwbox);

            if (screwBoxes.Count == 2)
            {
                LockBoxes(screwbox);
            }
        }

        private void LockBoxes(ScrewBox refScrewBox)
        {
            SpecialLevelTaskRecord record = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

            requestLockedScrewBox?.Invoke(refScrewBox, record);
            // ScrewBoxLocked screwBoxLocked = CommonUtil.ReplaceAndCopy<ScrewBox, ScrewBoxLocked>(screwBoxes[1].gameObject);
        }

        private void LockBox(int id)
        {

        }

        private IEnumerator IECreateNewTask()
        {
            yield return null;
        }
        #endregion

        #region Override Methods

        public override bool IsFactionLocked(GameEnum.GameFaction faction)
        {
            return base.IsFactionLocked(faction);
        }

        protected override void OnScrewLoosed(BaseScrew screw)
        {
            DebugLogger.LogSimple();

            foreach (var screwBox in screwBoxes)
            {
                // screwBox.OnScrewLoosed(screw);
            }
        }

        #endregion

        #region Public Methods



        #endregion

        #region Event Methods

        private void OnLevelLoaded()
        {

        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {

        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LockScrewBoxManager))]
    [CanEditMultipleObjects]
    public class LockScrewBoxManagerEditor : Editor
    {
        private LockScrewBoxManager script;
        private Texture2D frogIcon;

        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (LockScrewBoxManager)target;

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
#endif
}
