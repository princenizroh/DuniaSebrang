using UnityEditor;
using UnityEngine;

public class NoiseMakerEditor : EditorWindow
{
    public string textureName = "Volume Fog V2/3DTexture";
    public ComputeShader computeShader;
    public Material fogMat;
    public int size = 128, height = 128, octaves = 8;
    public float persistence = 0.7f;
    public float noiseSize = 4, seed = 0;

    private enum CustomTextureFormat
    {
        R8,
        R16,
        RHalf,
        RFloat
    }

    private CustomTextureFormat selectedFormat = CustomTextureFormat.R8;

    private Texture3D tex3D;

    [MenuItem("Window/Noise Maker")]
    public static void ShowWindow()
    {
        GetWindow<NoiseMakerEditor>("Noise Maker");
    }

    private void OnGUI()
    {
        string[] guids = AssetDatabase.FindAssets("Noise 3D gen t:ComputeShader");
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);

        GUILayout.Label("Noise Maker Settings", EditorStyles.boldLabel);

        // Fields for public variables
        textureName = EditorGUILayout.TextField("Texture Name", textureName);
        fogMat = (Material)EditorGUILayout.ObjectField("Fog Material", fogMat, typeof(Material), false);

        size = EditorGUILayout.IntField("Size (Width)", size);
        height = EditorGUILayout.IntField("Height (Depth)", height);
        octaves = EditorGUILayout.IntField("Octaves", octaves);
        persistence = EditorGUILayout.FloatField("Persistence", persistence);
        noiseSize = EditorGUILayout.FloatField("Noise Size", noiseSize);
        seed = EditorGUILayout.FloatField("Seed", seed);

        selectedFormat = (CustomTextureFormat)EditorGUILayout.EnumPopup("Texture Format", selectedFormat);

        // Generate Noise Button
        if (GUILayout.Button("Generate Noise"))
        {
            if (ValidateInput())
            {
                CreateTexture();
                Debug.Log("Noise texture generated successfully!");
            }
        }

        // Save Noise Button
        if (GUILayout.Button("Save Noise"))
        {
            if (tex3D == null)
            {
                Debug.LogError("No texture to save. Generate a texture first!");
            }
            else
            {
                SaveTexture3D();
                Debug.Log("Noise texture saved successfully!");
            }
        }
    }

    private TextureFormat GetSelectedTextureFormat()
    {
        return (TextureFormat)System.Enum.Parse(typeof(TextureFormat), selectedFormat.ToString());
    }

    private void CreateTexture()
    {
        tex3D = new Texture3D(size, height, size, GetSelectedTextureFormat(), false);

        if (fogMat != null)
        {
            fogMat.SetTexture("_Noise3D", tex3D);
        }

        int pixels = size * size * height;
        ComputeBuffer buffer = new ComputeBuffer(pixels, sizeof(float));

        computeShader.SetBuffer(0, "Result", buffer);
        computeShader.SetFloat("size", size);
        computeShader.SetFloat("height", height);
        computeShader.SetFloat("seed", seed);
        computeShader.SetFloat("noiseSize", noiseSize);
        computeShader.SetFloat("persistence", persistence);
        computeShader.SetInt("octaves", octaves);
        computeShader.Dispatch(0, size / 8, height / 8, size / 8);

        float[] noise = new float[pixels];
        Color[] colors = new Color[pixels];
        buffer.GetData(noise);
        buffer.Release();

        for (int i = 0; i < pixels; i++)
        {
            colors[i] = new Color(noise[i], 0, 0, 0);
        }
        tex3D.SetPixels(colors);
        tex3D.Apply();
    }

    private void SaveTexture3D()
    {
        string assetPath = "Assets/" + textureName + ".asset";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.Refresh();
        AssetDatabase.CreateAsset(tex3D, assetPath);
    }

    private bool ValidateInput()
    {
        if (computeShader == null)
        {
            Debug.LogError("Compute Shader is not assigned!");
            return false;
        }

        if (fogMat == null)
        {
            Debug.LogWarning("Fog Material is not assigned. Texture won't be applied to any material.");
        }

        if (size <= 0 || height <= 0)
        {
            Debug.LogError("Size and Height must be greater than zero!");
            return false;
        }

        return true;
    }
}
