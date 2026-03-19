using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceGetChildCountUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceGetChildCountUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceGetChildCountUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceGetChildCountUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_GetChildCountNode>();

            if (target != null)
                node.Configuration[Trace_GetChildCountNode.IdConfigTarget].Value = target.name;

            node.ValueOut(Trace_GetChildCountNode.IdValueOut).MapToPort(unit.value);

            return true;
        }
    }
}
