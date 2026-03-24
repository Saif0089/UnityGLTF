namespace UnityGLTF.Interactivity.Schema
{
    public class Trace_GetPropertyNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "trace/getProperty";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [ConfigDescription]
        public const string IdConfigProperty = "property";

        [OutputSocketDescription()]
        public const string IdValueOut = "value";
    }
}
