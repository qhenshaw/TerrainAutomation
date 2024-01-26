using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TerrainAutomation
{
    [RequireComponent(typeof(SplineContainer))]
    public class TerrainPath : MonoBehaviour, ITerrainModifier
    {
        [SerializeField] private Terrain _terrain;
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField, HideInInspector] private Bounds _bounds;
        [SerializeField] private float _pathWidth = 12f;
        [SerializeField] private bool _ignoreTerrainHeight = false;
        [SerializeField] private AnimationCurve _heightCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.75f, 1f, 0f, 0f), new Keyframe(1f, 0.5f, 8f, 0f) });
        [SerializeField] private AnimationCurve _falloff = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f, 0f, 0f), new Keyframe(0.75f, 1f, 0f, 0f), new Keyframe(1f, 0f, 0f, 0f) });
        [SerializeField] private Vector2 _height = new Vector2(0.5f, -0.5f);
        [SerializeField] private bool _modifyHeight = true;
        [SerializeField] private bool _modifySplat = true;
        [SerializeField] private Color _splatColor = new Color(0f, 0f, 0f, 1f);
        [SerializeField] private AnimationCurve _splatBlend = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -1f, 0f) });

        public Vector3 Min => _bounds.center - new Vector3(0f, _bounds.size.y * 0.5f, 0f) - transform.right * _bounds.size.x * 0.5f - transform.forward * _bounds.size.z * 0.5f;
        public Vector3 Max => _bounds.center + new Vector3(0f, _bounds.size.y * 0.5f, 0f) + transform.right * _bounds.size.x * 0.5f + transform.forward * _bounds.size.z * 0.5f;

        public Vector3 XCorner => Min + transform.right * _bounds.size.x;
        public Vector3 ZCorner => Min + transform.forward * _bounds.size.z;

        private void OnValidate()
        {
            _splineContainer = GetComponent<SplineContainer>();
            _terrain = GetComponentInParent<Terrain>();
            _bounds = CalculateBounds();
        }

        public void Initialize()
        {
            _bounds = CalculateBounds();
        }

        public bool GetHeight(Vector3 position, out float height)
        {
            if (!_modifyHeight || !_bounds.Contains(position))
            {
                height = 0f;
                return false;
            }

            float halfWidth = _pathWidth * 0.5f;

            Vector3 localPosition = transform.InverseTransformPoint(position);
            float distance = GetMinDistanceToSplines(localPosition, out float3 nearest, out float t);
            Vector3 flatNearest = new Vector3(nearest.x, localPosition.y, nearest.z);
            float flatDistance = Vector3.Distance(localPosition, flatNearest);
            if (flatDistance > halfWidth || (!_ignoreTerrainHeight && distance > halfWidth))
            {
                height = 0f;
                return false;
            }

            Vector3 worldPos = transform.TransformPoint(nearest);
            float distanceNormalized = (_ignoreTerrainHeight ? flatDistance : distance) / halfWidth;
            height = worldPos.y + Mathf.Lerp(_height.y, _height.x, _heightCurve.Evaluate(distanceNormalized));
            float falloff = _falloff.Evaluate(distanceNormalized);
            height = Mathf.Lerp(position.y, height, falloff);
            return true;
        }

        public bool GetSplat(Vector3 position, Color splatIn, out Color splatOut)
        {
            if (!_modifySplat || !_bounds.Contains(position))
            {
                splatOut = Color.black;
                return false;
            }

            float halfWidth = _pathWidth * 0.5f;

            Vector3 localPosition = transform.InverseTransformPoint(position);
            float distance = GetMinDistanceToSplines(localPosition, out float3 nearest, out float t);
            Vector3 flatLocalPosition = localPosition;
            flatLocalPosition.y = nearest.y;
            float flatDistance = Vector3.Distance(flatLocalPosition, nearest);
            if (flatDistance > halfWidth || (!_ignoreTerrainHeight && distance > halfWidth))
            {
                splatOut = Color.black;
                return false;
            }

            float distanceNormalized = distance / halfWidth;
            float blend = _splatBlend.Evaluate(distanceNormalized);
            if (_ignoreTerrainHeight) blend = 1f;
            splatOut = Color.Lerp(splatIn, _splatColor, blend);
            return true;
        }

        private float GetMinDistanceToSplines(Vector3 localPosition, out float3 nearest, out float t)
        {
            float min = Mathf.Infinity;
            nearest = float3.zero;
            t = 0f;

            foreach (Spline spline in _splineContainer.Splines)
            {
                float distance = SplineUtility.GetNearestPoint(spline, localPosition, out float3 near, out float position);
                if (distance < min)
                {
                    min = distance;
                    nearest = near;
                    t = position;
                }
            }

            return min;
        }

        [Button]
        public void UpdateTerrainHeightAndSplat()
        {
            TerrainAutomator automation = GetComponentInParent<TerrainAutomator>();
            automation?.UpdateHeightMap();
            automation?.UpdateSplatmap();
        }

        [Button]
        public void UpdateTerrainAll()
        {
            TerrainAutomator automation = GetComponentInParent<TerrainAutomator>();
            automation?.UpdateAll();
        }

        private Bounds CalculateBounds()
        {
            Bounds bounds = new Bounds(_splineContainer.EvaluatePosition(0f), Vector3.one);
            float padding = _pathWidth;
            int stepCount = 20;
            float step = 1f / stepCount;

            foreach (Spline spline in _splineContainer.Splines)
            {
                for (int i = 0; i <= stepCount; i++)
                {
                    Vector3 position = spline.EvaluatePosition(i * step);
                    bounds.Encapsulate(position);
                }
            }

            bounds.size += Vector3.one * padding;

            if (_ignoreTerrainHeight && _terrain)
            {
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                min.y = _terrain.transform.position.y;
                max.y = _terrain.transform.position.y + _terrain.terrainData.size.y;
                bounds.min = min;
                bounds.max = max;
            }

            return bounds;
        }

        private void OnDrawGizmosSelected()
        {
            ITerrainModifier tm = this;
            if (float.IsNaN(_bounds.size.x)) return;
            if (!_splineContainer || !tm.IsVisible(_bounds)) return;

            Gizmos.DrawWireCube(_bounds.center, _bounds.size);

            Vector3 cameraPosition = Camera.main.transform.position;
#if UNITY_EDITOR
            cameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
#endif
            Vector3 closestPoint = _bounds.ClosestPoint(cameraPosition);
            float boundsDistance = Vector3.Distance(cameraPosition, closestPoint);
            float stepDistance = Mathf.Clamp(boundsDistance / 4f, 4f, 20f);
            float4x4 TRS = float4x4.TRS(transform.position, transform.rotation, transform.localScale);

            foreach (Spline spline in _splineContainer.Splines)
            {
                float length = SplineUtility.CalculateLength(spline, TRS);
                int stepCount = Mathf.RoundToInt(length / stepDistance);
                float step = 1f / stepCount;

                Vector3[] left = new Vector3[stepCount + 1];
                Vector3[] right = new Vector3[stepCount + 1];

                for (int i = 0; i <= stepCount; i++)
                {
                    float t = i * step;
                    Vector3 position = spline.EvaluatePosition(t);
                    float3 tan = spline.EvaluateTangent(t);
                    Vector3 tanget = ((Vector3)tan).normalized;
                    Vector3 normal = Vector3.Cross(Vector3.up, tanget);
                    Vector3 start = position - normal * _pathWidth * 0.5f;
                    left[i] = start;
                    right[i] = start + normal * _pathWidth;
                }

                Gizmos.DrawLineStrip(left, false);
                Gizmos.DrawLineStrip(right, false);
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Terrain/Terrain Path", false)]
        private static void Create(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Terrain Path");
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            go.AddComponent<TerrainPath>();
            Selection.activeGameObject = go;
        }
#endif
    }
}