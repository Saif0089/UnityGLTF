using System;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Fires when the target GameObject is clicked (pointer click).
    /// Requires an EventSystem and PhysicsRaycaster in the scene.
    /// </summary>
    [UnitCategory("Trace\\Events")]
    [UnitTitle("Trace: On Select")]
    [TypeIcon(typeof(UnityEngine.GameObject))]
    public sealed class TraceOnSelectUnit : PointerEventUnit
    {
        public override Type MessageListenerType => typeof(UnityOnPointerClickMessageListener);
        protected override string hookName => EventHooks.OnPointerClick;
    }
}
