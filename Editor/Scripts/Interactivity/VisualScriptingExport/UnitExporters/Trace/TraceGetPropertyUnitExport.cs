using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceGetPropertyUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceGetPropertyUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceGetPropertyUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceGetPropertyUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_GetPropertyNode>();

            if (target != null)
                node.Configuration[Trace_GetPropertyNode.IdConfigTarget].Value = target.name;

            if (unitExporter.IsInputLiteralOrDefaultValue(unit.property, out var propObj) && propObj is string propName)
                node.Configuration[Trace_GetPropertyNode.IdConfigProperty].Value = propName;

            node.ValueOut(Trace_GetPropertyNode.IdValueOut).MapToPort(unit.value);

            return true;
        }
    }
}
