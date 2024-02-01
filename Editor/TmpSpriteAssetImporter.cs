using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace TMPro
{
    [ScriptedImporter(1, FileExtension)]
    public class TmpSpriteAssetImporter : ScriptedImporter
    {
        public const string FileExtension = "tmpspriteatlas";
        public const string FileExtensionWithDot = ".tmpspriteatlas";
        public const string FileVersion = "1.1.0";

        private static string[] GatherDependenciesFromSourceFile(string path)
        {
            var data = JsonUtility.FromJson<TmpSpriteAssetData>(File.ReadAllText(path));
            var spriteAtlasPath = AssetDatabase.GUIDToAssetPath(data.atlasGuid);
            var shaderPath = AssetDatabase.GUIDToAssetPath(data.shaderGuid);
            return new[] {spriteAtlasPath, shaderPath};
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var mainSpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();

            ctx.AddObjectToAsset("main", mainSpriteAsset);
            ctx.SetMainObject(mainSpriteAsset);

            mainSpriteAsset.name = "main";
            mainSpriteAsset.hideFlags = HideFlags.NotEditable;
            mainSpriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

            if (EditorSettings.spritePackerMode != SpritePackerMode.SpriteAtlasV2)
            {
                ctx.LogImportError("TmpSpriteAtlasAsset can be updates only when spritePackerMode is SpriteAtlasV2");
                return;
            }

            var data = JsonUtility.FromJson<TmpSpriteAssetData>(File.ReadAllText(ctx.assetPath));

            if (!GUID.TryParse(data.atlasGuid, out var spriteAtlasGuid))
            {
                ctx.LogImportError("Failed to import TmpSpriteAtlasAsset: atlas guid is invalid");
                return;
            }

            if (!GUID.TryParse(data.shaderGuid, out var shaderGuid))
            {
                ctx.LogImportError("Failed to import TmpSpriteAtlasAsset: shader guid is invalid");
                return;
            }

            var spriteAtlas = AssetDatabase.LoadMainAssetAtGUID(spriteAtlasGuid) as SpriteAtlas;
            if (spriteAtlas == null)
            {
                ctx.LogImportError("Failed to import TmpSpriteAtlasAsset: failed to load atlas");
                return;
            }

            var shader = AssetDatabase.LoadMainAssetAtGUID(shaderGuid) as Shader;
            if (shader == null)
            {
                ctx.LogImportError("Failed to import TmpSpriteAtlasAsset: failed to load shader");
                return;
            }

            mainSpriteAsset.version = FileVersion;
            mainSpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAtlas.name);

            SpriteAtlasUtility.PackAtlases(new[] {spriteAtlas}, ctx.selectedBuildTarget, false);

            TmpSpriteAssetGenerator.Generate(ctx, mainSpriteAsset, spriteAtlas, shader, data);

            mainSpriteAsset.SortGlyphTable();
            mainSpriteAsset.UpdateLookupTables();
        }
    }
}