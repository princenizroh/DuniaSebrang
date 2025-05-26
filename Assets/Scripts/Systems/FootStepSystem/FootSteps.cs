
using UnityEngine;

namespace DS
{
    public class FootSteps : MonoBehaviour
    {
        [SerializeField] ParticleSystem footStepParticle;
        public float delta = 0.7f;
        Vector3 lastEmit;
        [SerializeField] float gap = 0.1f;
        [SerializeField] int dir = 1;
        void Start()
        {
            lastEmit = transform.position;
        }
        private void Update()
        {
            if (Vector3.Distance(lastEmit, transform.position) > delta)
            {
                Gizmos.color = Color.green;
                var pos = transform.position + (transform.right * gap * dir);
                dir *= -1;
                ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
                ep.position = pos;
                ep.rotation = transform.rotation.eulerAngles.y;
                footStepParticle.Emit(ep, 1);
                lastEmit = transform.position;
            }
        }
    }
}
