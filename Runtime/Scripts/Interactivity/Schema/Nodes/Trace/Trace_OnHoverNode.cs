namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnHoverNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/onHover";

        [FlowOutSocketDescription]
        public const string IdFlowEnter = "enter";

        [FlowOutSocketDescription]
        public const string IdFlowExit = "exit";
    }
}
