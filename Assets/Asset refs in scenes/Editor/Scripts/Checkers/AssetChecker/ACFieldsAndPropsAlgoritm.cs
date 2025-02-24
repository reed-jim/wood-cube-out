#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using SearchEngine.Additions;

namespace SearchEngine.Checkers
{
    public class ACFieldsAndPropsAlgoritm
    {
        [SerializeField] protected IAssetCheckerType checker;
        protected HashSet<Type> validTypes;
        protected HashSet<Type> validParentTypes;
        protected Dictionary<Type, TypeInfo> validTypeInfos = new Dictionary<Type, TypeInfo>();
        
        public ACFieldsAndPropsAlgoritm(IAssetCheckerType checker, HashSet<Type> validTypes, HashSet<Type> validParentTypes)
        {
            this.checker = checker;
            this.validTypes = validTypes;
            this.validParentTypes = validParentTypes;
            this.CheckSerializeFields();
            
            validTypeInfos = TypeInfoValidatorFactory.GetTypeInfoDictionaryCleanFromStandartErrorsAndWarnings();
            foreach (var v in validTypeInfos.Keys)
            {
                if (validTypeInfos[v] != null)
                {
                    TypeInfoValidatorFactory.RemoveUselessFieldsAndProps(validTypeInfos[v]);        
                }
            }
            foreach (var v in validTypeInfos)
            {
                ValidateTypeInfo(v.Value);
            }
        }
        
        public void CheckComponentsVariables(GameObject target)
        {
            foreach (var comp in target.GetComponents<Component>())
            {
                if (comp != null)
                    CheckFieldsAndProps(comp);
            }
        }

        public void CheckFieldsAndProps(Component comp)
        {
            Type compType = comp.GetType();
            TypeInfo typeInfo = null;
            if (!validTypeInfos.ContainsKey(compType))
            {
                CreateNewTypeInfo(compType, comp);
            }
            typeInfo = validTypeInfos[compType];
            if(typeInfo==null)
                return;

            foreach (var v in typeInfo.Fields)      checker.CheckFieldObject(v.GetValue(comp));
            foreach (var v in typeInfo.FieldArrays) ArrayCheck(v.GetValue(comp), comp);
            foreach (var v in typeInfo.Props)       checker.CheckFieldObject(v.GetValue(comp, null));
            foreach (var v in typeInfo.PropArrays)  ArrayCheck(v.GetValue(comp, null), comp);
        }

        protected TypeInfo CreateNewTypeInfo(Type compType, Component comp)
        {
            var typeInfo = TypeInfoValidatorFactory.GetTypeInfo(compType);
            TypeInfoValidatorFactory.RemoveUselessFieldsAndProps(typeInfo);
            TypeInfoValidatorFactory.RemoveAcsesTryCatchErrors(typeInfo, comp);
            ValidateTypeInfo(typeInfo);
            if (typeInfo.IsEmpty)
                typeInfo = null;

            validTypeInfos.Add(compType, typeInfo);
            return typeInfo;
        }

        private void ValidateTypeInfo(TypeInfo tInfo)
        {
            TypeInfoValidatorFactory.ValidateTypeInfo(tInfo, validTypes, validParentTypes);
        }

        private void ArrayCheck(object field, Component comp)
        {
            var array = field as IEnumerable;
            if (array == null)
                return;
            foreach (var v in array)
            {
                checker.CheckFieldObject(v);
            }
        }
    }
}

#endif