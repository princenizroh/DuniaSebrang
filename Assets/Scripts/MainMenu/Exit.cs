using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{

    public void QuitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void SaveGame()
    {
        // Implement save game logic here
        // Example: save data to PlayerPrefs
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully.");
    }

    public void SaveAndQuit()
    {
        SaveGame();
        QuitGame();
    }
}
