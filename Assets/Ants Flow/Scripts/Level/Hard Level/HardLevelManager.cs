using System.Linq;
using Lean.Localization;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AntsFlow
{
    public class HardLevelManager : Singleton<HardLevelManager>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SkeletonGraphic selectedHardModeSpine;
        [SerializeField] private Image blackOverlay;
        [SerializeField] private TMP_Text hardLevelText;
        [SerializeField] private LeanLocalizedTextMeshProUGUI localizedHardLevelText;

        [SerializeField] private Material hardTextMaterial;
        [SerializeField] private Material superHardTextMaterial;
        public SkeletonGraphic hardLevelSpine;
        public SkeletonGraphic superHardLevelSpine;

        [SpineAnimation] public string appearAnimation;
        // [SpineAnimation] public string idleAnimation;

        public int[] hardLevels;
        public int[] superHardLevels;

        public IntVariable currentLevel;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            LevelLoader.setIsLevelLoadedEvent += OnLevelLoaded;
            SceneTransition.levelSceneTransitionCompleteEvent += OnLevelSceneTransitionComplete;
        }

        void OnDestroy()
        {
            LevelLoader.setIsLevelLoadedEvent -= OnLevelLoaded;
            SceneTransition.levelSceneTransitionCompleteEvent -= OnLevelSceneTransitionComplete;
        }

        public void Setup(LevelItem levelItem)
        {
            bool isHardLevel = hardLevels.Contains(currentLevel.Value);
            bool isSuperHardLevel = superHardLevels.Contains(currentLevel.Value);

            if (isHardLevel || isSuperHardLevel)
            {
                canvasGroup.gameObject.SetActive(true);
                canvasGroup.alpha = 1;

                if (isHardLevel)
                {
                    selectedHardModeSpine = hardLevelSpine;

                    superHardLevelSpine.gameObject.SetActive(false);
                }
                else
                {
                    selectedHardModeSpine = superHardLevelSpine;

                    hardLevelSpine.gameObject.SetActive(false);
                }

                selectedHardModeSpine.gameObject.SetActive(true);
                selectedHardModeSpine.color = Color.white;

                if (isSuperHardLevel)
                {
                    hardLevelText.fontMaterial = superHardTextMaterial;
                    localizedHardLevelText.TranslationName = "Super Hard";
                }
                else
                {
                    hardLevelText.fontMaterial = hardTextMaterial;
                    localizedHardLevelText.TranslationName = "Hard";
                }

                selectedHardModeSpine.Skeleton.SetToSetupPose();
                selectedHardModeSpine.AnimationState.ClearTracks();
                TrackEntry entry = selectedHardModeSpine.AnimationState.SetAnimation(0, appearAnimation, false);
                entry.TrackTime = 0;
                selectedHardModeSpine.AnimationState.Apply(selectedHardModeSpine.Skeleton);
                selectedHardModeSpine.Skeleton.UpdateWorldTransform();
                selectedHardModeSpine.AnimationState.Complete += OnAnimationComplete;
            }

            void OnAnimationComplete(TrackEntry entry)
            {
                selectedHardModeSpine.AnimationState.Complete -= OnAnimationComplete;

                // selectedHardModeSpine.AnimationState.SetAnimation(0, idleAnimation, true);

                Tween.Alpha(selectedHardModeSpine, 0, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        selectedHardModeSpine.gameObject.SetActive(false);
                    });
                SaferioTween.AlphaHide(canvasGroup);

                // Tween.Delay(2).OnComplete(() =>
                // {
                //     Tween.Alpha(selectedHardModeSpine, 0, duration: 0.3f)
                //     .OnComplete(() =>
                //     {
                //         selectedHardModeSpine.gameObject.SetActive(false);
                //     });
                //     SaferioTween.AlphaHide(canvasGroup);
                // });
            }
        }

        private void OnLevelLoaded()
        {
            bool isHardLevel = hardLevels.Contains(currentLevel.Value);
            bool isSuperHardLevel = superHardLevels.Contains(currentLevel.Value);
            bool isNotHardLevel = !isHardLevel && !isSuperHardLevel;

            if (GamePersistentVariable.levelItem == null || isNotHardLevel)
            {
                selectedHardModeSpine.gameObject.SetActive(false);
                SaferioTween.AlphaHide(canvasGroup);
            }
        }

        private void OnLevelSceneTransitionComplete()
        {
            if (GamePersistentVariable.levelItem != null)
            {
                Setup(GamePersistentVariable.levelItem);
            }
        }
    }
}
