using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace TMPro
{
    public static class TmpSpriteAssetGenerator
    {
        public static void Update(TmpSpriteAtlasAsset rootAsset, bool updateSprites = true)
        {
            var rootAssetPath = AssetDatabase.GetAssetPath(rootAsset);

            AddMainSpriteAsset(rootAsset);

            if (updateSprites)
            {
                UpdateSpriteAssets(rootAsset);
            }

            EditorUtility.SetDirty(rootAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(rootAssetPath);

            foreach (var spriteAsset in AssetDatabase.LoadAllAssetsAtPath(rootAssetPath).OfType<TMP_SpriteAsset>())
            {
                TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, spriteAsset);
            }
        }

        private static void UpdateSpriteAssets(TmpSpriteAtlasAsset rootAsset)
        {
            var rootAssetPath = AssetDatabase.GetAssetPath(rootAsset);
            var spriteAtlasPath = AssetDatabase.GetAssetPath(rootAsset.spriteAtlas);

            var subSpriteAssets = AssetDatabase.LoadAllAssetsAtPath(rootAssetPath)
                .OfType<TMP_SpriteAsset>()
                .Where(it => it != rootAsset.mainSpriteAsset)
                .ToList();

            var atlasTextures = AssetDatabase.LoadAllAssetsAtPath(spriteAtlasPath)
                .OfType<Texture2D>()
                .ToList();

            foreach (var atlasTex in atlasTextures)
            {
                var spriteAsset = subSpriteAssets.FirstOrDefault(it => it.name == atlasTex.name);
                if (spriteAsset == null)
                {
                    spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                    AssetDatabase.AddObjectToAsset(spriteAsset, rootAsset);
                }

                spriteAsset.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
                spriteAsset.name = atlasTex.name;
                spriteAsset.version = "1.1.0";
                spriteAsset.spriteSheet = atlasTex;
                spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(rootAsset.spriteAtlas.name);

                var spriteGlyphTable = new List<TMP_SpriteGlyph>();
                var spriteCharacterTable = new List<TMP_SpriteCharacter>();

                PopulateSpriteTables(rootAsset, atlasTex, spriteCharacterTable, spriteGlyphTable);

                spriteAsset.spriteCharacterTable = spriteCharacterTable;
                spriteAsset.spriteGlyphTable = spriteGlyphTable;

                spriteAsset.SortGlyphTable();
                spriteAsset.UpdateLookupTables();

                if (spriteAsset.material == null)
                {
                    AddDefaultMaterial(spriteAsset);
                }

                rootAsset.mainSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
            }

            foreach (var spriteAsset in subSpriteAssets)
            {
                if (atlasTextures.FirstOrDefault(it => it.name == spriteAsset.name) != null)
                {
                    continue;
                }

                if (spriteAsset.material != null)
                {
                    Object.DestroyImmediate(spriteAsset.material, allowDestroyingAssets: true);
                }

                Object.DestroyImmediate(spriteAsset, allowDestroyingAssets: true);
            }
        }

        private static void AddMainSpriteAsset(TmpSpriteAtlasAsset rootAsset)
        {
            var mainSpriteAsset = rootAsset.mainSpriteAsset;

            if (mainSpriteAsset == null)
            {
                mainSpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                AssetDatabase.AddObjectToAsset(mainSpriteAsset, rootAsset);

                rootAsset.mainSpriteAsset = mainSpriteAsset;
            }

            mainSpriteAsset.name = rootAsset.spriteAtlas.name;
            mainSpriteAsset.hideFlags = HideFlags.NotEditable;
            mainSpriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
        }

        private static void PopulateSpriteTables(
            TmpSpriteAtlasAsset rootAsset,
            Texture2D texture,
            List<TMP_SpriteCharacter> spriteCharacterTable,
            List<TMP_SpriteGlyph> spriteGlyphTable)
        {
            var spriteCount = rootAsset.spriteAtlas.spriteCount;
            var sprites = new Sprite[spriteCount];

            rootAsset.spriteAtlas.GetSprites(sprites);

            for (var i = 0; i < sprites.Length; i++)
            {
                var sprite = sprites[i];

                if (sprite.texture != texture)
                {
                    continue;
                }

                var spriteName = sprite.name;

                if (spriteName.EndsWith("(Clone)"))
                {
                    spriteName = spriteName.Substring(0, spriteName.Length - "(Clone)".Length);
                }

                var bearing = rootAsset.bearing;
                var advance = rootAsset.advance;

                var spriteGlyph = new TMP_SpriteGlyph
                {
                    index = (uint) i,
                    metrics = new GlyphMetrics(
                        width: sprite.textureRect.width,
                        height: sprite.textureRect.height,
                        bearingX: bearing.x,
                        bearingY: sprite.textureRect.height - bearing.y,
                        advance: sprite.textureRect.width + advance),
                    glyphRect = new GlyphRect(sprite.textureRect),
                    scale = 1.0f,
                    sprite = sprite,
                };

                spriteGlyphTable.Add(spriteGlyph);

                var spriteCharacter = new TMP_SpriteCharacter(0xFFFE, spriteGlyph)
                {
                    name = spriteName,
                    scale = rootAsset.scale,
                };

                spriteCharacterTable.Add(spriteCharacter);
            }
        }

        private static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
        {
            var shader = Shader.Find("TextMeshPro/Sprite");
            var material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            AssetDatabase.AddObjectToAsset(material, spriteAsset);
        }
    }
}