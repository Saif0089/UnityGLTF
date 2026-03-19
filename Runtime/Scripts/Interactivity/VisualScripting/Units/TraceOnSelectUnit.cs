using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Fires when the target GameObject is clicked (pointer click).
    /// Requires an EventSystem and PhysicsRaycaster in the scene.
    /// </summary>
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Events")]
    [UnitTitle("Trace: On Select")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceOnSelectUnit : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject target;
            public Delegate handler;
        }

        [DoNotSerialize]
        [PortLabel("Trigger")]
        public ControlOutput trigger { get; private set; }

        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabel("Target")]
        [PortLabelHidden]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            trigger = ControlOutput(nameof(trigger));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;

            var go = Flow.FetchValue<GameObject>(target, stack.ToReference());
            if (go == null) return;
            data.target = go;

            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);

            var reference = stack.ToReference();
            Action<PointerEventData> onClick = _ =>
            {
                using (var flow = Flow.New(reference))
                    flow.Invoke(trigger);
            };

            var hook = new EventHook(EventHooks.OnPointerClick, go);
            EventBus.Register(hook, onClick);
            data.handler = onClick;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;

            if (data.target != null)
            {
                var hook = new EventHook(EventHooks.OnPointerClick, data.target);
                EventBus.Unregister(hook, data.handler);
            }

            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) =>
            pointer.GetElementData<Data>(this).isListening;
    }
}
