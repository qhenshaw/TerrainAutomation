using UnityEditor;
using TerrainAutomation;

namespace TerrainAutomation.Editor
{
    [CustomEditor(typeof(LoftRiverBehaviour))]
    [CanEditMultipleObjects]
    class SplineWidthEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.delayCall += () =>
                {
                    foreach (var target in targets)
                        ((LoftRiverBehaviour)target).LoftAllRoads();
                };
            }
        }
    }
}