using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceSetActiveChildUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceSetActiveChildUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceSetActiveChildUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceSetActiveChildUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_SetActiveChildNode>();

            if (target != null)
                node.Configuration[Trace_SetActiveChildNode.IdConfigTarget].Value = target.name;

            node.ValueIn(Trace_SetActiveChildNode.IdValueIndex).MapToInputPort(unit.index);

            node.FlowIn(Trace_SetActiveChildNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_SetActiveChildNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
