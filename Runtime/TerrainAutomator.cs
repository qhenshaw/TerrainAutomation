using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainAutomation
{
    [RequireComponent(typeof(Terrain)), HideMonoScript]
    public class TerrainAutomator : MonoBehaviour
    {
        [SerializeField, TitleGroup("Setup"), Required] private Terrain _terrain;
        [SerializeField, TitleGroup("Setup")] private Vector3 _terrainSize = new Vector3(1000f, 100f, 1000f);
        [SerializeField, TitleGroup("Setup"), Required] private Texture2D _heightMap;
        [SerializeField, TitleGroup("Setup"), Required] private Texture2D _splatMap;
        [SerializeField, TitleGroup("Setup")] private Material _terrainMaterial;
        [SerializeField, TitleGroup("Setup")] private bool _drawInstanced = true;
        [SerializeField, TitleGroup("Profile"), InlineEditor(InlineEditorObjectFieldModes.Boxed, Expanded = true), InlineButton("New", "New Profile", Icon = SdfIconType.PlusSquare, ShowIf = "@!_profile"), Required, HideLabel] private TerrainAutomationProfile _profile;

        private void OnValidate()
        {
            _terrain = GetComponent<Terrain>();
            _terrain.drawInstanced = _drawInstanced;
            if (_terrainMaterial) _terrain.materialTemplate = _terrainMaterial;

            if (_heightMap) ConfigMap(_heightMap);
            if (_splatMap) ConfigMap(_splatMap);
        }

        private void ConfigMap(Texture2D texture)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.sRGBTexture = false;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
#endif
        }

        private void New()
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save new profile", "New TerrainAutomationProfile", "asset", "");
            if (path.Length != 0)
            {
                CreateAt<TerrainAutomationProfile>(path);
                TerrainAutomationProfile profile = AssetDatabase.LoadAssetAtPath<TerrainAutomationProfile>(path);
                _profile = profile;
            }
#endif
        }

        private void Start()
        {
            TerrainUpdater.UpdateGlobalSplatTexture(_terrain.terrainData.GetAlphamapTexture(0), _terrainSize);
        }

        [TitleGroup("Update")]
        [ButtonGroup("Update/ButtonsTop"), Button("All", ButtonSizes.Large, Icon = SdfIconType.Recycle, Stretch = false)]
        public void UpdateAll()
        {
            AssignTerrainLayers();
            UpdateHeightMap();
            UpdateSplatmap();
            PlaceTrees();
            PlaceDetails();
        }

        [ButtonGroup("Update/ButtonsTop"), Button("Height", ButtonSizes.Large, Icon = SdfIconType.Map, Stretch = false)]
        public void UpdateHeightMap()
        {
            TerrainUpdater.UpdateHeightMap(_terrain, _heightMap, _terrainSize);
        }

        [ButtonGroup("Update/ButtonsTop"), Button("Splat", ButtonSizes.Large, Icon = SdfIconType.Image, Stretch = false)]
        public void UpdateSplatmap()
        {
            TerrainUpdater.UpdateSplatMap(_terrain, _heightMap, _splatMap, _terrainSize);
        }

        [ButtonGroup("Update/ButtonsBottom"), Button("Layers", ButtonSizes.Large, Icon = SdfIconType.Brush, Stretch = false)]
        public void AssignTerrainLayers()
        {
            TerrainUpdater.AssignTerrainLayerSet(_terrain, _profile.TerrainLayers);
        }

        [ButtonGroup("Update/ButtonsBottom"), Button("Trees", ButtonSizes.Large, Icon = SdfIconType.Tree, Stretch = false)]
        public void PlaceTrees()
        {
            TerrainTreePlacement.PlaceTrees(_terrain, _profile.Trees, _profile.Seed, _profile.Count, _profile.KeepExistingTrees);
        }

        [ButtonGroup("Update/ButtonsBottom"), Button("Details", ButtonSizes.Large, Icon = SdfIconType.Binoculars, Stretch = false)]
        public void PlaceDetails()
        {
            TerrainDetailPlacement.PlaceDetails(_terrain, _profile.DetailLayers);
        }

#if UNITY_EDITOR
        public static T CreateAt<T>(string assetPath) where T : ScriptableObject
        {
            return CreateAt(typeof(T), assetPath) as T;
        }

        public static ScriptableObject CreateAt(Type assetType, string assetPath)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(assetType);
            if (asset == null)
            {
                Debug.LogError("failed to create instance of " + assetType.Name + " at " + assetPath);
                return null;
            }
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }
#endif
    }
}