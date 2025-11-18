using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class ButtonSwitchFade : MonoBehaviour
    {
        
        public enum State
        {
            OFF = 0,
            ON = 1
        }
        #region Private Serializable Fields
        
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;

        [Header("Indicator")]
        [SerializeField] private RectTransform indicator;
        [SerializeField] private RectTransform offTransform;
        [SerializeField] private RectTransform onTransform;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        private State _currentState = State.OFF;
        private Tween _fadeTween;
        private Tween _moveTween;
        
        public State CurrentState => _currentState;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        #endregion

        #region Private Methods
        
        private void ApplyStateImmediate()
        {
            backgroundImage.sprite = _currentState == State.ON ? onSprite : offSprite;
            backgroundImage.color = Color.white;
            indicator.anchoredPosition = _currentState == State.ON ? onTransform.anchoredPosition : offTransform.anchoredPosition;
        }

        private void AnimateToState()
        {
            // Fade background
            Sprite targetSprite = _currentState == State.ON ? onSprite : offSprite;
            Color targetColor = _currentState == State.ON ? onSprite.texture.GetPixel(100, 50) : offSprite.texture.GetPixel(100, 50);

            _fadeTween = DOTween.Sequence()
                .Append(backgroundImage.DOColor(targetColor, fadeDuration))
                .AppendCallback(() =>
                {
                    backgroundImage.sprite = targetSprite;
                    backgroundImage.color = Color.white; 
                });
            

            // Move indicator
            Vector2 targetPosition = _currentState == State.ON ? onTransform.anchoredPosition : offTransform.anchoredPosition;
            _moveTween = indicator.DOAnchorPos(targetPosition, moveDuration).SetEase(moveEase);
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
            _moveTween?.Kill();
        }
        
        #endregion

        #region Public Methods
        
        public void SetState(State newState, bool immediate = false)
        {
            if (_currentState == newState && !immediate) return;

            _currentState = newState;

            // Kill existing tweens
            _fadeTween?.Kill();
            _moveTween?.Kill();

            if (immediate)
            {
                ApplyStateImmediate();
            }
            else
            {
                AnimateToState();
            }
        }

        public void ToggleState()
        {
            SetState(_currentState == State.OFF ? State.ON : State.OFF);
        }
        
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }
        
        #endregion
    }

    /*#if UNITY_EDITOR
    [CustomEditor(typeof(ButtonSwitchFade))]
    [CanEditMultipleObjects]
    public class ButtonSwitchFadeEditor : Editor
    {
        private ButtonSwitchFade script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (ButtonSwitchFade)target;

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