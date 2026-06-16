#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.EditorViews.Data;
using Object=UnityEngine.Object;

namespace SearchEngine.Memento.KeyValueMementoClasses
{
    [Serializable]
    public class ObjectBool: KeyValueMemento<object, bool>
    {
        public ObjectBool(object key, bool value) : base(key, value){}
    }
    [Serializable]
    public class ObjecObjectM: KeyValueMemento<object, object[]>
    {
        public ObjecObjectM(object key, object[] value) : base(key, value){}
    }
    [Serializable]
    public class ObjectSceneGODataL : KeyValueMemento<Object, List<SceneGOData>>, IValidatable
    {
        public ObjectSceneGODataL(){}

        public bool Validate()
        {
            return
                Value != null
                && Value.All(v => v != null && v.Validate());
        }
    }
    [Serializable]
    public class StringSceneGODataL : KeyValueMemento<string, List<SceneGOData>>, IValidatable
    {
        public StringSceneGODataL(){}

        public bool Validate()
        {
            return
                !string.IsNullOrEmpty(Key)
                && Value != null
                && Value.All(v => v != null && v.Validate());
        }
    }
    [Serializable]
    public class SceneGODataBool : KeyValueMemento<SceneGOData, bool>
    {
        public SceneGODataBool(){}
    }
    [Serializable]
    public class ObjectDataGOPathM : KeyValueMemento<Object, DataGOPath[]>, IValidatable
    {
        public ObjectDataGOPathM(){}

        public bool Validate()
        {
            return
                Value != null
                && Value.All(v => v != null && v.Validate());
        }
    }    
    [Serializable]
    public class StringDataGOPathM : KeyValueMemento<string, DataGOPath[]>, IValidatable
    {
        public StringDataGOPathM(){}

        public bool Validate()
        {
            return
                !string.IsNullOrEmpty(Key)
                && Value != null
                && Value.All(v => v != null && v.Validate());
        }
    }    
}

#endif