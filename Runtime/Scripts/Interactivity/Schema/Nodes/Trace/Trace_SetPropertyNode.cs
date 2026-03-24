namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_SetPropertyNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/setProperty";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [ConfigDescription]
        public const string IdConfigProperty = "property";

        [FlowInSocketDescription]
        public const string IdFlowIn = "in";

        [FlowOutSocketDescription]
        public const string IdFlowOut = "out";

        [InputSocketDescription()]
        public const string IdValueIn = "value";
    }
}
