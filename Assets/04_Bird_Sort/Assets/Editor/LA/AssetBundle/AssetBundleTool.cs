using System.IO;
using UnityEditor;

namespace LA.AssetBundle
{
    public static class AssetBundleTool
    {
        static string outputLocate = "Assets/StreamingAssets/Export";
        [MenuItem("LA/Asset Bundle/Build All Asset Bundle")]
        public static void BuildAllAssetBundle()
        {
            
            
        
            BuildPipeline.BuildAssetBundles(outputLocate, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    
        public static void BuildAssetBundlesByName(string assetBundleName) 
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
 
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetBundleName;
            build.assetNames = assetPaths;
            if (!Directory.Exists(outputLocate))
            {
                Directory.CreateDirectory(outputLocate);
            }
            BuildPipeline.BuildAssetBundles(outputLocate, new []{build}, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            
        }

        

        
    }
}
