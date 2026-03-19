namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_GetChildCountNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/getChildCount";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [OutputSocketDescription(GltfTypes.Int)]
        public const string IdValueOut = "value";
    }
}
