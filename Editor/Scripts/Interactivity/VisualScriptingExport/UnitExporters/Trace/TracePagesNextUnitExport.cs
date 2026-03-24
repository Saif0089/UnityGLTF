using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePagesNextUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePagesNextUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePagesNextUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePagesNextUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Pages_NextNode>();

            if (target != null)
                node.Configuration[Pages_NextNode.IdConfigTarget].Value = target.name;

            node.FlowIn(Pages_NextNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Pages_NextNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
