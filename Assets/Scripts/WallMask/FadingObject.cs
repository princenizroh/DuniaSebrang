using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class FadingObject : MonoBehaviour, IEquatable<FadingObject>
    {
        public List<Renderer> Renderers = new List<Renderer>();
        public List<Material> Materials = new List<Material>();
        public float InitialAlpha { get; private set; }

        private void Awake()
        {
            if (Renderers.Count == 0)
                Renderers.AddRange(GetComponentsInChildren<Renderer>());

            foreach (Renderer rend in Renderers)
                Materials.AddRange(rend.materials);

            if (Materials.Count > 0)
                InitialAlpha = Materials[0].color.a;
        }

        public bool Equals(FadingObject other)
        {
            return other != null && transform.position == other.transform.position;
        }

        public override int GetHashCode()
        {
            return transform.position.GetHashCode();
        }
    }
}
