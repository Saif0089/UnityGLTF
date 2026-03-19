namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_PlayVideoNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/playVideo";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [InputSocketDescription(GltfTypes.Bool)]
        public const string IdValuePlay = "play";

        [InputSocketDescription(GltfTypes.Float)]
        public const string IdValueVolume = "volume";

        [InputSocketDescription(GltfTypes.Float)]
        public const string IdValueSeek = "seek";
    }
}
