using System.Collections.Generic;
using UnityEngine;

namespace TerrainAutomation
{
    public static class TerrainDetailPlacement
    {
        public static void UpdateDetails(Terrain terrain, List<DetailLayerConfig> layers)
        {
            TerrainData data = terrain.terrainData;
            DetailPrototype[] prototpes = new DetailPrototype[layers.Count];
            for (int i = 0; i < prototpes.Length; i++)
            {
                DetailLayerConfig layer = layers[i];
                DetailPrototype prototype = new DetailPrototype();
                prototype.usePrototypeMesh = true;
                prototype.prototype = layer.Prefab;
                prototype.density = layer.DensityScale;
                prototype.targetCoverage = layer.Density;
                prototype.alignToGround = layer.AlignToGround;
                prototype.minWidth = layer.ScaleRange.x;
                prototype.minHeight = layer.ScaleRange.x;
                prototype.maxWidth = layer.ScaleRange.y;
                prototype.maxHeight = layer.ScaleRange.y;
                prototype.noiseSeed = layer.NoiseSeed;
                prototype.noiseSpread = 0.1f;
                prototype.positionJitter = 1f;
                prototype.holeEdgePadding = 0f;
                prototype.useInstancing = true;
                prototype.renderMode = DetailRenderMode.VertexLit;
                prototype.useDensityScaling = true;
                prototpes[i] = prototype;
            }

            data.detailPrototypes = prototpes;
            data.RefreshPrototypes();
        }

        public static void PlaceDetails(Terrain terrain, List<DetailLayerConfig> layers)
        {
            UpdateDetails(terrain, layers);

            // get terrain data
            TerrainData data = terrain.terrainData;
            Vector3 size = data.size;
            Vector3 terrainPosition = terrain.GetPosition();

            Texture2D splat = data.GetAlphamapTexture(0);
            if (splat == null)
            {
                Debug.LogWarning("No splatmap found on terrain!");
                return;
            }

            for (int i = 0; i < layers.Count; i++)
            {
                DetailLayerConfig config = layers[i];

                // get detail layer
                int[,] layer = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, i);

                for (int x = 0; x < data.detailWidth; x++)
                {
                    for (int y = 0; y < data.detailHeight; y++)
                    {
                        // 2D => world => terrain position
                        Vector2 normalized2DPosition = new Vector2((float)y / data.detailHeight, (float)x / data.detailWidth);
                        Vector3 localPosition = new Vector3(normalized2DPosition.x * size.x, 0f, normalized2DPosition.y * size.z);
                        Vector3 worldPosition = localPosition + terrainPosition;
                        float height = terrain.SampleHeight(worldPosition);
                        if (height < config.MinMaxPlacementHeight.x || height > config.MinMaxPlacementHeight.y)
                        {
                            layer[x, y] = 0;
                            continue;
                        }
                        float slope = data.GetSteepness(normalized2DPosition.x, normalized2DPosition.y);
                        if (slope > config.MaxSlope)
                        {
                            layer[x, y] = 0;
                            continue;
                        }

                        Color splatPixel = splat.GetPixel(y, x);
                        Vector4 splatWeights = new Vector4(splatPixel.r * config.SplatFilter.x, splatPixel.g * config.SplatFilter.y, splatPixel.b * config.SplatFilter.z, splatPixel.a * config.SplatFilter.w);
                        float maxWeight = splatWeights.x + splatWeights.y + splatWeights.z + splatWeights.w;
                        if (maxWeight < config.SplatCutoff) maxWeight = 0f;
                        int density = Mathf.RoundToInt(config.Density * maxWeight);

                        layer[x, y] = density;
                    }
                }

                data.SetDetailLayer(0, 0, i, layer);
            }
        }
    }
}