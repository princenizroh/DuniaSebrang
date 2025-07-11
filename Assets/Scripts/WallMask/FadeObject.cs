using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class FadeObject : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform target;
        [SerializeField] private Camera camera;
        [SerializeField] [Range(0, 1f)] private float fadeAlpha = 0.33f;
        [SerializeField] private bool retainShadows = true;
        [SerializeField] private Vector3 targetPositionOffset = Vector3.up;
        [SerializeField] private float fadeSpeed = 1f;

        [Header("Read Only Data")]
        [SerializeField] private List<FadingObject> objectBlockingView = new List<FadingObject>();
        private Dictionary<FadingObject, Coroutine> runningCoroutines = new Dictionary<FadingObject, Coroutine>();
        private RaycastHit[] hits = new RaycastHit[10];

        private void Start()
        {
            StartCoroutine(CheckForObjects());
        }

        private IEnumerator CheckForObjects()
        {
            while (true)
            {
                int hitCount = Physics.RaycastNonAlloc(
                    camera.transform.position,
                    (target.position + targetPositionOffset - camera.transform.position).normalized,
                    hits,
                    Vector3.Distance(camera.transform.position, target.position + targetPositionOffset),
                    layerMask
                );

                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        FadingObject fadingObject = GetFadingObjectFromHit(hits[i]);
                        if (fadingObject != null && !objectBlockingView.Contains(fadingObject))
                        {
                            if (runningCoroutines.ContainsKey(fadingObject))
                            {
                                StopCoroutine(runningCoroutines[fadingObject]);
                                runningCoroutines.Remove(fadingObject);
                            }
                            Coroutine fadeOut = StartCoroutine(FadeOut(fadingObject));
                            runningCoroutines.Add(fadingObject, fadeOut);
                            objectBlockingView.Add(fadingObject);
                        }
                    }
                }

                CheckNoLongerHitObjects();
                ClearHits();

                yield return null;
            }
        }

        private void CheckNoLongerHitObjects()
        {
            List<FadingObject> toRemove = new List<FadingObject>();

            foreach (FadingObject fadingObject in objectBlockingView)
            {
                bool isStillHit = false;

                foreach (RaycastHit hit in hits)
                {
                    FadingObject hitObj = GetFadingObjectFromHit(hit);
                    if (hitObj != null && hitObj == fadingObject)
                    {
                        isStillHit = true;
                        break;
                    }
                }

                if (!isStillHit)
                {
                    if (runningCoroutines.ContainsKey(fadingObject))
                    {
                        StopCoroutine(runningCoroutines[fadingObject]);
                        runningCoroutines.Remove(fadingObject);
                    }

                    Coroutine fadeIn = StartCoroutine(FadeIn(fadingObject));
                    runningCoroutines.Add(fadingObject, fadeIn);
                    toRemove.Add(fadingObject);
                }
            }

            foreach (FadingObject remove in toRemove)
            {
                objectBlockingView.Remove(remove);
            }
        }

        private IEnumerator FadeOut(FadingObject obj)
        {
            SetupMaterialForFade(obj);

            float time = 0f;
            while (obj.Materials[0].color.a > fadeAlpha)
            {
                foreach (var mat in obj.Materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = Mathf.Lerp(obj.InitialAlpha, fadeAlpha, time * fadeSpeed);
                        mat.color = c;
                    }
                }

                time += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeIn(FadingObject obj)
        {
            float time = 0f;
            while (obj.Materials[0].color.a < obj.InitialAlpha)
            {
                foreach (var mat in obj.Materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = Mathf.Lerp(fadeAlpha, obj.InitialAlpha, time * fadeSpeed);
                        mat.color = c;
                    }
                }

                time += Time.deltaTime;
                yield return null;
            }

            RestoreMaterialToOpaque(obj);
        }

        private void SetupMaterialForFade(FadingObject obj)
        {
            foreach (var mat in obj.Materials)
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        private void RestoreMaterialToOpaque(FadingObject obj)
        {
            foreach (var mat in obj.Materials)
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
        }

        private void ClearHits()
        {
            System.Array.Clear(hits, 0, hits.Length);
        }

        private FadingObject GetFadingObjectFromHit(RaycastHit hit)
        {
            return hit.collider != null ? hit.collider.GetComponent<FadingObject>() : null;
        }
    }
}
