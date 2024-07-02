using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.U2D;

namespace TMPro
{
    [CustomEditor(typeof(TmpSpriteAssetImporter))]
    public class TmpSpriteAssetImporterEditor : ScriptedImporterEditor
    {
        private static readonly GUIContent AtlasContent = new GUIContent("Source Atlas",
            "The source asset from which the TMP atlas is generated");

        private static readonly GUIContent ReferenceSizeContent = new GUIContent("Reference Size",
            "Reference size is used for Bearing and Advance properties scaling when atlas contain textures with different sizes");

        private static readonly GUIContent BearingContent = new GUIContent("Bearing",
            "Sets the sprite's offset relative to the center point");

        private static readonly GUIContent AdvanceContent = new GUIContent("Advance",
            "Sets the additional space that the sprite occupies in the text");

        private static readonly GUIContent ScaleContent = new GUIContent("Scale",
            "Sets the size of the sprite in the text");

        private TmpSpriteAssetData _data;
        private bool _dataModified;

        private TmpSpriteAssetImporter Importer => (TmpSpriteAssetImporter) target;

        public override void OnEnable()
        {
            base.OnEnable();
            LoadData();
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AssetDatabase.GUIDToAssetPath(_data.atlasGuid));
                EditorGUILayout.ObjectField(AtlasContent, atlas, typeof(SpriteAtlas), false);
            }

            GUILayout.Space(5);
            GUILayout.Label("Sprite Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            GuiLayoutXYField(ReferenceSizeContent, ref _data.referenceWidth, ref _data.referenceHeight);
            GuiLayoutXYField(BearingContent, ref _data.bearingX, ref _data.bearingY);
            _data.advance = EditorGUILayout.FloatField(AdvanceContent, _data.advance);
            _data.scale = EditorGUILayout.FloatField(ScaleContent, _data.scale);

            if (EditorGUI.EndChangeCheck())
            {
                _dataModified = true;
            }

            ApplyRevertGUI();
        }

        private void LoadData()
        {
            _dataModified = false;
            _data = JsonUtility.FromJson<TmpSpriteAssetData>(File.ReadAllText(Importer.assetPath));
        }

        public override bool HasModified()
        {
            return base.HasModified() || _dataModified;
        }

        protected override void Apply()
        {
            File.WriteAllText(Importer.assetPath, JsonUtility.ToJson(_data));
            LoadData();
            base.Apply();
            AssetDatabase.ImportAsset(Importer.assetPath, ImportAssetOptions.ForceUpdate);
        }

        public override void DiscardChanges()
        {
            LoadData();
            base.DiscardChanges();
        }

        private static void GuiLayoutXYField(GUIContent label, ref float x, ref float y)
        {
            var vec = EditorGUILayout.Vector2Field(label, new Vector2(x, y));
            x = vec.x;
            y = vec.y;
        }
    }
}