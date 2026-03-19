namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_SetActiveNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/setActive";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [InputSocketDescription(GltfTypes.Bool)]
        public const string IdValueState = "state";
    }
}
