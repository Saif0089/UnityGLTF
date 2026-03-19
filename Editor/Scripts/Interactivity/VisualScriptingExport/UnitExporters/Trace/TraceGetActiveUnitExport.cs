using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceGetActiveUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceGetActiveUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceGetActiveUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceGetActiveUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_GetActiveNode>();

            if (target != null)
                node.Configuration[Trace_GetActiveNode.IdConfigTarget].Value = target.name;

            node.ValueOut(Trace_GetActiveNode.IdValueOut).MapToPort(unit.value);

            return true;
        }
    }
}
