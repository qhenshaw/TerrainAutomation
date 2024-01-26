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

        bool IsVisible(Vector3 point)
        {
#if UNITY_EDITOR
            Vector3 viewPortPoint = UnityEditor.SceneView.currentDrawingSceneView.camera.WorldToViewportPoint(point);
            bool visible = viewPortPoint.x > 0 && viewPortPoint.x < 1f && viewPortPoint.y > 0 && viewPortPoint.y < 1f;
            return visible;
#endif
            return false;
        }
    }
}