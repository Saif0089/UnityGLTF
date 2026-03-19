namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_GetActiveNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/getActive";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [OutputSocketDescription(GltfTypes.Bool)]
        public const string IdValueOut = "value";
    }
}
