#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class MissingRefsChecker : IMissingRefsChecker
    { 
        public bool CheckGO(GameObject target)
        {
            return PrefabUtility.GetPrefabType(target) == PrefabType.MissingPrefabInstance;
        }

        public bool CheckComponent(Component target)
        {
            return target == null;
        }
        
        public IEnumerable<string> CheckFieldsAndProps(Object[] comps)
        {
            List<string> results = new List<string>();
            SerializedProperty sp;

            foreach (var comp in comps)
            {
                if (comp == null)
                    continue;

                sp = new SerializedObject(comp).GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                        {
                            if (results.Contains(sp.type))
                                continue;
                            results.Add(sp.type);
                        }
                    }
                }
            }

            return results.Select(v => v.Substring(5, v.Length - 6));
        }
    }
}

#endif
