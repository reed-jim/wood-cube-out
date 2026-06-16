using System;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class SpineExtensions
{
    public static void Initialize(this IAnimationStateComponent Anim, bool isOverride = false)
    {
        if (Anim is SkeletonAnimation)
        {
            ((SkeletonAnimation)Anim).Initialize(isOverride);
        }
        else
        {
            ((SkeletonGraphic)Anim).Initialize(isOverride);
        }
    }

    private static Dictionary<ISkeletonAnimation, AnimInfo> animDic = new Dictionary<ISkeletonAnimation, AnimInfo>();

    [Serializable]
    public class AnimInfo
    {
        public string animName;
        public bool loop;
        public int priority;
        public float duration;

        public AnimInfo(string animName, bool loop, int priority)
        {
            this.animName = animName;
            this.loop = loop;
            this.priority = priority;
        }
    }

    public static AnimInfo SetAnimation(this ISkeletonAnimation anim, string nameAnim,
        bool loop = false, Action onComplete = null, int priority = 0)
    {
        if (animDic.ContainsKey(anim))
        {
            if (animDic[anim].animName == nameAnim)
            {
                return animDic[anim];
            }
            else
            if (animDic[anim].priority <= priority)
            {
                animDic[anim] = new AnimInfo(nameAnim, loop, priority);
            }
            else
            {
                return animDic[anim];
            }
        }
        else
        {
            animDic.Add(anim, new AnimInfo(nameAnim, loop, priority));
        }

        var animState = (IAnimationStateComponent)(anim);
        var spine = animState.AnimationState.SetAnimation(1, nameAnim, loop);
        spine.Complete += entry =>
        {
            animDic.Remove(anim);
            onComplete?.Invoke();
        };
        animDic[anim].duration = spine.Animation.Duration;
        return animDic[anim];
    }

    public static void SetAnimation(this ISkeletonAnimation anim, string nameAnim, List<string> skinMix, Color color,
        bool loop = false, Action onComplete = null)
    {
        anim.SetAnimation(nameAnim, skinMix, loop, onComplete);
        anim.SetColor(color);
    }


    static void SetColor(this ISkeletonAnimation anim, Color color)
    {
        var skeleton = anim.Skeleton;
        foreach (var slot in skeleton.Slots.Where(s => s.Data.Name.StartsWith("Stickman/")))
            slot.SetColor(color);
        foreach (var slot in skeleton.Slots.Where(s => s.Data.Name.StartsWith("swordsman/stickman")))
            slot.SetColor(color);
    }

    public static void SetAnimation(this ISkeletonAnimation anim, string nameAnim, List<string> skinMix,
        bool loop = false, Action onComplete = null)
    {
        var skeleton = anim.Skeleton;
        var animState = (IAnimationStateComponent)(anim);
        var skeletonData = skeleton.Data;
        var mixAndMatchSkin = new Skin(skinMix[0]);
        foreach (var skinName in skinMix)
            mixAndMatchSkin.AddSkin(skeletonData.FindSkin(skinName));
        skeleton.SetSkin(mixAndMatchSkin);
        skeleton.SetSlotsToSetupPose();
        var spine = animState.AnimationState.SetAnimation(1, nameAnim, loop);
        spine.Complete += entry => onComplete?.Invoke();
    }

    public static void SetAnimation(this ISkeletonAnimation anim, List<string> skinMix)
    {
        var skeleton = anim.Skeleton;
        // var animState = (IAnimationStateComponent) (anim);
        var skeletonData = skeleton.Data;
        var mixAndMatchSkin = new Skin(skinMix[0]);
        foreach (var skinName in skinMix)
            mixAndMatchSkin.AddSkin(skeletonData.FindSkin(skinName));
        skeleton.SetSkin(mixAndMatchSkin);
        skeleton.SetSlotsToSetupPose();
    }

    public static void SetAnimation(this ISkeletonAnimation anim, List<string> skinMix, Color color)
    {
        anim.SetAnimation(skinMix);
        anim.SetColor(color);
    }
}