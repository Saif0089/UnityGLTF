using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceSetActiveUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceSetActiveUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceSetActiveUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceSetActiveUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_SetActiveNode>();

            if (target != null)
                node.Configuration[Trace_SetActiveNode.IdConfigTarget].Value = target.name;

            node.ValueIn(Trace_SetActiveNode.IdValueState).MapToInputPort(unit.state);

            node.FlowIn(Trace_SetActiveNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_SetActiveNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
