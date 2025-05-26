using UnityEngine;

namespace DS
{
    public class TerrainDetector
    {
        private TerrainData terrainData;
        private int alphamapWidth;
        private int alphamapHeight;
        private float[,,] splatmapData;
        private int numTextures;

        public TerrainDetector()
        {
            if (Terrain.activeTerrain == null)
            {
                Debug.LogError("No active terrain found!");
                return;
            }

            terrainData = Terrain.activeTerrain.terrainData;
            alphamapWidth = terrainData.alphamapWidth;
            alphamapHeight = terrainData.alphamapHeight;

            splatmapData = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
            numTextures = splatmapData.GetLength(2);
        }

        private Vector3 ConvertToSplatMapCoordinate(Vector3 worldPosition)
        {
            Terrain ter = Terrain.activeTerrain;
            Vector3 terPosition = ter.transform.position;

            float relativeX = (worldPosition.x - terPosition.x) / ter.terrainData.size.x;
            float relativeZ = (worldPosition.z - terPosition.z) / ter.terrainData.size.z;

            int mapX = Mathf.Clamp(Mathf.RoundToInt(relativeX * alphamapWidth), 0, alphamapWidth - 1);
            int mapZ = Mathf.Clamp(Mathf.RoundToInt(relativeZ * alphamapHeight), 0, alphamapHeight - 1);

            return new Vector3(mapX, 0, mapZ);
        }

        public int GetActiveTerrainTextureIdx(Vector3 position)
        {
            if (terrainData == null || splatmapData == null)
                return 0;

            Vector3 terrainCoord = ConvertToSplatMapCoordinate(position);
            int x = (int)terrainCoord.x;
            int z = (int)terrainCoord.z;

            int activeIndex = 0;
            float maxOpacity = 0f;

            for (int i = 0; i < numTextures; i++)
            {
                float opacity = splatmapData[z, x, i];
                if (opacity > maxOpacity)
                {
                    maxOpacity = opacity;
                    activeIndex = i;
                }
            }

            return activeIndex;
        }
    }
}
