#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using SearchEngine.Additions;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class ACScript : IAssetCheckerType
    {
        [SerializeField] private Dictionary<Type, string> assets;//assetScriptClass, asset
        private HashSet<string> assetsFound = new HashSet<string>();
        [SerializeField] private Dictionary<Type, Type[]> validMonobs;//MonoBehaviourClass, assetScriptClasses

        public ACScript(IEnumerable<string> assets)
        {
            this.assets = new Dictionary<Type, string>();
            foreach (var v in assets)
            {
                var v2 = (AssetDatabase.LoadMainAssetAtPath(v) as MonoScript).GetClass();
                if (v2 != null)
                    this.assets.Add(v2, v);
            }
            validMonobs = new Dictionary<Type, Type[]>();
            this.CheckSerializeFields();
        }

        public IEnumerable<string> CheckGO(GameObject go)
        {
            assetsFound.Clear();
            
            foreach (var comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp != null)
                {
                    CheckAssetContains(comp.GetType());                    
                    CheckFieldsAndProps(comp);
                }
            }
            return assetsFound;
        }

        public void CheckSceneObject(GameObject go){}
        public void CheckFieldObject(object assetField){}

        private void CheckFieldsAndProps(Component comp)
        {
            Type compType = comp.GetType();
            if (!validMonobs.ContainsKey(compType))
            {
                AddNewValidMonob(compType);
            }
            foreach (var v in validMonobs[compType])
            {
                CheckAssetContains(v);
            }
        }

        private void AddNewValidMonob(Type compType)
        {
            HashSet<Type> types = new HashSet<Type>();

            var typeInfo = TypeInfoValidatorFactory.GetTypeInfo(compType);
            foreach (var v in typeInfo.Fields)
            {
                if (assets.ContainsKey(v.FieldType))
                    types.Add(v.FieldType);
            }
            foreach (var v in typeInfo.FieldArrays)
            {
                var type = GetArrayType(v.FieldType);
                if (assets.ContainsKey(type))
                    types.Add(type);
            }
            foreach (var v in typeInfo.Props)
            {
                if (assets.ContainsKey(v.PropertyType))
                    types.Add(v.PropertyType);
            }
            foreach (var v in typeInfo.PropArrays)
            {
                var type = GetArrayType(v.PropertyType);
                if (assets.ContainsKey(type))
                    types.Add(type);
            }

            validMonobs.Add(compType, types.ToArray());
        }
        
        private void CheckAssetContains(Type type)
        {
            if (assets.ContainsKey(type))
            {
                assetsFound.Add(assets[type]);
            }
        }

        private Type GetArrayType(Type t)
        {
            if (t.IsArray)
            {
                return t.GetElementType();
            }
            else
            {
                return t.GetGenericArguments()[0];
            }
        }
    }
}

#endif