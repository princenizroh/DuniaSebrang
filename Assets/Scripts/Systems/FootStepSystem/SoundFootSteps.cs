using UnityEngine;

namespace DS
{
    public class SoundFootSteps : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stoneClips;
        [SerializeField] private AudioClip[] grassClips;

        private AudioSource audioSource;
        private TerrainDetector terrainDetector;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            terrainDetector = new TerrainDetector(); // Not MonoBehaviour, just a helper class
        }

        public void Step()
        {
            AudioClip clip = GetRandomClip();
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        private AudioClip GetRandomClip()
        {
            int terrainIndex = terrainDetector.GetActiveTerrainTextureIdx(transform.position);

            switch (terrainIndex)
            {
                case 0: // Assuming 0 = stone
                    return stoneClips.Length > 0
                        ? stoneClips[Random.Range(0, stoneClips.Length)]
                        : null;

                case 1: // Assuming 1 = grass
                default:
                    return grassClips.Length > 0
                        ? grassClips[Random.Range(0, grassClips.Length)]
                        : null;
            }
        }
    }
}
