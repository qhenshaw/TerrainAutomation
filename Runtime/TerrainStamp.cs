using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

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
        [SerializeField] private int _gizmoResolution = 24;

        public Vector3 Min => _box.bounds.center - new Vector3(0f, _box.size.y * 0.5f, 0f) - transform.right * _box.size.x * 0.5f - transform.forward * _box.size.z * 0.5f;
        public Vector3 Max => _box.bounds.center + new Vector3(0f, _box.size.y * 0.5f, 0f) + transform.right * _box.size.x * 0.5f + transform.forward * _box.size.z * 0.5f;

        public Vector3 XCorner => Min + transform.right * _box.size.x;
        public Vector3 ZCorner => Min + transform.forward * _box.size.z;

        private List<Vector3> _verts = new List<Vector3>();
        private List<int> _tris = new List<int>();

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

            int gridCount = _gizmoResolution;
            float stepX = _box.size.x / gridCount;
            float stepZ = _box.size.z / gridCount;
            int totalCount = gridCount * gridCount;

            _verts.Clear();
            _tris.Clear();

            //Bottom left section of the map, other sections are similar
            for (int x = 0; x < gridCount; x++)
            {
                for (int z = 0; z < gridCount; z++)
                {
                    Vector3 position = Min + stepX * x * transform.right + stepZ * z * transform.forward;
                    position.y = Min.y;
                    float normX = InverseLerp(Min, XCorner, position) + 1f;
                    float normY = InverseLerp(Min, ZCorner, position) + 1f;
                    Vector2Int pos = new Vector2Int(Mathf.RoundToInt(normX * _stamp.width), Mathf.RoundToInt(normY * _stamp.height));
                    float texHeight = _stamp.GetPixel(pos.x, pos.y).r;

                    float height = texHeight * _box.size.y * _heightMultiplier + Min.y;
                    if (Mode == StampMode.Subtract) height = -texHeight * _box.size.y * _heightMultiplier + Max.y;
                    Vector3 point = new Vector3(position.x, height, position.z);

                    //Add each new vertex in the plane
                    _verts.Add(point);
                    //Skip if a new square on the plane hasn't been formed
                    if (x == 0 || z == 0) continue;
                    //Adds the index of the three vertices in order to make up each of the two tris
                    _tris.Add(gridCount * x + z); //Top right
                    _tris.Add(gridCount * x + z - 1); //Bottom right
                    _tris.Add(gridCount * (x - 1) + z - 1); //Bottom left - First triangle
                    _tris.Add(gridCount * (x - 1) + z - 1); //Bottom left 
                    _tris.Add(gridCount * (x - 1) + z); //Top left
                    _tris.Add(gridCount * x + z); //Top right - Second triangle
                }
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(_verts);
            mesh.SetTriangles(_tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            Gizmos.DrawWireMesh(mesh);
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