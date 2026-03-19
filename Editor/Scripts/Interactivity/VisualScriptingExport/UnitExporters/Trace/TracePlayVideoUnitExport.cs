using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using UnityGLTF.Interactivity.Schema;

namespace UnityGLTF.Interactivity.VisualScripting.Export
{
    internal class TracePlayVideoUnitExport : IUnitExporter
    {
        public System.Type unitType => typeof(TracePlayVideoUnit);

        [InitializeOnLoadMethod]
        private static void Register()
        {
            UnitExporterRegistry.RegisterExporter(new TracePlayVideoUnitExport());
        }

        public bool InitializeInteractivityNodes(UnitExporter unitExporter)
        {
            var unit = unitExporter.unit as TracePlayVideoUnit;

            GameObject target = UnitsHelper.GetGameObjectFromValueInput(
                unit.target, unit.defaultValues, unitExporter.vsExportContext);

            var node = unitExporter.CreateNode<Trace_PlayVideoNode>();

            if (target != null)
                node.Configuration[Trace_PlayVideoNode.IdConfigTarget].Value = target.name;

            node.ValueIn(Trace_PlayVideoNode.IdValuePlay).MapToInputPort(unit.play);
            node.ValueIn(Trace_PlayVideoNode.IdValueVolume).MapToInputPort(unit.volume);
            node.ValueIn(Trace_PlayVideoNode.IdValueSeek).MapToInputPort(unit.seek);

            node.FlowIn(Trace_PlayVideoNode.IdFlowIn).MapToControlInput(unit.enter);
            node.FlowOut(Trace_PlayVideoNode.IdFlowOut).MapToControlOutput(unit.exit);

            return true;
        }
    }
}
