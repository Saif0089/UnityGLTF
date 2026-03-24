using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceGreaterEqualUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceGreaterEqualUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceGreaterEqualUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceGreaterEqualUnit;
            var node = unitExporter.CreateNode<Math_GeNode>();
            node.ValueIn(Math_GeNode.IdValueA).MapToInputPort(unit.a);
            node.ValueIn(Math_GeNode.IdValueB).MapToInputPort(unit.b);
            node.ValueOut(Math_GeNode.IdOut).MapToPort(unit.value);
            return true;
        }
    }
}
