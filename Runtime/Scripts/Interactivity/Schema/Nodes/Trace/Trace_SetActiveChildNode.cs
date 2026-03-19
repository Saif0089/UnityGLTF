namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_SetActiveChildNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/setActiveChild";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [InputSocketDescription(GltfTypes.Int)]
        public const string IdValueIndex = "index";
    }
}
