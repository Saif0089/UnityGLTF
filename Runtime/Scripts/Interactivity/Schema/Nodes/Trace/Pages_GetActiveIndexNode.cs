namespace UnityGLTF.Interactivity.Schema
{
    public class Pages_GetActiveIndexNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "pages/getActiveIndex";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [OutputSocketDescription(GltfTypes.Int)]
        public const string IdValueOut = "value";
    }
}
