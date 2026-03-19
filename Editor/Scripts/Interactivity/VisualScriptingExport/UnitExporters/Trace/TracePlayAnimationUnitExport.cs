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

            var animation = target.GetComponent<Animation>();
            if (!animation)
            {
                UnitExportLogging.AddErrorLog(unit, "Target GameObject does not have an Animation component.");
                return false;
            }

            // Resolve clip name
            AnimationClip clip = animation.clip;
            if (unitExporter.IsInputLiteralOrDefaultValue(unit.clip, out var clipNameObj) && clipNameObj is string clipName && !string.IsNullOrEmpty(clipName))
            {
                clip = animation.GetClip(clipName);
                if (clip == null)
                {
                    UnitExportLogging.AddErrorLog(unit, $"Animation clip '{clipName}' not found.");
                    return false;
                }
            }

            int animationId = unitExporter.vsExportContext.exporter.GetAnimationId(clip, target.transform);
            if (animationId == -1)
            {
                UnitExportLogging.AddErrorLog(unit, "Animation not found in export context.");
                return false;
            }

            var node = unitExporter.CreateNode<Animation_StartNode>();
            node.ValueInConnection[Animation_StartNode.IdValueAnimation].Value = animationId;
            node.ValueIn(Animation_StartNode.IdValueSpeed).MapToInputPort(unit.speed);

            // Resolve loop
            bool isLooping = false;
            if (unitExporter.IsInputLiteralOrDefaultValue(unit.loop, out var loopObj) && loopObj is bool loopVal)
                isLooping = loopVal;

            if (!isLooping && clip != null && !clip.isLooping && clip.wrapMode != WrapMode.Loop)
            {
                var animationLength = AnimationHelper.GetAnimationLength(unitExporter, animationId);
                node.ValueIn(Animation_StartNode.IdValueStartTime).SetValue(0f);
                node.ValueIn(Animation_StartNode.IdValueEndtime).ConnectToSource(animationLength);
            }
            else
            {
                node.ValueIn(Animation_StartNode.IdValueStartTime).SetValue(0f);
                node.ValueIn(Animation_StartNode.IdValueEndtime).SetValue(float.PositiveInfinity);
            }

            // Map flow
            node.FlowIn(Animation_StartNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Animation_StartNode.IdFlowOut).MapToControlOutput(unit.exit);
            node.FlowOut(Animation_StartNode.IdFlowDone).MapToControlOutput(unit.done);

            return true;
        }
    }
}
