#if UNITY_EDITOR

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SearchEngine.Checkers
{
    public static class TypeInfoValidatorFactory
    {
        public static TypeInfo GetTypeInfo(Type type)
        {
            return new TypeInfoGetter().GetValidTypeInfo(type);
        }
        private class TypeInfoGetter
        {
            private const BindingFlags flagsPublic = BindingFlags.Public | BindingFlags.Instance;
            private const BindingFlags flagsNonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        
            public TypeInfo GetValidTypeInfo(Type type)
            {
                var validFields = GetAllFields(type);
                var validProperties = GetAllProperties(type);

                return new TypeInfo(
                    GetNotArrayFields(validFields), 
                    GetArrayFields(validFields), 
                    GetNotArrayProperties(validProperties), 
                    GetArrayProperties(validProperties));
            }
            
            private FieldInfo[] GetAllFields(Type compType)
            {
                List<FieldInfo> fi = new List<FieldInfo>();
                var fields = compType.GetFields(flagsNonPublic);
                fi.AddRange(fields.Where(field => 
                    field.GetCustomAttributes(typeof(SerializeField), false).Any()));
                fields = compType.GetFields(flagsPublic);
                fi.AddRange(fields);
                return fi.ToArray();
            }

            private FieldInfo[] GetNotArrayFields(FieldInfo[] fields)
            {
                return fields.Where(f => !IsListOrArray(f.FieldType)).ToArray();
            }

            private FieldInfo[] GetArrayFields(FieldInfo[] fields)
            {
                return fields.Where(f => IsListOrArray(f.FieldType)).ToArray();
            }

            private PropertyInfo[] GetAllProperties(Type compType)
            {
                List<PropertyInfo> props = new List<PropertyInfo>();
                var properties = compType.GetProperties(flagsPublic);
                props.AddRange(properties.Where(p =>
                    !p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()));
                return props.ToArray();
            }

            private PropertyInfo[] GetNotArrayProperties(PropertyInfo[] properties)
            {
                return properties.Where(p => !IsListOrArray(p.PropertyType)).ToArray();
            }

            private PropertyInfo[] GetArrayProperties(PropertyInfo[] properties)
            {
                return properties.Where(p => IsListOrArray(p.PropertyType)).ToArray();
            }

            private bool IsListOrArray(Type type)
            {
                return type.IsArray || type.Name == "List`1";
            }             
        }        

        public static Dictionary<Type, TypeInfo> GetTypeInfoDictionaryCleanFromStandartErrorsAndWarnings()
        {
            return new TypeInfoValidator2().GetTypeInfoDictionaryCleanFromStandartErrorsAndWarnings();
        }
        private class TypeInfoValidator2
        {
            private Dictionary<Type, TypeInfo> validFieldsAndProps = new Dictionary<Type, TypeInfo>();
            
            public Dictionary<Type, TypeInfo> GetTypeInfoDictionaryCleanFromStandartErrorsAndWarnings()
            {
                FillValidFieldsAndProps();
                return validFieldsAndProps;
            }
            
            private void FillValidFieldsAndProps()
            {
                validFieldsAndProps.Add(typeof(Component), null);
                validFieldsAndProps.Add(typeof(Transform), null);
                validFieldsAndProps.Add(typeof(UnityEngine.AI.NavMeshAgent), null);
                validFieldsAndProps.Add(typeof(Rigidbody), null);

                //Unity Editor when get acces to some properties
                    //throws errors
                validFieldsAndProps.Add(typeof(MeshFilter), 
                    ExcludeFromValidTypeInfoProps(typeof(MeshFilter), 
                    new string[] { "mesh" }, new string[]{}));
                validFieldsAndProps.Add(typeof(Renderer), 
                    ExcludeFromValidTypeInfoProps(typeof(Renderer), 
                    new string[] { "material" }, new string[] { "materials" }));
                AddValidTypeInfoForRendererSubclasses();
                validFieldsAndProps.Add(typeof(UnityEngine.UI.Text), 
                    ExcludeFromValidTypeInfoProps(typeof(UnityEngine.UI.Text), 
                    new string[] { "material" }, new string[0]{}));
                    //throws warnings
                validFieldsAndProps.Add(typeof(Animator), 
                    ExcludeFromValidTypeInfoProps(typeof(Animator), 
                    new string[0] { }, new string[] { "parameters" }));
            }

            private void AddValidTypeInfoForRendererSubclasses()
            {
                var a = Assembly.GetAssembly(typeof(Renderer));
                var rendererSubclasses = a.GetTypes().Where(x => x.IsSubclassOf(typeof(Renderer))).ToArray();
                foreach (var renderer in rendererSubclasses)
                {
                    validFieldsAndProps.Add(renderer,
                        ExcludeFromValidTypeInfoProps(renderer, new string[] { "material" }, new string[] { "materials" }));
                }
            }

            private TypeInfo ExcludeFromValidTypeInfoProps(Type type, string[] properties, string[] arrayProperties)
            {
                TypeInfo vti = Checkers.TypeInfoValidatorFactory.GetTypeInfo(type);

                vti.Props = vti.Props.Where(p => !properties.Contains(p.Name)).ToArray();
                vti.PropArrays = vti.PropArrays.Where(p => !arrayProperties.Contains(p.Name)).ToArray();

                return vti;
            }
        }

        public static void RemoveAcsesTryCatchErrors(TypeInfo typeInfo, Component comp)
        {
            new AcsesTryCatchErrorsRemover().RemoveAcsesTryCatchErrors(typeInfo, comp);
        }
        private class AcsesTryCatchErrorsRemover
        {
            public void RemoveAcsesTryCatchErrors(TypeInfo typeInfo, Component comp)
            {
                typeInfo.Fields = GetValidFields(typeInfo.Fields, comp);
                typeInfo.FieldArrays = GetValidFields(typeInfo.FieldArrays, comp);
                typeInfo.Props = GetValidProps(typeInfo.Props, comp);
                typeInfo.PropArrays = GetValidProps(typeInfo.PropArrays, comp);
            }

            private FieldInfo[] GetValidFields(FieldInfo[] fields, Component comp)
            {
                System.Collections.Generic.List<FieldInfo> validList = new System.Collections.Generic.List<FieldInfo>();
                for (int i = 0; i < fields.Length; i++)
                {
                    try
                    {
                        fields[i].GetValue(comp);
                    }
                    catch
                    {
                        continue;
                    }
                    validList.Add(fields[i]);
                }
                return validList.ToArray();
            }

            private PropertyInfo[] GetValidProps(PropertyInfo[] properties, Component comp)
            {
                System.Collections.Generic.List<PropertyInfo> validList = new System.Collections.Generic.List<PropertyInfo>();
                for (int i = 0; i < properties.Length; i++)
                {
                    try
                    {
                        properties[i].GetValue(comp, null);
                    }
                    catch
                    {

                        continue;
                    }
                    validList.Add(properties[i]);
                }
                return validList.ToArray();
            }
        }

        public static void RemoveUselessFieldsAndProps(TypeInfo typeInfo)
        {
            new UselessFieldsAndPropsRemover().RemoveUselessFieldsAndProps(typeInfo);
        }
        private class UselessFieldsAndPropsRemover
        {
            public void RemoveUselessFieldsAndProps(TypeInfo typeInfo)
            {
                typeInfo.Fields = GetUsefullFields(typeInfo.Fields);
                typeInfo.FieldArrays = GetUsefullFieldArrays(typeInfo.FieldArrays);
                typeInfo.Props = GetUsefullProps(typeInfo.Props);
                typeInfo.PropArrays = GetUsefullPropArrays(typeInfo.PropArrays);
            }

            private FieldInfo[] GetUsefullFields(FieldInfo[] fields)
            {
                return fields.Where(f =>
                    CheckType(f.FieldType)
                ).ToArray();
            }

            private FieldInfo[] GetUsefullFieldArrays(FieldInfo[] fields)
            {
                return fields.Where(f =>
                    CheckArrayType(f.FieldType)
                ).ToArray();
            }
            
            private PropertyInfo[] GetUsefullPropArrays(PropertyInfo[] props)
            {
                return props.Where(f =>
                    CheckArrayType(f.PropertyType)
                ).ToArray();
            }

            private PropertyInfo[] GetUsefullProps(PropertyInfo[] props)
            {
                return props.Where(f =>
                    CheckType(f.PropertyType)
                    && !String.Equals(f.Name, "gameObject")
                    && !String.Equals(f.Name, "transform")
                ).ToArray();
            }

            private bool CheckType(Type type)
            {
                return !(
                    type.IsPrimitive
                    || type.IsValueType
                    || type.IsEnum
                    || type == typeof(string)
                );
            }

            private bool CheckArrayType(Type t)
            {
                if (t.IsArray)
                {
                    return CheckType(t.GetElementType());
                }
                if (t.Name.Equals("List`1"))
                {
                    return CheckType(t.GetGenericArguments()[0]);
                }
                return true;
            }
        }

        public static void ValidateTypeInfo(TypeInfo typeInfo, HashSet<Type> types, HashSet<Type> parentTypes = null)
        {
            if(typeInfo == null 
                || types == null && parentTypes == null)
                return;

            typeInfo.Fields = typeInfo.Fields.Where(v =>
                types != null && types.Contains(v.FieldType)
                || parentTypes != null && parentTypes.Any(v2 => v2.IsAssignableFrom(v.FieldType))
            ).ToArray();
            typeInfo.FieldArrays = typeInfo.FieldArrays.Where(v =>
                types != null && types.Contains(GetArrayType(v.FieldType))
                || parentTypes != null && parentTypes.Any(v2 => v2.IsAssignableFrom(GetArrayType(v.FieldType)))
            ).ToArray();

            typeInfo.Props = typeInfo.Props.Where(v =>
                types != null && types.Contains(v.PropertyType)
                || parentTypes != null && parentTypes.Any(v2 => v2.IsAssignableFrom(v.PropertyType))
            ).ToArray();
            typeInfo.PropArrays = typeInfo.PropArrays.Where(v =>
                types != null && types.Contains(GetArrayType(v.PropertyType))
                || parentTypes != null && parentTypes.Any(v2 => v2.IsAssignableFrom(GetArrayType(v.PropertyType)))
            ).ToArray();
        }
        private static Type GetArrayType(Type t)
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
