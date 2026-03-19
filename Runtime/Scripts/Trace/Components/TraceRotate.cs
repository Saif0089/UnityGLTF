using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Rotate")]
    public class TraceRotate : MonoBehaviour
    {
        public string axis = "y";
        public float speed = 30f;
        public float speedRange = 0f;

        private float _actualSpeed;

        private void Start()
        {
            _actualSpeed = speed + Random.Range(-speedRange, speedRange);
        }

        private void Update()
        {
            var axisVector = axis switch
            {
                "x" => Vector3.right,
                "y" => Vector3.up,
                "z" => Vector3.forward,
                _ => Random.onUnitSphere,
            };
            transform.Rotate(axisVector, _actualSpeed * Time.deltaTime);
        }
    }
}
