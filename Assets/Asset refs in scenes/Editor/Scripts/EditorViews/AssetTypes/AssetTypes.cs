#if UNITY_EDITOR

using System;

namespace SearchEngine.EditorViews.AssetTypes
{
    [Flags]                                                                                     
    public enum AssetTypes
    {
        AnimationClip=  0x1,
        AudioClip=      0x2,
        AudioMixer =    0x4,
        Font =          0x8,
        GUISkin =       0x10,
        Material=       0x20,
        Texture=        0x40,
        Model =         0x80,
        PhysicMaterial= 0x100,
        Prefab=         0x200,
        Scene=          0x400,
        Script=         0x800,
        Shader=         0x1000,
        VideoClip=      0x2000,
        Other =         0x4000,
    }
}

#endif