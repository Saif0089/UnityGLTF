namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_PlayAnimationNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/playAnimation";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [ConfigDescription]
        public const string IdConfigClip = "clip";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [FlowOutSocketDescription]
        public const string IdFlowDone = "done";

        [InputSocketDescription(GltfTypes.Float)]
        public const string IdValueSpeed = "speed";

        [InputSocketDescription(GltfTypes.Bool)]
        public const string IdValueLoop = "loop";
    }
}
