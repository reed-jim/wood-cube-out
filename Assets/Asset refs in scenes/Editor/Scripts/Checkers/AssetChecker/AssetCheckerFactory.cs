#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SearchEngine.Checkers
{
    public class AssetCheckerFactory : AssetProcessorFactory
    {
        public override AssetProcessor Create(IEnumerable<string> assets)
        {
            if (assets == null)
                return null;

            SetupAssetLists(assets);

            List<IAssetProcessor> typeCheckers = new List<IAssetProcessor>();
            if (PrefabModels.Any()) typeCheckers.Add(new ACPrefabModel(PrefabModels));
            if(Shaders.Any()) typeCheckers.Add(new ACShader(Shaders));
            if(Textures.Any()) typeCheckers.Add(new ACTexture(Textures));
            if(Scripts.Any()) typeCheckers.Add(new ACScript(Scripts));
            if (OtherTypes.Any()) typeCheckers.Add(new ACOtherType(OtherTypes));

            if (typeCheckers.Any())
                return new AssetProcessor(typeCheckers.ToArray());
            return null;
        }
    }
}

#endif
