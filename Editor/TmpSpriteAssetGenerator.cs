using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.U2D;
using SpriteUtility = UnityEditor.Sprites.SpriteUtility;

namespace TMPro
{
    public static class TmpSpriteAssetGenerator
    {
        public static void Generate(AssetImportContext ctx,
            TMP_SpriteAsset mainSpriteAsset, SpriteAtlas atlas, TmpSpriteAssetData data)
        {
            var spriteAtlasPath = AssetDatabase.GetAssetPath(atlas);

            var atlasTextures = AssetDatabase.LoadAllAssetsAtPath(spriteAtlasPath)
                .OfType<Texture2D>()
                .ToList();

            foreach (var atlasTex in atlasTextures)
            {
                var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                ctx.AddObjectToAsset(atlasTex.name, spriteAsset);

                spriteAsset.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
                spriteAsset.name = atlasTex.name;
                spriteAsset.version = "1.1.0";
                spriteAsset.spriteSheet = atlasTex;
                spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(atlasTex.name);

                var spriteGlyphTable = new List<TMP_SpriteGlyph>();
                var spriteCharacterTable = new List<TMP_SpriteCharacter>();

                PopulateSpriteTables(ctx, atlas, data, atlasTex, spriteCharacterTable, spriteGlyphTable);

                spriteAsset.spriteCharacterTable = spriteCharacterTable;
                spriteAsset.spriteGlyphTable = spriteGlyphTable;

                spriteAsset.SortGlyphTable();
                spriteAsset.UpdateLookupTables();

                if (spriteAsset.material == null)
                {
                    AddDefaultMaterial(ctx, spriteAsset);
                }

                mainSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
            }
        }

        private static void PopulateSpriteTables(
            AssetImportContext ctx,
            SpriteAtlas atlas,
            TmpSpriteAssetData data,
            Texture2D texture,
            List<TMP_SpriteCharacter> spriteCharacterTable,
            List<TMP_SpriteGlyph> spriteGlyphTable)
        {
            var spritesCount = atlas.spriteCount;
            var sprites = new Sprite[spritesCount];
            atlas.GetSprites(sprites);

            for (var i = 0; i < sprites.Length; i++)
            {
                var spriteAccess = sprites[i];

                Texture2D spriteTex;
                Vector2[] spriteUv;

                try
                {
                    spriteTex = SpriteUtility.GetSpriteTexture(spriteAccess, getAtlasData: true);
                    spriteUv = SpriteUtility.GetSpriteUVs(spriteAccess, getAtlasData: true);
                }
                catch
                {
                    // for non-packed sprites SpriteUtility throws exception
                    // this only happens when the atlas is not baked e.g.
                    // when new assets were added
                    // after atlas re-baking this function works as it should again

                    spriteTex = null;
                    spriteUv = null;

                    ctx.LogImportError($"Failed to process '{spriteAccess.name}' sprite because it is not packed");
                }

                if (spriteTex == null || spriteUv == null || spriteTex != texture)
                {
                    continue;
                }

                var spriteName = spriteAccess.name;

                if (spriteName.EndsWith("(Clone)"))
                {
                    spriteName = spriteName.Substring(0, spriteName.Length - "(Clone)".Length);
                }

                var boundMin = spriteUv[0];
                var boundMax = spriteUv[0];

                foreach (var uv in spriteUv)
                {
                    boundMin = Vector2.Min(boundMin, uv);
                    boundMax = Vector2.Max(boundMax, uv);
                }

                var textureRect = new Rect(
                    x: boundMin.x * spriteTex.width,
                    y: boundMin.y * spriteTex.height,
                    width: (boundMax.x - boundMin.x) * spriteTex.width,
                    height: (boundMax.y - boundMin.y) * spriteTex.height
                );

                var spriteGlyph = new TMP_SpriteGlyph
                {
                    index = (uint) i,
                    metrics = new GlyphMetrics(
                        width: textureRect.width,
                        height: textureRect.height,
                        bearingX: data.bearingX,
                        bearingY: textureRect.height - data.bearingY,
                        advance: textureRect.width + data.advance),
                    glyphRect = new GlyphRect(textureRect),
                    scale = 1.0f,
                    sprite = spriteAccess,
                };

                spriteGlyphTable.Add(spriteGlyph);

                var spriteCharacter = new TMP_SpriteCharacter(0xFFFE, spriteGlyph)
                {
                    name = spriteName,
                    scale = data.scale,
                };

                spriteCharacterTable.Add(spriteCharacter);
            }
        }

        private static void AddDefaultMaterial(AssetImportContext ctx, TMP_SpriteAsset spriteAsset)
        {
            var name = $"{spriteAsset.spriteSheet.name} Material";
            var shader = Shader.Find("TextMeshPro/Sprite");
            var material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.name = name;
            material.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            ctx.AddObjectToAsset(name, material);
        }
    }
}