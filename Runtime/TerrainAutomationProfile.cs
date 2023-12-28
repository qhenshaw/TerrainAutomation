using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainAutomation
{
    [CreateAssetMenu]
    public class TerrainAutomationProfile : ScriptableObject
    {
        private enum EditingMode
        {
            [LabelText(SdfIconType.Brush)] TerrainLayers,
            [LabelText(SdfIconType.Tree)] Trees,
            [LabelText(SdfIconType.Binoculars)] Details
        }

#pragma warning disable 0414
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [SerializeField, HideLabel, EnumToggleButtons] private EditingMode _editingMode = EditingMode.TerrainLayers;
#pragma warning restore 0414

        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.TerrainLayers), InlineEditor, AssetsOnly, ListDrawerSettings(ShowFoldout = false)] public List<TerrainLayer> TerrainLayers { get; private set; }

        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.Trees)] public int Seed { get; private set; } = 0;
        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.Trees)] public int Count { get; private set; } = 10000;
        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.Trees)] public bool KeepExistingTrees { get; private set; } = false;
        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.Trees), ListDrawerSettings(ShowFoldout = false)] public List<TreeLayerConfig> Trees { get; private set; }

        [field: SerializeField, ShowIf("_editingMode", Value = EditingMode.Details), ListDrawerSettings(ShowFoldout = false)] public List<DetailLayerConfig> DetailLayers { get; private set; }

        [ShowIf("_editingMode", Value = EditingMode.TerrainLayers), Button("Add New", Icon = SdfIconType.PlusSquare)]
        private void AddNewTerrainLayer()
        {
            TerrainLayers.Add(null);
        }

        [ShowIf("_editingMode", Value = EditingMode.Trees), Button("Add New", Icon = SdfIconType.PlusSquare)]
        private void AddNewTree()
        {
            Trees.Add(new TreeLayerConfig());
        }

        [ShowIf("_editingMode", Value = EditingMode.Details), Button("Add New", Icon = SdfIconType.PlusSquare)]
        private void AddNewDetail()
        {
            DetailLayers.Add(new DetailLayerConfig());
        }

        private bool ValidateTerrainLayers(List<TerrainLayer> terrainLayers, ref string errorMessage, ref InfoMessageType? messageType)
        {
            int count = terrainLayers.Count;
            if (count >= 5)
            {
                errorMessage = "Maximum 4 terrain layers allowed.";
                messageType = InfoMessageType.Error;
                return false;
            }

            return true;
        }

    }

    [System.Serializable]
    public class DetailLayerConfig
    {
        [BoxGroup, Required] public GameObject Prefab;
        [FoldoutGroup("Settings")] public int Density = 100;
        [FoldoutGroup("Settings")][Range(0f, 3f)] public float DensityScale = 1f;
        [FoldoutGroup("Settings"), Range(0f, 1f)] public float AlignToGround = 0f;
        [FoldoutGroup("Settings")] public int NoiseSeed = 0;
        [FoldoutGroup("Settings")] public Vector2 ScaleRange = new Vector2(1f, 2f);
        [FoldoutGroup("Settings")] public Vector4 SplatFilter = new Vector4(1f, 1f, 1f, 1f);
        [FoldoutGroup("Settings")] public float SplatCutoff = 0.25f;
        [FoldoutGroup("Settings")] public Vector2 MinMaxPlacementHeight = new Vector2(-1000f, 1000f);
        [FoldoutGroup("Settings")] public float MaxSlope = 90f;

        public DetailLayerConfig()
        {
            Density = 100;
            DensityScale = 1f;
            AlignToGround = 0f;
            System.Random random = new System.Random();
            NoiseSeed = random.Next(int.MaxValue);
            ScaleRange = new Vector2(1f, 2f);
            SplatFilter = new Vector4(1f, 1f, 1f, 1f);
            SplatCutoff = 0.25f;
            MinMaxPlacementHeight = new Vector2(-1000f, 1000f);
        }
    }

    [System.Serializable]
    public class TreeLayerConfig
    {
        [BoxGroup][Required][Tooltip("Tree prefab, must have LODGroup to work with terrain system.")] public LODGroup Prefab;
        [FoldoutGroup("Settings"), Tooltip("The relative distribution of each tree. Increase to see more of this tree.")] public float DistributionWeight = 1f;
        [FoldoutGroup("Settings"), Tooltip("Random base scale value chosen from curve.")] public AnimationCurve ScaleRange;
        [FoldoutGroup("Settings"), Tooltip("Overall size multiplier.")] public float ScaleMultiplier = 1f;
        [FoldoutGroup("Settings"), Tooltip("Choose which terrain layers (from splatmap) this tree will appear on.")] public Vector4 SplatFilter;
        [FoldoutGroup("Settings"), Tooltip("Minimum filter value required to place tree on layer.")] public float SplatCutoff = 0.2f;
        [FoldoutGroup("Settings"), Tooltip("Min/max world height acceptable for trees.")] public Vector2 MinMaxPlacementHeight;
        [FoldoutGroup("Settings"), Tooltip("Max slop this tree can be placed on.")] public float MaxSlope = 90f;

        public TreeLayerConfig()
        {
            ScaleRange = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.75f, -1f, 1f), new Keyframe(1f, 1.25f, 1f, -1f) });
            ScaleMultiplier = 1f;
            SplatFilter = new Vector4(1f, 1f, 1f, 1f);
            SplatCutoff = 0.2f;
            MinMaxPlacementHeight = new Vector2(-1000f, 1000f);
            MaxSlope = 90f;
        }
    }
}