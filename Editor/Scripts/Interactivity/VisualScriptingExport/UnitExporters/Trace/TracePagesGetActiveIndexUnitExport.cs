using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePagesGetActiveIndexUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePagesGetActiveIndexUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePagesGetActiveIndexUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePagesGetActiveIndexUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Pages_GetActiveIndexNode>();

            if (target != null)
                node.Configuration[Pages_GetActiveIndexNode.IdConfigTarget].Value = target.name;

            node.ValueOut(Pages_GetActiveIndexNode.IdValueOut).MapToPort(unit.value);

            return true;
        }
    }
}
