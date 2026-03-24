using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePagesGetPageCountUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePagesGetPageCountUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePagesGetPageCountUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePagesGetPageCountUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Pages_GetPageCountNode>();

            if (target != null)
                node.Configuration[Pages_GetPageCountNode.IdConfigTarget].Value = target.name;

            node.ValueOut(Pages_GetPageCountNode.IdValueOut).MapToPort(unit.value);

            return true;
        }
    }
}
