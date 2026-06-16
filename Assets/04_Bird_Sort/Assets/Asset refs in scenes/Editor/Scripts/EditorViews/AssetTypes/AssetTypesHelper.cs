#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.AssetTypes
{
    public static class AssetTypesHelper
    {
        static AssetTypesHelper()
        {
            EnumSum = 0;
            foreach (int v in Enum.GetValues(typeof(AssetTypes)))
            {
                EnumSum += v;
            }
        }
        public static int EnumSum { get; private set; }
        
        public static AssetTypes GetType(string assetPath)
        {
            var ext = Path.GetExtension(assetPath);

            if (string.IsNullOrEmpty(ext))
                return AssetTypes.Other;

            ext = ext.Substring(1, ext.Length - 1);

            Dictionary< AssetTypes, string[]> exts = new Dictionary<AssetTypes, string[]>();
            exts.Add(AssetTypes.AnimationClip, new string[]{"anim"});
            exts.Add(AssetTypes.AudioClip, new string[]{ "wav", "mp3", "ogg", "aif", "aiff", "mod", "it", "s3m", "xm" });
            exts.Add(AssetTypes.AudioMixer, new string[]{ "mixer" });            
            exts.Add(AssetTypes.Font, new string[]{ "ttf" });            
            exts.Add(AssetTypes.GUISkin, new string[]{ "guiskin" });            
            exts.Add(AssetTypes.Material, new string[]{ "mat" });            
            exts.Add(AssetTypes.Model, new string[]{ "fbx", "dae", "3ds", "dxf", "obj", "skp" });            
            exts.Add(AssetTypes.PhysicMaterial, new string[]{ "physicMaterial" });    
            exts.Add(AssetTypes.Prefab, new string[]{ "prefab" });    
            exts.Add(AssetTypes.Scene, new string[]{ "unity" });    
            exts.Add(AssetTypes.Script, new string[]{ "cs" });    
            exts.Add(AssetTypes.Shader, new string[]{ "shader" });    
            exts.Add(AssetTypes.Texture, new string[]{ "png", "psd", "tga", "bmp", "jpg", "EXR", "GIF", "HDR", "IFF", "PICT", "TIFF" });            
            exts.Add(AssetTypes.VideoClip, new string[]{ "mov", "mpg", "mpeg", "mp4", "avi", "asf" });

            foreach (var v in exts)
            {
                if (v.Value.Any(v2 => String.Compare(v2, ext, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return v.Key;
                }
            }

            return AssetTypes.Other;
        }
        public static AssetTypes GetType(Object o)
        {
            if(o==null)
                return AssetTypes.Other;

            if (o is AnimationClip)
                return AssetTypes.AnimationClip;
            if (o is AudioClip)
                return AssetTypes.AudioClip;
            if (o is AudioMixer)
                return AssetTypes.AudioMixer;
            if (o is Font)
                return AssetTypes.Font;
            if (o is GUISkin)
                return AssetTypes.GUISkin;
            if (o is Material)
                return AssetTypes.Material;
            if (PrefabUtility.GetPrefabType(o) == PrefabType.ModelPrefab)
                return AssetTypes.Model;
            if (o is PhysicsMaterial)
                return AssetTypes.PhysicMaterial;
            if (PrefabUtility.GetPrefabType(o) == PrefabType.Prefab)
                return AssetTypes.Prefab;
            if (o is SceneAsset)
                return AssetTypes.Scene;
            if (o is MonoScript)
                return AssetTypes.Script;
            if (o is Shader)
                return AssetTypes.Shader;
            if (o is Texture)
                return AssetTypes.Texture;
            if (o is VideoClip)
                return AssetTypes.VideoClip;
            return AssetTypes.Other;
        }

        public static bool CheckAsset(AssetTypes assetType, AssetTypes mask)
        {
            return (int)(assetType & mask) > 0;
        }
        public static bool CheckAsset(Object assetObj, AssetTypes mask)
        {
            return CheckAsset(GetType(assetObj), mask);
        }
        public static bool CheckAsset(string assetPath, AssetTypes mask)
        {
            return CheckAsset(GetType(assetPath), mask);
        }
        public static bool CheckAsset(int assetType, AssetTypes mask)
        {
            return CheckAsset((AssetTypes)assetType, mask);
        }
    }
}

#endif