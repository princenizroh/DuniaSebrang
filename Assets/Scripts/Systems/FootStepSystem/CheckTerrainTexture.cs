using UnityEngine;

namespace DS
{
    public class CheckTerrainTexture : MonoBehaviour
    {
        public Transform playerTransform;
        public Terrain terrainObject;

        public int posX;
        public int posZ;
        public float[] textureValue;

        void Start()
        {
            terrainObject = Terrain.activeTerrain;
            playerTransform = gameObject.transform;

            int numTextures = terrainObject.terrainData.alphamapLayers;
            textureValue = new float[numTextures];
        }

        void Update()
        {
            GetTerrainTexture();
        }

        void GetTerrainTexture()
        {
            UpdatePosition();
            CheckTexture();
        }

        void UpdatePosition()
        {
            Vector3 terrainPosition = playerTransform.position - terrainObject.transform.position;
            TerrainData terrainData = terrainObject.terrainData;

            float xCoord = terrainPosition.x / terrainData.size.x;
            float zCoord = terrainPosition.z / terrainData.size.z;

            posX = Mathf.Clamp((int)(xCoord * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
            posZ = Mathf.Clamp((int)(zCoord * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);
        }

        void CheckTexture()
        {
            float[,,] splatMap = terrainObject.terrainData.GetAlphamaps(posX, posZ, 1, 1);
            for (int i = 0; i < textureValue.Length; i++)
            {
                textureValue[i] = splatMap[0, 0, i];
            }
        }
    }
}
