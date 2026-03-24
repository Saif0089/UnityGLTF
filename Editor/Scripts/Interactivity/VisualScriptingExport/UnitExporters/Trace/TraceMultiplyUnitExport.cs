using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceMultiplyUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceMultiplyUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceMultiplyUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceMultiplyUnit;
            var node = unitExporter.CreateNode<Math_MulNode>();
            node.ValueIn(Math_MulNode.IdValueA).MapToInputPort(unit.a);
            node.ValueIn(Math_MulNode.IdValueB).MapToInputPort(unit.b);
            node.ValueOut(Math_MulNode.IdOut).MapToPort(unit.value);
            return true;
        }
    }
}
