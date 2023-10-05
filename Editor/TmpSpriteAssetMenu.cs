using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace TMPro
{
    public static class TmpSpriteAssetMenu
    {
        [MenuItem("CONTEXT/TmpSpriteAtlasAsset/Update Sprites", true, 2200)]
        private static bool UpdateValidate(MenuCommand command)
        {
            return command.context is TmpSpriteAtlasAsset;
        }

        [MenuItem("CONTEXT/TmpSpriteAtlasAsset/Update Sprites", false, 2200)]
        private static void Update(MenuCommand command)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("TmpSpriteAtlasAsset can be updates only in play mode");
                return;
            }

            if (EditorSettings.spritePackerMode != SpritePackerMode.SpriteAtlasV2)
            {
                Debug.LogError("TmpSpriteAtlasAsset can be updates only when spritePackerMode is SpriteAtlasV2");
                return;
            }

            if (command.context is TmpSpriteAtlasAsset rootAsset)
            {
                TmpSpriteAssetGenerator.Update(rootAsset);
            }
        }

        [MenuItem("Assets/Create/TextMeshPro/SpriteAtlas Asset", true, 2200)]
        private static bool PackValidate()
        {
            return Selection.activeObject is SpriteAtlas;
        }

        [MenuItem("Assets/Create/TextMeshPro/SpriteAtlas Asset", false, 2200)]
        private static void Pack()
        {
            var spriteAtlas = (SpriteAtlas) Selection.activeObject;
            var rootAssetPath = GetSpriteAtlasPathFromSpriteAtlas(spriteAtlas);

            var rootAsset = AssetDatabase.LoadAssetAtPath<TmpSpriteAtlasAsset>(rootAssetPath);
            if (rootAsset == null)
            {
                rootAsset = ScriptableObject.CreateInstance<TmpSpriteAtlasAsset>();
                AssetDatabase.CreateAsset(rootAsset, rootAssetPath);
            }

            rootAsset.spriteAtlas = spriteAtlas;

            TmpSpriteAssetGenerator.Update(rootAsset, updateSprites: false);
        }

        private static string GetSpriteAtlasPathFromSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            var filePathWithName = AssetDatabase.GetAssetPath(spriteAtlas);
            var fileNameWithExtension = Path.GetFileName(filePathWithName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
            var filePath = filePathWithName.Replace(fileNameWithExtension, "");
            return filePath + fileNameWithoutExtension + ".asset";
        }
    }
}