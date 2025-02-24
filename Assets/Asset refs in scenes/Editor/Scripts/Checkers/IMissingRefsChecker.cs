#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public interface IMissingRefsChecker
    {
        bool CheckGO(GameObject target);
        bool CheckComponent(Component target);
        IEnumerable<string> CheckFieldsAndProps(Object[] comps);
    }
}

#endif