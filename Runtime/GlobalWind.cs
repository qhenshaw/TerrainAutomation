using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainAutomation
{
    [ExecuteInEditMode]
    public class GlobalWind : MonoBehaviour
    {
        [SerializeField] private float _strength = 1f;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private Texture2D _windNoise;

        private void OnValidate()
        {
            Shader.SetGlobalTexture("_GlobalWindNoiseMap", _windNoise);
        }

        private void Start()
        {
            Shader.SetGlobalTexture("_GlobalWindNoiseMap", _windNoise);
        }

        private void Update()
        {
            Vector2 direction = new Vector2(transform.forward.x, transform.forward.z);
            direction.Normalize();
            Shader.SetGlobalVector("_GlobalWindDirection", direction);
            Shader.SetGlobalFloat("_GlobalWindStrength", _strength);
            Shader.SetGlobalFloat("_GlobalWindSpeed", _speed);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}
