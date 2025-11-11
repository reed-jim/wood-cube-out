using DG.Tweening;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.UI;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NamPhuThuy.UI
{
    public class ButtonClicky : Button, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual related")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite pressed;

        private Image _image;

        [Space(10)]
        [Header("Scales")]
        [SerializeField] private float pointerHoverScale = 1.1f;
        [SerializeField] private float pointerReleaseScale = 1f;
        [SerializeField] private float pointerClickScale = 0.9f;

        [Space(10)]
        [Header("Flags")]
        public bool isUseClickyFX = true;
        public bool isUseHoverFX = true;
        public bool isActive = true;

        private RectTransform _rectTransform;
        private float _changeY = 5.6f;


        private Vector3 _originalLocalScale;
        private Sequence _sequence;

        protected override void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();

            _originalLocalScale = this.transform.localScale;
        }

        // public void OnPointerUp(PointerEventData eventData)
        // {
        //     if (!isUseClickyFX) return;
        //     transform.localScale = _originalLocalScale;
        //     DOTween.Sequence().Append(transform.DOScale(_originalLocalScale, 0.15f))
        //         .OnComplete(() =>
        //         {
        //             // _image.sprite = _default;
        //         });
        // }

        /*
         If I made this method override: "public override void OnPointerDown", it wont active the onClick event of the Button when I click (UnityEditor context)
         */
        // public void OnPointerDown(PointerEventData eventData)
        // {
        //     if (!isUseClickyFX) return;
        //     transform.localScale = _originalLocalScale * pointerClickScale;
        //     DOTween.Sequence()
        //         .Append(transform.DOScale(_originalLocalScale * pointerClickScale, 0.15f))
        //         .OnComplete((() =>
        //         {
        //             // _image.sprite = _pressed;
        //         }));
        // }

        // public void OnPointerClick(PointerEventData eventData)
        // {
        //     HapticsHelper.MediumVibrate();
        // }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayAnimationOnClick();

            // if (!isUseClickyFX) return;
            // if (!isUseHoverFX) return;


            // DOTween.Sequence().Append(transform.DOScale(_originalLocalScale * pointerHoverScale, 0.2f));
        }

        // public void OnPointerExit(PointerEventData eventData)
        // {
        //     if (!isUseClickyFX) return;
        //     if (!isUseHoverFX) return;

        //     DOTween.Sequence().Append(transform.DOScale(_originalLocalScale, 0.2f));
        // }


        // // detect and handle events when a pointer (e.g., mouse cursor or touch) moves over a UI element
        // public void OnPointerMove(PointerEventData eventData)
        // {
        //     // transform.localScale = localScaleOld * _pointerHoverScale;
        // }

        private void PlayAnimationOnClick()
        {
            if (!isActive) return;
            if (_sequence != null)
            {
                _sequence.Complete();
            }

            _sequence = DOTween.Sequence();

            _sequence.Append(transform.DOScale(1.15f * _originalLocalScale, 0.2f).SetEase(Ease.InOutSine));
            _sequence.Append(transform.DOScale(_originalLocalScale, 0.1f).SetEase(Ease.InOutSine));
        }
        
        #region Public Methods

        public void EnableButton()
        {
            isActive = true;
        }
        
        public void DisableButton()
        {
            isActive = false;
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ButtonClicky))]
    [CanEditMultipleObjects]
    public class ButtonClickyEditor : ButtonEditor
    {
        SerializedProperty _default;
        SerializedProperty _pressed;
        SerializedProperty _pointerHoverScale;
        SerializedProperty _pointerReleaseScale;
        SerializedProperty _pointerClickScale;
        SerializedProperty isUseClickyFX;
        SerializedProperty isUseHoverFX;

        protected override void OnEnable()
        {
            base.OnEnable();

            _default = serializedObject.FindProperty("defaultSprite");
            _pressed = serializedObject.FindProperty("pressed");
            _pointerHoverScale = serializedObject.FindProperty("pointerHoverScale");
            _pointerReleaseScale = serializedObject.FindProperty("pointerReleaseScale");
            _pointerClickScale = serializedObject.FindProperty("pointerClickScale");
            isUseClickyFX = serializedObject.FindProperty("isUseClickyFX");
            isUseHoverFX = serializedObject.FindProperty("isUseHoverFX");
        }

        public override void OnInspectorGUI()
        {
            // Draw the parent (Button) inspector first
            base.OnInspectorGUI();

            // Draw your extra fields
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Clicky Extension", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_default);
            EditorGUILayout.PropertyField(_pressed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scales", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_pointerHoverScale);
            EditorGUILayout.PropertyField(_pointerReleaseScale);
            EditorGUILayout.PropertyField(_pointerClickScale);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isUseClickyFX);
            EditorGUILayout.PropertyField(isUseHoverFX);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
