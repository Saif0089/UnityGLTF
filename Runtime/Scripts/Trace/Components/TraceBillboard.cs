using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Billboard")]
    public class TraceBillboard : MonoBehaviour
    {
        public enum BillboardMode { All, Y }
        public BillboardMode mode = BillboardMode.All;

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (mode == BillboardMode.All)
            {
                transform.LookAt(cam.transform);
                transform.Rotate(0, 180, 0);
            }
            else
            {
                var dir = cam.transform.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(-dir);
                }
            }
        }
    }
}
