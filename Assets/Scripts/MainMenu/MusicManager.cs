using System.Collections;
using UnityEngine;

namespace DS
{
    public class MusicManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public static MusicManager Instance;
    
        [SerializeField]
        private MusicLibrary musicLibrary;
        [SerializeField]
        private AudioSource musicSource;
    
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    
        public void PlayMusic(string trackName, float fadeDuration = 0.5f)
        {
            StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeDuration));
        }
    
        IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
        {
            float percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * 1 / fadeDuration;
                musicSource.volume = Mathf.Lerp(1f, 0, percent);
                yield return null;
            }
    
            musicSource.clip = nextTrack;
            musicSource.Play();
    
            percent = 0;
            while (percent < 1)
            {
                percent += Time.deltaTime * 1 / fadeDuration;
                musicSource.volume = Mathf.Lerp(0, 1f, percent);
                yield return null;
            }
        }

        public void StopMusic(float fadeDuration = 0.5f)
        {
            StartCoroutine(AnimateMusicFadeOut(fadeDuration));
        }
        
        IEnumerator AnimateMusicFadeOut(float fadeDuration = 0.5f)
        {
            float startVolume = musicSource.volume;
            float percent = 0;
            
            while (percent < 1)
            {
                percent += Time.deltaTime * 1 / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = 1f; // Reset volume for next track
        }
    }
}
