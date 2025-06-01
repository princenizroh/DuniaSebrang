using UnityEngine;

namespace DS
{
    public class TerrainDetector
    {
        private Terrain terrain;

        public TerrainDetector()
        {
            terrain = Terrain.activeTerrain;
        }

        public int GetActiveTerrainTextureIdx(Vector3 worldPos)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = worldPos - terrain.transform.position;

            int mapX = Mathf.RoundToInt((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = Mathf.RoundToInt((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            float maxMix = 0f;
            int maxIndex = 0;

            for (int i = 0; i < terrainData.alphamapLayers; i++)
            {
                if (splatmapData[0, 0, i] > maxMix)
                {
                    maxMix = splatmapData[0, 0, i];
                    maxIndex = i;
                }
            }

            return maxIndex; // returns the index of the most dominant texture
        }
    }
}
