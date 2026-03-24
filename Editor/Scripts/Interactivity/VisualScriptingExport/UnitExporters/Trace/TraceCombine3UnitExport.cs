using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceCombine3UnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceCombine3Unit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceCombine3UnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceCombine3Unit;
            var node = unitExporter.CreateNode<Math_Combine3Node>();
            node.ValueIn("a").MapToInputPort(unit.x);
            node.ValueIn("b").MapToInputPort(unit.y);
            node.ValueIn("c").MapToInputPort(unit.z);
            node.ValueOut(Math_Combine3Node.IdOut).MapToPort(unit.value);
            return true;
        }
    }
}
