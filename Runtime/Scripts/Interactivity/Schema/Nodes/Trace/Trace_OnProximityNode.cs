namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_OnProximityNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/onProximity";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [ConfigDescription]
        public const string IdConfigDistance = "distance";

        [FlowOutSocketDescription]
        public const string IdFlowEnter = "enter";

        [FlowOutSocketDescription]
        public const string IdFlowExit = "exit";

        [OutputSocketDescription(GltfTypes.Float)]
        public const string IdValueDistance = "distance";
    }
}
