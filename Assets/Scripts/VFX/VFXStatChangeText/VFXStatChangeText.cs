using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NamPhuThuy;
using NamPhuThuy.UI;
using NamPhuThuy.VFX;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    public class VFXStatChangeText : VFXBase
    {
        #region Private Serializable Fields
        
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private float moveY = 50f;
        [SerializeField] private float duration = 1f;
        
        [SerializeField] Vector3 startPos;

        #endregion

        #region Private Fields

        private float _value;
        private Transform _initialParent;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            _initialParent = transform.parent;

        }

        void Update()
        {
            
        }

        private void Reset()
        {
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion

        protected override void OnPlay()
        {
            DebugLogger.LogSimple();
            _value = args.amount;
            targetText = args.initialParent.GetComponent<TextMeshProUGUI>();
            //copy properties from targetText to text
            text.CopyProperties(targetText);
            // text.transform.SetParent((args.initialParent));
            text.transform.position = args.initialParent.position;
            text.autoSizeTextContainer = false;
            
            gameObject.SetActive(true);
            startPos = transform.localPosition;
            

            if (_value >= 0)
            {
                text.text = $"{_value}";
                text.color = Color.green;
            }
            else
            {
                text.text = $"{-_value}";
                text.color = Color.red;
            }
            
            
            text.DOFade(1f, 0f); // Ensure visible
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(startPos.y + moveY, duration));
            seq.Join(text.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                transform.localPosition = startPos; // Reset position
                gameObject.SetActive(false);
                transform.SetParent(_initialParent);
            });
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VFXStatChangeText))]
    [CanEditMultipleObjects]
    public class VFXStatChangeTextEditor : Editor
    {
        private VFXStatChangeText script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (VFXStatChangeText)target;

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