using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TerrainAutomation
{
    public static class TerrainUpdater
    {
        public static void UpdateHeightMap(Terrain terrain, Texture2D heightMap, Vector3 terrainSize)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            int texWidth = heightMap.width;
            int texHeight = heightMap.height;
            terrainData.heightmapResolution = texWidth;
            terrainData.size = terrainSize;

            float[,] heights = new float[texWidth, texHeight];

            ITerrainModifier[] modifiers = terrain.GetComponentsInChildren<ITerrainModifier>();

            Color[] heightPixels = heightMap.GetPixels(0, 0, texWidth, texHeight);
            Color[,] heightPixels2D = GetColors2D(heightPixels, texWidth, texHeight);

            for (int x = 0; x < texWidth; x++)
            {
                for (int y = 0; y < texHeight; y++)
                {
                    Vector2 normalizedPosition = new Vector2((float)x / texWidth, (float)y / texHeight);
                    Vector3 localPosition = new Vector3(normalizedPosition.x * terrainSize.x, 0f, normalizedPosition.y * terrainSize.z);
                    Vector3 position = terrainPosition + localPosition;

                    float height = heightPixels2D[y,x].r;
                    Vector3 worldPosition = new Vector3(position.x, terrainPosition.y + height * terrainSize.y, position.z);

                    for (int i = 0; i < modifiers.Length; i++)
                    {
                        ITerrainModifier stamp = modifiers[i];
                        float worldHeight = terrainPosition.y + height * terrainSize.y;
                        worldPosition.y = worldHeight;
                        if (stamp.GetHeight(worldPosition, out float stampHeight))
                        {
                            float normalizedStampHeight = (stampHeight - terrainPosition.y) / terrainSize.y;
                            height = normalizedStampHeight;
                        }
                    }

                    heights[y,x] = height;
                }
            }

            terrainData.SetHeights(0, 0, heights);

            timer.Stop();
            Debug.Log($"Update heights time: {timer.Elapsed.TotalSeconds}");
        }

        public static void UpdateSplatMap(Terrain terrain, Texture2D heightMap, Texture2D splatMap, Vector3 terrainSize)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            int texWidth = splatMap.width;
            int texHeight = splatMap.height;
            terrainData.alphamapResolution = texWidth;

            float[,,] alphaMap = new float[texWidth, texHeight, 4];
            ITerrainModifier[] modifiers = terrain.GetComponentsInChildren<ITerrainModifier>();

            Color[] heightPixels = heightMap.GetPixels(0, 0, texWidth, texHeight);
            Color[,] heightPixels2D = GetColors2D(heightPixels, texWidth, texHeight);
            Color[] splatPixels = splatMap.GetPixels(0, 0, texWidth, texHeight);
            Color[,] splatPixels2D = GetColors2D(splatPixels, texWidth, texHeight);

            for (int x = 0; x < texWidth; x++)
            {
                for (int y = 0; y < texHeight; y++)
                {
                    Color pixel = splatPixels2D[y,x];

                    Vector2 normalizedPosition = new Vector2((float)x / texWidth, (float)y / texHeight);
                    Vector3 localPosition = new Vector3(normalizedPosition.x * terrainSize.x, 0f, normalizedPosition.y * terrainSize.z);
                    Vector3 position = terrainPosition + localPosition;

                    float height = heightPixels2D[y,x].r;
                    Vector3 worldPosition = new Vector3(position.x, terrainPosition.y + height * terrainSize.y, position.z);

                    for (int i = 0; i < modifiers.Length; i++)
                    {
                        ITerrainModifier stamp = modifiers[i];
                        if (stamp.GetSplat(worldPosition, pixel, out Color modifiedColor))
                        {
                            pixel = modifiedColor;
                        }
                    }

                    alphaMap[y, x, 0] = pixel.r;
                    alphaMap[y, x, 1] = pixel.g;
                    alphaMap[y, x, 2] = pixel.b;
                    alphaMap[y, x, 3] = pixel.a;
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMap);

            UpdateGlobalSplatTexture(terrainData.GetAlphamapTexture(0), terrainSize);

            timer.Stop();
            Debug.Log($"Update splats time: {timer.Elapsed.TotalSeconds}");
        }

        private static Color[,] GetColors2D(Color[] colors, int width, int height)
        {
            Color[,] colors2D = new Color[width, height];
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    colors2D[x, y] = colors[index++];
                }
            }
            return colors2D;
        }

        public static void AssignTerrainLayerSet(Terrain terrain, List<TerrainLayer> layers)
        {
            TerrainData terrainData = terrain.terrainData;
            terrainData.terrainLayers = layers.ToArray();
        }

        public static void UpdateGlobalSplatTexture(Texture2D splatMap, Vector4 size)
        {
            Shader.SetGlobalTexture("_TerrainSplatMap", splatMap);
            Shader.SetGlobalVector("_TerrainSize", size);
        }
    }
}