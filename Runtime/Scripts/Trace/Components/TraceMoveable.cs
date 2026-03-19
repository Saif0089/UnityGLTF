using UnityEngine;

namespace UnityGLTF.Trace.Components
{
    [AddComponentMenu("Trace/Moveable")]
    public class TraceMoveable : MonoBehaviour
    {
        [Tooltip("Drag constraint axes: XYZ, XY, XZ, YZ, X, Y, Z")]
        public string axes = "XYZ";
    }
}
