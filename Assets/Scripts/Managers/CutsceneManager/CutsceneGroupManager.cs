using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace DS
{
    [System.Serializable]
    public class AreaCutsceneData
    {
        public string areaName;
        public List<PlayableDirector> directors = new();
    }

    public class CutsceneGroupManager : MonoBehaviour
    {
        [SerializeField] private List<AreaCutsceneData> areaCutscenes = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoSetupAreaCutscenes();
        }
#endif

        private void AutoSetupAreaCutscenes()
        {
            areaCutscenes.Clear(); // Reset semua supaya bersih

            foreach (Transform area in transform) // Misal Area_0, Area_1, dll
            {
                AreaCutsceneData data = new()
                {
                    areaName = area.name
                };

                foreach (PlayableDirector director in area.GetComponentsInChildren<PlayableDirector>(true))
                {
                    data.directors.Add(director);
                }

                // Sorting biar rapih berdasarkan nama GameObject
                data.directors.Sort((a, b) => string.Compare(a.gameObject.name, b.gameObject.name));

                areaCutscenes.Add(data);
            }
        }

        public PlayableDirector GetDirector(string areaName, int stepIndex)
        {
            var areaData = areaCutscenes.Find(area => area.areaName == areaName);

            if (areaData == null)
            {
                Debug.LogError($"[CutsceneGroupManager] Area {areaName} tidak ditemukan!");
                return null;
            }

            if (stepIndex < 0 || stepIndex >= areaData.directors.Count)
            {
                Debug.LogError($"[CutsceneGroupManager] Step index {stepIndex} invalid di Area {areaName}!");
                return null;
            }

            return areaData.directors[stepIndex];
        }
    }
}
