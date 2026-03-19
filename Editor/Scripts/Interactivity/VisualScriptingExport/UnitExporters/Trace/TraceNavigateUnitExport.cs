using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceNavigateUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceNavigateUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceNavigateUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceNavigateUnit;

            var node = unitExporter.CreateNode<Trace_NavigateNode>();

            node.ValueIn(Trace_NavigateNode.IdValueUrl).MapToInputPort(unit.url);

            node.FlowIn(Trace_NavigateNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_NavigateNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
