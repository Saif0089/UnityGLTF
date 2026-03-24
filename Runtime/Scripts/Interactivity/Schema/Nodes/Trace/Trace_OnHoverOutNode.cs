namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnHoverOutNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "event/onHoverOut";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";
    }
}
