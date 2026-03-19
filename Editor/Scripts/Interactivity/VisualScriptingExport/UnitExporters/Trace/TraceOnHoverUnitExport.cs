using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceOnHoverUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceOnHoverUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceOnHoverUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceOnHoverUnit;

            var node = unitExporter.CreateNode<Trace_OnHoverNode>();
            node.FlowOut(Trace_OnHoverNode.IdFlowEnter).MapToControlOutput(unit.enter);
            node.FlowOut(Trace_OnHoverNode.IdFlowExit).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
