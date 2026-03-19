using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePlayExternalUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePlayExternalUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePlayExternalUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePlayExternalUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_PlayExternalNode>();

            if (target != null)
                node.Configuration[Trace_PlayExternalNode.IdConfigTarget].Value = target.name;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.src, out var srcObj) && srcObj is string srcVal)
                node.Configuration[Trace_PlayExternalNode.IdConfigSrc].Value = srcVal;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.clip, out var clipObj) && clipObj is string clipVal && !string.IsNullOrEmpty(clipVal))
                node.Configuration[Trace_PlayExternalNode.IdConfigClip].Value = clipVal;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.loop, out var loopObj) && loopObj is bool loopVal)
                node.Configuration[Trace_PlayExternalNode.IdConfigLoop].Value = loopVal;

            node.ValueIn(Trace_PlayExternalNode.IdValueSpeed).MapToInputPort(unit.speed);

            node.FlowIn(Trace_PlayExternalNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_PlayExternalNode.IdFlowOut).MapToControlOutput(unit.exit);
            node.FlowOut(Trace_PlayExternalNode.IdFlowDone).MapToControlOutput(unit.done);

            return true;
        }
    }
}
