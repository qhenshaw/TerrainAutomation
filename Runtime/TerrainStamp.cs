using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainAutomation
{
    [RequireComponent(typeof(BoxCollider))]
    public class TerrainStamp : MonoBehaviour, ITerrainModifier
    {
        public enum StampMode
        {
            Add,
            Subtract,
            Max,
            Min
        }

        [SerializeField] private Texture2D _stamp;
        [SerializeField] private float _heightMultiplier = 1f;
        [field: SerializeField] public StampMode Mode = StampMode.Max;
        [SerializeField] private BoxCollider _box;

        public Vector3 Min => _box.bounds.center - new Vector3(0f, _box.size.y * 0.5f, 0f) - transform.right * _box.size.x * 0.5f - transform.forward * _box.size.z * 0.5f;
        public Vector3 Max => _box.bounds.center + new Vector3(0f, _box.size.y * 0.5f, 0f) + transform.right * _box.size.x * 0.5f + transform.forward * _box.size.z * 0.5f;

        public Vector3 XCorner => Min + transform.right * _box.size.x;
        public Vector3 ZCorner => Min + transform.forward * _box.size.z;

        private void OnValidate()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        private void Start()
        {
            _box.enabled = false;
        }

        public void Initialize()
        {

        }

        public bool GetHeight(Vector3 position, out float height)
        {
            float originalHeight = position.y;
            if (!ContainsPoint(position, _box))
            {
                height = 0f;
                return false;
            }

            position.y = Min.y;
            float normX = InverseLerp(Min, XCorner, position) + 1f;
            float normY = InverseLerp(Min, ZCorner, position) + 1f;
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(normX * _stamp.width), Mathf.RoundToInt(normY * _stamp.height));
            float texHeight = _stamp.GetPixel(pos.x, pos.y).r;
            float calcHeight = texHeight * _box.size.y * _heightMultiplier + Min.y;

            switch (Mode)
            {
                case StampMode.Max:
                    height = Mathf.Max(originalHeight, calcHeight);
                    break;
                case StampMode.Min:
                    height = Mathf.Min(originalHeight, calcHeight);
                    break;
                case StampMode.Add:
                    height = originalHeight + texHeight * _box.size.y * _heightMultiplier;
                    break;
                case StampMode.Subtract:
                    height = originalHeight - texHeight * _box.size.y * _heightMultiplier;
                    break;
                default:
                    height = originalHeight;
                    break;
            }

            return true;
        }

        public bool GetSplat(Vector3 position, Color splatIn, out Color splatOut)
        {
            splatOut = Color.black;
            return false;
        }

        private bool ContainsPoint(Vector3 point, BoxCollider box)
        {
            point = box.transform.InverseTransformPoint(point) - box.center;

            float halfX = (box.size.x * 0.5f);
            float halfY = (box.size.y * 0.5f);
            float halfZ = (box.size.z * 0.5f);

            if (point.x < halfX && point.x > -halfX &&
               point.y < halfY && point.y > -halfY &&
               point.z < halfZ && point.z > -halfZ)
                return true;
            else
                return false;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        [Button]
        public void UpdateTerrainHeightMap()
        {
            TerrainAutomator automation = GetComponentInParent<TerrainAutomator>();
            automation?.UpdateHeightMap();
        }

        [Button]
        public void UpdateTerrainAll()
        {
            TerrainAutomator automation = GetComponentInParent<TerrainAutomator>();
            automation?.UpdateAll();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_stamp || !_box) return;
            ITerrainModifier tm = this;
            if (!tm.IsVisible(_box.bounds)) return;

            Vector3 cameraPosition = Camera.main.transform.position;
#if UNITY_EDITOR
            cameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
#endif
            int maxCount = 32;
            int maxX = Mathf.Min(Mathf.RoundToInt(_box.size.x), maxCount);
            int maxZ = Mathf.Min(Mathf.RoundToInt(_box.size.z), maxCount);

            float stepX = Mathf.Max(_box.size.x / maxX, 1f);
            float stepZ = Mathf.Max(_box.size.z / maxZ, 1f);
            int xCount = Mathf.FloorToInt(_box.size.x / stepX);
            int zCount = Mathf.FloorToInt(_box.size.z / stepZ);

            List<Vector3> points = new List<Vector3>();

            for (int x = 0; x < xCount; x++)
            {
                points.Clear();

                for (int z = 0; z < zCount; z++)
                {
                    Vector3 position = Min + stepX * x * transform.right + stepZ * z * transform.forward;
                    position.y = Min.y;
                    if (!tm.IsVisible(position)) continue;
                    float normX = InverseLerp(Min, XCorner, position) + 1f;
                    float normY = InverseLerp(Min, ZCorner, position) + 1f;
                    Vector2Int pos = new Vector2Int(Mathf.RoundToInt(normX * _stamp.width), Mathf.RoundToInt(normY * _stamp.height));
                    float texHeight = _stamp.GetPixel(pos.x, pos.y).r;

                    float height = texHeight * _box.size.y * _heightMultiplier + Min.y;
                    if (Mode == StampMode.Subtract) height = -texHeight * _box.size.y * _heightMultiplier + Max.y;
                    Vector3 point = new Vector3(position.x, height, position.z);
                    points.Add(point);
                }

                Gizmos.DrawLineStrip(points.ToArray(), false);
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Terrain/Terrain Stamp", false)]
        private static void Create(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Terrain Stamp");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            go.AddComponent<TerrainStamp>();
            Selection.activeGameObject = go;
        }
#endif
    }
}