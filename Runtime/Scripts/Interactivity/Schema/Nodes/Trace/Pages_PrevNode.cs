namespace UnityGLTF.Interactivity.Schema
{
    public class Pages_PrevNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "pages/prev";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [InputSocketDescription(GltfTypes.Bool)]
        public const string IdValueWrap = "wrap";

        [InputSocketDescription(GltfTypes.Bool)]
        public const string IdValueAnimate = "animate";
    }
}
