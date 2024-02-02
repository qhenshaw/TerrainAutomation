using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace TerrainAutomation
{
    public static class TerrainTreePlacement
    {
        public static void AssignTrees(Terrain terrain, List<TreeLayerConfig> trees)
        {
            TerrainData data = terrain.terrainData;
            TreePrototype[] prototypes = new TreePrototype[trees.Count];
            for (int i = 0; i < prototypes.Length; i++)
            {
                TreePrototype prototype = new TreePrototype();
                prototype.navMeshLod = 0;
                prototype.prefab = trees[i].Prefab.gameObject;
                prototypes[i] = prototype;
            }

            data.treePrototypes = prototypes;
            data.RefreshPrototypes();
        }

        public static void PlaceTrees(Terrain terrain, List<TreeLayerConfig> trees, int seed, int count, bool keepExistingTrees)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            AssignTrees(terrain, trees);

            // initialize from (random) seed
            if (seed <= 0) seed = Random.Range(0, int.MaxValue);
            Random.InitState(seed);

            // get terrain data
            TerrainData data = terrain.terrainData;
            Vector3 size = data.size;
            Vector3 terrainPosition = terrain.GetPosition();

            TreePrototype[] treePrototypes = data.treePrototypes;
            if (treePrototypes.Length < 1)
            {
                Debug.LogWarning("No trees assigned to terrain!");
                return;
            }

            Texture2D splat = data.GetAlphamapTexture(0);
            if (splat == null)
            {
                Debug.LogWarning("No splatmap found on terrain!");
                return;
            }

            if (!keepExistingTrees) terrain.terrainData.treeInstances = new TreeInstance[0];

            int successCount = 0;
            int maxAttempts = count * 100;
            int attempts = 0;
            while (successCount < count && attempts < maxAttempts)
            {
                attempts++;

                int prototypeIndex = GetRandomWeightedTreeIndex(trees);
                TreeLayerConfig config = trees[prototypeIndex];

                // randomize position
                Vector2 normalized2DPosition = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));

                // scale
                float scale = config.ScaleRange.Evaluate(Random.value);

                // filter by splat layer
                Color splatPixel = SampleTexture(splat, normalized2DPosition);
                Vector4 splatWeights = new Vector4(splatPixel.r * config.SplatFilter.x, splatPixel.g * config.SplatFilter.y, splatPixel.b * config.SplatFilter.z, splatPixel.a * config.SplatFilter.w);
                float maxWeight = splatWeights.x + splatWeights.y + splatWeights.z + splatWeights.w;
                if (maxWeight < config.SplatCutoff) continue;
                scale *= config.ScaleMultiplier;

                // 2D => world => terrain position
                Vector3 localPosition = new Vector3(normalized2DPosition.x * size.x, 0f, normalized2DPosition.y * size.z);
                Vector3 worldPosition = localPosition + terrainPosition;
                float height = terrain.SampleHeight(worldPosition);
                if (height < config.MinMaxPlacementHeight.x || height > config.MinMaxPlacementHeight.y) continue;
                float slope = data.GetSteepness(normalized2DPosition.x, normalized2DPosition.y);
                if (slope > config.MaxSlope) continue;
                Vector3 normalizedPosition = new Vector3(localPosition.x / size.x, height / size.y, localPosition.z / size.z);

                TreeInstance instance = new TreeInstance
                {
                    position = normalizedPosition,
                    prototypeIndex = prototypeIndex,
                    widthScale = scale,
                    heightScale = scale,
                    color = Color.white,
                    lightmapColor = Color.white,
                    rotation = Random.Range(0f, Mathf.PI * 2f)
                };
                terrain.AddTreeInstance(instance);

                successCount++;
            }

            timer.Stop();
            Debug.Log($"Update trees time: {timer.Elapsed.TotalSeconds}");
        }

        private static Color SampleTexture(Texture2D texture, Vector2 position)
        {
            Vector2Int texturePosition = new Vector2Int(Mathf.RoundToInt(position.x * texture.width), Mathf.RoundToInt(position.y * texture.height));
            return texture.GetPixel(texturePosition.x, texturePosition.y);
        }

        private static int GetRandomWeightedTreeIndex(List<TreeLayerConfig> trees)
        {
            float weightTotal = 0f;
            for (int i = 0; i < trees.Count; i++)
            {
                weightTotal += trees[i].DistributionWeight;
            }

            float randomValue = Random.Range(0f, weightTotal);

            float currentWeight = 0f;
            for (int i = 0; i < trees.Count; i++)
            {
                currentWeight += trees[i].DistributionWeight;
                if (randomValue < currentWeight) return i;
            }

            return 0;
        }
    }
}