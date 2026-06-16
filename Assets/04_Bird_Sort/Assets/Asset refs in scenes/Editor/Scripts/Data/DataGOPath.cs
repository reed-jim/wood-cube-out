#if UNITY_EDITOR

using System;
using SearchEngine.Memento;
using UnityEngine;

namespace SearchEngine.EditorViews.Data
{
    [Serializable]
    public class DataGOPath : ICopyAble<DataGOPath>, IValidatable
    {
        [SerializeField] private int[] indexPath;
        [SerializeField] private string name;

        public int[] IndexPaths
        {
            get { return indexPath; }
        }

        public string Name
        {
            get { return name; }
        }

        public DataGOPath(string name, int[] indexPath)
        {
            this.indexPath = indexPath;
            this.name = name;
        }

        public DataGOPath Copy()
        {
            return new DataGOPath(name, (int[])indexPath.Clone());
        }

        object ICopyAble.Copy()
        {
            return Copy();
        }

        public bool Validate()
        {
            return
                indexPath != null
                && indexPath.Length > 0;
        }
    }
}

#endif