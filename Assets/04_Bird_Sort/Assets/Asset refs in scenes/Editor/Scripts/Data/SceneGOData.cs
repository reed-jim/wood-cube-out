#if UNITY_EDITOR

using System;
using System.Linq;
using SearchEngine.Memento;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Data
{
    [Serializable]
    public class SceneGOData : ICopyAble<SceneGOData>, IValidatable
    {
        [SerializeField] protected string objPath;
        [SerializeField] protected DataGOPath[] gameObjs;

        public string ObjPath
        {
            get { return objPath; }
        }
        
        public DataGOPath[] GameObjOnScene
        {
            get { return gameObjs; }
        }

        public SceneGOData(string objPath, DataGOPath[] gameObjs)
        {
            this.objPath = objPath;
            this.gameObjs = gameObjs;
        }

        public SceneGOData Copy()
        {
            return new SceneGOData(objPath, CopyAbleHelper.ToArray(gameObjs));
        }

        object ICopyAble.Copy()
        {
            return Copy();
        }

        public bool Validate()
        {
            return
                gameObjs != null
                && gameObjs.All(v => v != null && v.Validate());
        }
    }
}

#endif