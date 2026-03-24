namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnProximityNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/onProximity";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [FlowOutSocketDescription]
        public const string IdFlowEnter = "enter";

        [FlowOutSocketDescription]
        public const string IdFlowExit = "exit";

        [InputSocketDescription(GltfTypes.Float)]
        public const string IdValueThreshold = "threshold";

        [OutputSocketDescription(GltfTypes.Float)]
        public const string IdValueDistance = "distance";
    }
}
