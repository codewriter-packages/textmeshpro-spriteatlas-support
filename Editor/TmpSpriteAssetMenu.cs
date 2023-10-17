using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace TMPro
{
    public static class TmpSpriteAssetMenu
    {
        [MenuItem("Assets/Create/TextMeshPro/SpriteAtlas Asset", true, 2200)]
        private static bool PackValidate()
        {
            return Selection.activeObject is SpriteAtlas;
        }

        [MenuItem("Assets/Create/TextMeshPro/SpriteAtlas Asset", false, 2200)]
        private static void Pack()
        {
            var spriteAtlas = (SpriteAtlas) Selection.activeObject;
            var spriteAtlasPath = AssetDatabase.GetAssetPath(spriteAtlas);
            var spriteAtlasGuid = AssetDatabase.AssetPathToGUID(spriteAtlasPath);

            var rootAssetPath = GetSpriteAtlasPathFromSpriteAtlas(spriteAtlas);

            var data = File.Exists(rootAssetPath)
                ? JsonUtility.FromJson<TmpSpriteAssetData>(File.ReadAllText(rootAssetPath))
                : new TmpSpriteAssetData();

            data.atlasGuid = spriteAtlasGuid;

            File.WriteAllText(rootAssetPath, JsonUtility.ToJson(data, prettyPrint: true));

            AssetDatabase.ImportAsset(rootAssetPath, ImportAssetOptions.ForceUpdate);
        }

        private static string GetSpriteAtlasPathFromSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            var filePathWithName = AssetDatabase.GetAssetPath(spriteAtlas);
            var fileNameWithExtension = Path.GetFileName(filePathWithName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
            var filePath = filePathWithName.Replace(fileNameWithExtension, "");
            return filePath + fileNameWithoutExtension + TmpSpriteAssetImporter.FileExtensionWithDot;
        }
    }
}