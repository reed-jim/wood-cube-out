#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Additions
{
    public class GameObjectsCollector
    {
        private List<GameObject> foundGO = new List<GameObject>();

        public GameObject[] FindAllObjectsOnScene()
        {
            var root =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Select(x => x.transform);

            foreach (var tr in root)
                RecursiveSearch(tr);

            var newGoOs = foundGO.ToArray();
            foundGO.Clear();
            return newGoOs;
        }

        private void RecursiveSearch(Transform tr)
        {
            this.foundGO.Add(tr.gameObject);
            foreach (Transform t in tr.transform)
                RecursiveSearch(t);
        }
    }
}

#endif