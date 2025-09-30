using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    public class ScrewBoxLocked : ScrewBox
    {
        #region Private Serializable

        [SerializeField] private SpecialLevelTaskRecord record;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            BaseScrew.screwLoosenedEvent += OnScrewLoosed;

            _screwBoxLockUI = (ScrewBoxLockUI)screwBoxServiceLocator.screwBoxUI;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
        }

        #region Private Fields
        private ScrewBoxLockUI _screwBoxLockUI;
        private bool _isDoneTaskToUnlock;
        #endregion


        #region Private Methods
        #endregion

        #region Public Methods

        public void Setup(SpecialLevelTaskRecord record)
        {
            _screwBoxLockUI = (ScrewBoxLockUI)screwBoxServiceLocator.screwBoxUI;

            Lock();

            this.record = record;

            _screwBoxLockUI.UpdateUI(this.record);
        }

        public void OnScrewLoosed(BaseScrew screw)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (_isDoneTaskToUnlock)
            {
                return;
            }

            foreach (var componentAmount in record.requiredComponents)
            {
                if (GameComponentType.Screw != componentAmount.componentType) continue;
                if (screw.Faction != componentAmount.faction) continue;

                componentAmount.remainingAmount--;

                if (componentAmount.remainingAmount <= 0)
                {
                    Unlock();
                }
            }

            _screwBoxLockUI.UpdateUI(record);
        }

        #endregion

        #region Override Methods

        public override void Lock()
        {
            DebugLogger.LogSimple();
            screwBoxSpriteRenderer.enabled = false;
            _screwBoxLockUI.Lock();
            isLocked = true;
            _isDoneTaskToUnlock = false;
        }

        public override void Unlock()
        {
            base.Unlock();

            _isDoneTaskToUnlock = true;

            _screwBoxLockUI.Unlock();
        }

        public override bool IsValidToUnlock()
        {
            return isLocked && _isDoneTaskToUnlock;
        }

        #endregion

        #region Editor Methods

        public void ResetValues()
        {

        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ScrewBoxLocked))]
    [CanEditMultipleObjects]
    public class ScrewBoxLockedEditor : Editor
    {
        private ScrewBoxLocked script;
        private Texture2D frogIcon;

        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (ScrewBoxLocked)target;

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
