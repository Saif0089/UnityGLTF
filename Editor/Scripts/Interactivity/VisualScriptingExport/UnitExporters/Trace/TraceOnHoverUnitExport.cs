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

            // trace-viewer uses two separate ops: event/onHoverIn and event/onHoverOut
            var inNode = unitExporter.CreateNode<Trace_OnHoverInNode>();
            inNode.FlowOut(Trace_OnHoverInNode.IdFlowOut).MapToControlOutput(unit.enter);

            var outNode = unitExporter.CreateNode<Trace_OnHoverOutNode>();
            outNode.FlowOut(Trace_OnHoverOutNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
