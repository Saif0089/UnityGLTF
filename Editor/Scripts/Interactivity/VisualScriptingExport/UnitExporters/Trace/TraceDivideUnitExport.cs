using UnityEditor;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TraceDivideUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TraceDivideUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TraceDivideUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TraceDivideUnit;
            var node = unitExporter.CreateNode<Math_DivNode>();
            node.ValueIn(Math_DivNode.IdValueA).MapToInputPort(unit.a);
            node.ValueIn(Math_DivNode.IdValueB).MapToInputPort(unit.b);
            node.ValueOut(Math_DivNode.IdOut).MapToPort(unit.value);
            return true;
        }
    }
}
