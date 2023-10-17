using UnityEditor;

namespace TMPro
{
    public class TmpSpriteAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (var assetPath in importedAssets)
            {
                if (!assetPath.EndsWith(TmpSpriteAssetImporter.FileExtensionWithDot))
                {
                    continue;
                }

                NotifyChanged(assetPath);
            }
        }

        private static void NotifyChanged(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(assetPath);

            if (asset.version != TmpSpriteAssetImporter.FileVersion)
            {
                return;
            }

            foreach (var subAsset in asset.fallbackSpriteAssets)
            {
                subAsset.UpdateLookupTables();
                TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, subAsset);
            }

            asset.UpdateLookupTables();
            TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, asset);
        }
    }
}