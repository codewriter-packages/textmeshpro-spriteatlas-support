using UnityEngine;
using UnityEngine.U2D;

namespace TMPro
{
    public class TmpSpriteAtlasAsset : ScriptableObject
    {
        public SpriteAtlas spriteAtlas;
        public TMP_SpriteAsset mainSpriteAsset;

        public Vector2 bearing;
        public float advance;
        public float scale = 1.0f;
    }
}