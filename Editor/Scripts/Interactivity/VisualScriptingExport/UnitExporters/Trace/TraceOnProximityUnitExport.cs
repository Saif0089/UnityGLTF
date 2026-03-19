using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceOnProximityUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceOnProximityUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceOnProximityUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceOnProximityUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_OnProximityNode>();

            if (target != null)
                node.Configuration[Trace_OnProximityNode.IdConfigTarget].Value = target.name;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.threshold, out var thresholdObj) && thresholdObj is float thresholdVal)
                node.Configuration[Trace_OnProximityNode.IdConfigDistance].Value = thresholdVal;

            node.FlowOut(Trace_OnProximityNode.IdFlowEnter).MapToControlOutput(unit.enter);
            node.FlowOut(Trace_OnProximityNode.IdFlowExit).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
