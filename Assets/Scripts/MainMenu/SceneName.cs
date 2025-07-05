using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneName: MonoBehaviour
{
    [Header("Nama Scene yang Akan Dimuat")]
    public string namaScene;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(namaScene))
        {
            SceneManager.LoadScene(namaScene);
        }
        else
        {
            Debug.LogWarning("Nama scene belum diisi!");
        }
    }
}
