namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_NavigateNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/navigate";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        // url is passed as a value input; trace-viewer uses a string type
        // GltfTypes doesn't have string, so we use the broadest supported type
        [InputSocketDescription()]
        public const string IdValueUrl = "url";
    }
}
