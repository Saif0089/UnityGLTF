using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceSetPropertyUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceSetPropertyUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceSetPropertyUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceSetPropertyUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_SetPropertyNode>();

            if (target != null)
                node.Configuration[Trace_SetPropertyNode.IdConfigTarget].Value = target.name;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.property, out var propObj) && propObj is string propName)
                node.Configuration[Trace_SetPropertyNode.IdConfigProperty].Value = propName;

            node.ValueIn(Trace_SetPropertyNode.IdValueIn).MapToInputPort(unit.value);
            node.FlowIn(Trace_SetPropertyNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_SetPropertyNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
