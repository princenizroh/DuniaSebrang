using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public void LoadScene()
    {
        SceneManager.LoadScene("Level-1");
    }
}
