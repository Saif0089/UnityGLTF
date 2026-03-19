using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceOnSelectUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceOnSelectUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceOnSelectUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceOnSelectUnit;

            var node = unitExporter.CreateNode<Trace_OnSelectNode>();
            node.FlowOut(Trace_OnSelectNode.IdFlowOut).MapToControlOutput(unit.trigger);

            return true;
        }
    }
}
