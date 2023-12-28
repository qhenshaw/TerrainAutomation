using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainAutomation
{
    public interface ITerrainModifier
    {
        void Initialize();
        bool GetHeight(Vector3 position, out float height);
        bool GetSplat(Vector3 position, Color splatIn, out Color splatOut);

        bool IsVisible(Bounds bounds)
        {
#if UNITY_EDITOR
            var planes = GeometryUtility.CalculateFrustumPlanes(UnityEditor.SceneView.currentDrawingSceneView.camera);
            bool visible = GeometryUtility.TestPlanesAABB(planes, bounds);
            return visible;
#endif
            return false;
        }

        float GetScreenSize(Bounds bounds)
        {
#if UNITY_EDITOR
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
            Vector2 screenMin = cam.WorldToViewportPoint(min);
            Vector2 screenMax = cam.WorldToViewportPoint(max);
            return (screenMax - screenMin).magnitude;
#endif
            return 0f;
        }
    }
}