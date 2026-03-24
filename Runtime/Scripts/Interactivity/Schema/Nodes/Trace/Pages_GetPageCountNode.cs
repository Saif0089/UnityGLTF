namespace UnityGLTF.Interactivity.Schema
{
    public class Pages_GetPageCountNode : GltfInteractivityNodeSchema
    {
        public override string Op { get; set; } = "pages/getPageCount";

        [ConfigDescription]
        public const string IdConfigTarget = "target";

        [OutputSocketDescription(GltfTypes.Int)]
        public const string IdValueOut = "value";
    }
}
