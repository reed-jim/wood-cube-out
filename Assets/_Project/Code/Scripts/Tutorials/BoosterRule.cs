using System;
using NamPhuThuy.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Tutorial
{
    public enum LockOverride
    {
        NONE = 0,
        DEFAULT = 1, 
        FORCE_LOCK = 2, 
        FORCE_UNLOCK = 3
    }

    [Serializable]
    public class BoosterRule
    {
        public GameEnum.BoosterType type;
        public LockOverride lockOverride = LockOverride.DEFAULT;

        [Min(0)]
        [ShowIf(nameof(lockOverride), LockOverride.DEFAULT)]
        public int grantAmount = 0;
    }
}