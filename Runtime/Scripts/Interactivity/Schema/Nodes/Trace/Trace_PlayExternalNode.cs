namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_PlayExternalNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/playExternal";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [ConfigDescription]
        public const string IdConfigSrc = "src";

        [ConfigDescription]
        public const string IdConfigClip = "clip";

        [ConfigDescription]
        public const string IdConfigLoop = "loop";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [FlowOutSocketDescription]
        public const string IdFlowDone = "done";

        [InputSocketDescription(GltfTypes.Float)]
        public const string IdValueSpeed = "speed";
    }
}
