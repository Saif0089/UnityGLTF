using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePagesPrevUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePagesPrevUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePagesPrevUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePagesPrevUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Pages_PrevNode>();

            if (target != null)
                node.Configuration[Pages_PrevNode.IdConfigTarget].Value = target.name;

            node.FlowIn(Pages_PrevNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Pages_PrevNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
