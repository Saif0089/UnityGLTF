namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnSelectNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/onSelect";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";
    }
}
