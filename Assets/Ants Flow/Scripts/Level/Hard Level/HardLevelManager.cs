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
        [SerializeField] private SkeletonGraphic hardLevelSpine;
        [SerializeField] private Image blackOverlay;
        [SerializeField] private TMP_Text hardLevelText;
        [SerializeField] private LeanLocalizedTextMeshProUGUI localizedHardLevelText;

        [SerializeField] private Material hardTextMaterial;
        [SerializeField] private Material superHardTextMaterial;

        [SpineAnimation] public string appearAnimation;
        [SpineAnimation] public string idleAnimation;
        [SpineSkin] public string hardSkin;
        [SpineSkin] public string superHardSkin;

        public float hardLevelThreshold;
        public float superHardLevelThreshold;

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

                hardLevelSpine.gameObject.SetActive(true);
                hardLevelSpine.color = Color.white;

                if (isSuperHardLevel)
                {
                    hardLevelSpine.Skeleton.SetSkin(superHardSkin);

                    hardLevelText.fontMaterial = superHardTextMaterial;
                    localizedHardLevelText.TranslationName = "Super Hard";
                }
                else
                {
                    hardLevelSpine.Skeleton.SetSkin(hardSkin);

                    hardLevelText.fontMaterial = hardTextMaterial;
                    localizedHardLevelText.TranslationName = "Hard";
                }

                hardLevelSpine.Skeleton.SetToSetupPose();
                hardLevelSpine.AnimationState.ClearTracks();
                TrackEntry entry = hardLevelSpine.AnimationState.SetAnimation(0, appearAnimation, false);
                entry.TrackTime = 0;
                hardLevelSpine.AnimationState.Apply(hardLevelSpine.Skeleton);
                hardLevelSpine.Skeleton.UpdateWorldTransform();
                hardLevelSpine.AnimationState.Complete += OnAnimationComplete;
            }

            void OnAnimationComplete(TrackEntry entry)
            {
                hardLevelSpine.AnimationState.Complete -= OnAnimationComplete;

                hardLevelSpine.AnimationState.SetAnimation(0, idleAnimation, true);

                Tween.Delay(2).OnComplete(() =>
                {
                    Tween.Alpha(hardLevelSpine, 0, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        hardLevelSpine.gameObject.SetActive(false);
                    });
                    SaferioTween.AlphaHide(canvasGroup);
                });
            }
        }

        private void OnLevelLoaded()
        {
            bool isHardLevel = hardLevels.Contains(currentLevel.Value);
            bool isSuperHardLevel = superHardLevels.Contains(currentLevel.Value);
            bool isNotHardLevel = !isHardLevel && !isSuperHardLevel;

            if (GamePersistentVariable.levelItem == null || isNotHardLevel)
            {
                hardLevelSpine.gameObject.SetActive(false);
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
