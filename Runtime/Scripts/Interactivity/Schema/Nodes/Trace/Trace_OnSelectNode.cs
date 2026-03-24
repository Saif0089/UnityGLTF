namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnSelectNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "event/onSelect";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";
    }
}
