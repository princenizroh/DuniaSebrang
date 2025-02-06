using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AssetLoadDialog
{
    private static bool isDialogShown = false;
    private static double loadTime;

    static AssetLoadDialog()
    {
        loadTime = EditorApplication.timeSinceStartup;

        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (!isDialogShown && EditorApplication.timeSinceStartup - loadTime >= (4 * 60))
        {
            ShowInformationDialog();
            isDialogShown = true;
        }
    }

    private static void ShowInformationDialog()
    {
        bool result = EditorUtility.DisplayDialog(
            "Thank You for Using Noise Maker!",
            "Thank you for downloading and using Noise Maker! 🎉\n\n" +
            "This asset is free, and I hope it helps bring your creative projects to life.\n\n" +
            "If you'd like to support my work, please take a moment to explore my other assets in the Unity Asset Store. " +
            "Your support means the world and helps me continue creating tools like this for the community.\n\n" +
            "Happy creating!",
            "Show Me Your Other Assets!",
            "Maybe Later"
        );
        if (result)
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/14079");
        }
    }
}
