using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

namespace UnityGLTF.Trace
{
    [InitializeOnLoad]
    public static class TraceNodeRegistration
    {
        private static readonly Type[] TraceUnitTypes = new Type[]
        {
            typeof(TraceOnSelectUnit),
            typeof(TraceOnHoverUnit),
            typeof(TraceOnProximityUnit),
            typeof(TraceOnStartUnit),
            typeof(TraceSetActiveUnit),
            typeof(TraceGetActiveUnit),
            typeof(TraceSetActiveChildUnit),
            typeof(TraceGetChildCountUnit),
            typeof(TracePlayAnimationUnit),
            typeof(TracePlayExternalUnit),
            typeof(TracePlayVideoUnit),
            typeof(TraceNavigateUnit),
            typeof(TraceBranchUnit),
            typeof(TraceSequenceUnit),
            typeof(TraceSetVariableUnit),
            typeof(TraceGetVariableUnit),
            typeof(TraceNotUnit),
            typeof(TraceAndUnit),
            typeof(TraceOrUnit),
            typeof(TraceEqualUnit),
            typeof(TraceGreaterThanUnit),
            typeof(TraceAddUnit),
            typeof(TraceSubtractUnit),
            typeof(TraceSelectUnit),
            typeof(TraceFloorUnit),
            typeof(TraceRandomUnit),
            typeof(TraceMultiplyUnit),
            typeof(TraceDivideUnit),
            typeof(TraceGreaterEqualUnit),
            typeof(TraceLessThanUnit),
            typeof(TraceCombine3Unit),
            // Property ops
            typeof(TraceGetPropertyUnit),
            typeof(TraceSetPropertyUnit),
            // Pages ops
            typeof(TracePagesNextUnit),
            typeof(TracePagesPrevUnit),
            typeof(TracePagesGetActiveIndexUnit),
            typeof(TracePagesGetPageCountUnit),
            // Templates
            typeof(TraceToggleVisibilityTemplate),
            typeof(TraceShowOnStartTemplate),
            typeof(TraceHideOnStartTemplate),
            typeof(TraceShowAOrBTemplate),
            typeof(TraceShowMultipleTemplate),
            typeof(TraceRandomChildTemplate),
            typeof(TracePlayVideoTemplate),
            typeof(TracePlayAnimationTemplate),
            typeof(TraceAnimationSequenceTemplate),
        };

        static TraceNodeRegistration()
        {
            EditorApplication.delayCall += EnsureRegistered;
        }

        [MenuItem("Tools/Trace/Force Register All Trace Nodes")]
        public static void ForceRegister()
        {
            EnsureRegistered();
            UnitBase.Rebuild();
            Debug.Log("[Trace] All Trace nodes force-registered and unit database rebuilt.");
        }

        private static void EnsureRegistered()
        {
            try
            {
                var config = BoltCore.Configuration;
                var types = config.typeOptions;
                bool changed = false;

                foreach (var t in TraceUnitTypes)
                {
                    if (!types.Contains(t))
                    {
                        types.Add(t);
                        changed = true;
                    }
                }

                if (changed)
                {
                    config.Save();
                    Codebase.UpdateSettings();
                }
            }
            catch (Exception e)
            {
                // VS may not be fully initialized during first domain reload
                if (!(e is InvalidOperationException))
                    Debug.LogWarning($"[Trace] Node registration deferred: {e.Message}");
            }
        }
    }
}
