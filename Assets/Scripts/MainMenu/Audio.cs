using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace DS
{
    public class Audio : MonoBehaviour
    {
        public AudioMixer audioMixer;
        public Slider musicSlider;
        public Slider sfxSlider;
    
        private void Start()
        {
            LoadVolume();
            MusicManager.Instance.PlayMusic("MainMenu");
        }
    
    public void Play()
    {
        // Stop main menu music before starting game music
        MusicManager.Instance.StopMusic();
        
        // Start game music
        MusicManager.Instance.PlayMusic("Game");
    }
    
        public void UpdateMusicVolume(float volume)
        {
            audioMixer.SetFloat("MusicVolume", volume);
        }
    
        public void UpdateSoundVolume(float volume)
        {
            audioMixer.SetFloat("SFXVolume", volume);
        }
    
        public void SaveVolume()
        {
            audioMixer.GetFloat("MusicVolume", out float musicVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    
            audioMixer.GetFloat("SFXVolume", out float sfxVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
    
        public void LoadVolume()
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        }
    }
}
