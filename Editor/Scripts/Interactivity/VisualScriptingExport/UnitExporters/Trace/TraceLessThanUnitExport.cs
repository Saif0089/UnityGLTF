using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceLessThanUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceLessThanUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceLessThanUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceLessThanUnit;
            var node = unitExporter.CreateNode<Math_LtNode>();
            node.ValueIn(Math_LtNode.IdValueA).MapToInputPort(unit.a);
            node.ValueIn(Math_LtNode.IdValueB).MapToInputPort(unit.b);
            node.ValueOut(Math_LtNode.IdOut).MapToPort(unit.value);
            return true;
        }
    }
}
