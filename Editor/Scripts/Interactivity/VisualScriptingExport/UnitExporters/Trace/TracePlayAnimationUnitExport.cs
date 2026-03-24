using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePlayAnimationUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePlayAnimationUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePlayAnimationUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePlayAnimationUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            if (target == null)
            {
                UnitExportLogging.AddErrorLog(unit, "Can't resolve target GameObject");
                return false;
            }

            // Resolve clip name
            string resolvedClipName = "";
            var animation = target.GetComponent<Animation>();
            if (animation != null)
            {
                AnimationClip clip = animation.clip;
                if (unitExporter.IsInputLiteralOrDefaultValue(unit.clip, out var clipNameObj) && clipNameObj is string clipName && !string.IsNullOrEmpty(clipName))
                {
                    clip = animation.GetClip(clipName);
                }
                if (clip != null)
                    resolvedClipName = clip.name;
            }

            // Create trace/playAnimation node directly (no KHR animation/start workaround)
            var node = unitExporter.CreateNode<Trace_PlayAnimationNode>();

            // Config: target name and clip name
            node.Configuration[Trace_PlayAnimationNode.IdConfigTarget] =
                new GltfInteractivityNode.ConfigData { Value = target.name };
            node.Configuration[Trace_PlayAnimationNode.IdConfigClip] =
                new GltfInteractivityNode.ConfigData { Value = resolvedClipName };

            // Value inputs: speed and loop
            node.ValueIn(Trace_PlayAnimationNode.IdValueSpeed).MapToInputPort(unit.speed);
            node.ValueIn(Trace_PlayAnimationNode.IdValueLoop).MapToInputPort(unit.loop);

            // Flow
            node.FlowIn(Trace_PlayAnimationNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_PlayAnimationNode.IdFlowOut).MapToControlOutput(unit.exit);
            node.FlowOut(Trace_PlayAnimationNode.IdFlowDone).MapToControlOutput(unit.done);

            return true;
        }
    }
}
