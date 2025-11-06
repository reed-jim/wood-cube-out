using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.UI
{
    
    public static class ScrollRectExtenstion
    {
        /// <summary>
        /// Lock the vertical slide of scroll for {duration}
        /// </summary>
        /// <param name="scrollRect"></param>
        /// <param name="duration"></param>
        public static void LockVerticalScroll(this ScrollRect scrollRect, float duration)
        {
            scrollRect.vertical = false;

            DOVirtual.DelayedCall(duration, () =>
            {
                scrollRect.vertical = true;
            });
        }

        /// <summary>
        /// Lock the vertical slide of scroll for 0.25f
        /// </summary>
        /// <param name="scrollRect"></param>
        public static void LockVerticalScrollALittle(this ScrollRect scrollRect)
        {
            scrollRect.LockVerticalScroll(0.25f);
        }
    }
}