#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Memento
{
    public static class CopyAbleHelper
    {
        public static T[] ToArray<T>(IEnumerable<T> data)
            where T : ICopyAble<T>
        {
            return data.Select(v => v.Copy()).ToArray();
        }
        public static List<T> ToList<T>(IEnumerable<T> data)
            where T : ICopyAble<T>
        {
            return data.Select(v => v.Copy()).ToList();
        }
        public static IEnumerable<T> ToEnum<T>(IEnumerable<T> data)
            where T : ICopyAble<T>
        {
            return data.Select(v => v.Copy());
        }



        public static Dictionary<T1, T2> Copy00<T1, T2>(IEnumerable<KeyValueMemento<T1, T2>> data)
        {
            return data.ToDictionary(v => v.Key, v => v.Value);
        }
        public static Dictionary<T1, T2> Copy10<T1, T2>(IEnumerable<KeyValueMemento<T1, T2>> data)
            where T1 : ICopyAble<T1>
        {
            return data.ToDictionary(v => v.Key.Copy(), v => v.Value);
        }
        public static Dictionary<T1, T2> Copy01<T1, T2>(IEnumerable<KeyValueMemento<T1, T2>> data)
            where T2 : ICopyAble<T2>
        {
            return data.ToDictionary(v => v.Key, v => v.Value.Copy());
        }
        public static Dictionary<T1, T2> Copy11<T1, T2>(IEnumerable<KeyValueMemento<T1, T2>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.ToDictionary(v => v.Key.Copy(), v => v.Value.Copy());
        }

        public static IEnumerable<KeyValuePair<T1, T2>> Copy00<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> data)
        {
            return data.Select(v => new KeyValuePair<T1, T2>(v.Key, v.Value));
        }
        public static IEnumerable<KeyValuePair<T1, T2>> Copy10<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> data)
            where T1 : ICopyAble<T1>
        {
            return data.Select(v => new KeyValuePair<T1, T2>(v.Key.Copy(), v.Value));
        }
        public static IEnumerable<KeyValuePair<T1, T2>> Copy01<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> data)
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2>(v.Key, v.Value.Copy()));
        }
        public static IEnumerable<KeyValuePair<T1, T2>> Copy11<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2>(v.Key.Copy(), v.Value.Copy()));
        }



        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy00<T1, T2>(IEnumerable<KeyValueMemento<T1, T2[]>> data)
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key, v.Value.ToArray()));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy00<T1, T2>(IEnumerable<KeyValueMemento<T1, List<T2>>> data)
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key, v.Value.ToList()));
        }
        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy10<T1, T2>(IEnumerable<KeyValueMemento<T1, T2[]>> data)
            where T1 : ICopyAble<T1>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key.Copy(), v.Value.ToArray()));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy10<T1, T2>(IEnumerable<KeyValueMemento<T1, List<T2>>> data)
            where T1 : ICopyAble<T1>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key.Copy(), v.Value.ToList()));
        }

        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy01<T1, T2>(IEnumerable<KeyValueMemento<T1, T2[]>> data)
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key, ToArray(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy01<T1, T2>(IEnumerable<KeyValueMemento<T1, List<T2>>> data)
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key, ToList(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy11<T1, T2>(IEnumerable<KeyValueMemento<T1, T2[]>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key.Copy(), ToArray(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy11<T1, T2>(IEnumerable<KeyValueMemento<T1, List<T2>>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key.Copy(), ToList(v.Value)));
        }

        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy00<T1, T2>(IEnumerable<KeyValuePair<T1, T2[]>> data)
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key, v.Value.ToArray()));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy00<T1, T2>(IEnumerable<KeyValuePair<T1, List<T2>>> data)
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key, v.Value.ToList()));
        }
        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy10<T1, T2>(IEnumerable<KeyValuePair<T1, T2[]>> data)
            where T1 : ICopyAble<T1>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key.Copy(), v.Value.ToArray()));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy10<T1, T2>(IEnumerable<KeyValuePair<T1, List<T2>>> data)
            where T1 : ICopyAble<T1>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key.Copy(), v.Value.ToList()));
        }

        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy01<T1, T2>(IEnumerable<KeyValuePair<T1, T2[]>> data)
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key, ToArray(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy01<T1, T2>(IEnumerable<KeyValuePair<T1, List<T2>>> data)
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key, ToList(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, T2[]>> Copy11<T1, T2>(IEnumerable<KeyValuePair<T1, T2[]>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, T2[]>(v.Key.Copy(), ToArray(v.Value)));
        }
        public static IEnumerable<KeyValuePair<T1, List<T2>>> Copy11<T1, T2>(IEnumerable<KeyValuePair<T1, List<T2>>> data)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
        {
            return data.Select(v => new KeyValuePair<T1, List<T2>>(v.Key.Copy(), ToList(v.Value)));
        }



        public static T3[] Copy00<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2>> data, T3 type)
            where T3 : KeyValueMemento<T1, T2>, new()
        {
            return CopyAlg(Copy00(data), type);
        }
        public static T3[] Copy10<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T3 : KeyValueMemento<T1, T2>, new()
        {
            return CopyAlg(Copy10(data), type);
        }
        public static T3[] Copy01<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2>> data, T3 type)
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, T2>, new()
        {
            return CopyAlg(Copy01(data), type);
        }
        public static T3[] Copy11<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, T2>, new()
        {
            return CopyAlg(Copy11(data), type);
        }

        public static T3[] Copy00<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2[]>> data, T3 type)
            where T3 : KeyValueMemento<T1, T2[]>, new()
        {
            return CopyAlg(Copy00(data), type);
        }
        public static T3[] Copy00<T1, T2, T3>(IEnumerable<KeyValuePair<T1, List<T2>>> data, T3 type)
            where T3 : KeyValueMemento<T1, List<T2>>, new()
        {
            return CopyAlg(Copy00(data), type);
        }
        public static T3[] Copy10<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2[]>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T3 : KeyValueMemento<T1, T2[]>, new()
        {
            return CopyAlg(Copy10(data), type);
        }
        public static T3[] Copy10<T1, T2, T3>(IEnumerable<KeyValuePair<T1, List<T2>>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T3 : KeyValueMemento<T1, List<T2>>, new()
        {
            return CopyAlg(Copy10(data), type);
        }

        public static T3[] Copy01<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2[]>> data, T3 type)
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, T2[]>, new()
        {
            return CopyAlg(Copy01(data), type);
        }
        public static T3[] Copy01<T1, T2, T3>(IEnumerable<KeyValuePair<T1, List<T2>>> data, T3 type)
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, List<T2>>, new()
        {
            return CopyAlg(Copy01(data), type);
        }
        public static T3[] Copy11<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2[]>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, T2[]>, new()
        {
            return CopyAlg(Copy11(data), type);
        }
        public static T3[] Copy11<T1, T2, T3>(IEnumerable<KeyValuePair<T1, List<T2>>> data, T3 type)
            where T1 : ICopyAble<T1>
            where T2 : ICopyAble<T2>
            where T3 : KeyValueMemento<T1, List<T2>>, new()
        {
            return CopyAlg(Copy11(data), type);
        }



        private static T3[] CopyAlg<T1, T2, T3>(IEnumerable<KeyValuePair<T1, T2>> data, T3 type)
            where T3 : KeyValueMemento<T1, T2>, new()
        {
            var result = new T3[data.Count()];
            int i = -1;
            foreach (var v in data)
            {
                result[++i] = new T3();
                result[i].SetKeyValue(v.Key, v.Value);
            }
            return result;
        }
    }
}

#endif