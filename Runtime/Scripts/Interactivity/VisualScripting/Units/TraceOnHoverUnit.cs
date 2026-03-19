using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Fires enter/exit when the pointer hovers over the target GameObject.
    /// Requires an EventSystem and PhysicsRaycaster in the scene.
    /// </summary>
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Events")]
    [UnitTitle("Trace: On Hover")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceOnHoverUnit : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject target;
            public Delegate enterHandler;
            public Delegate exitHandler;
        }

        [DoNotSerialize]
        [PortLabel("Enter")]
        public ControlOutput enter { get; private set; }

        [DoNotSerialize]
        [PortLabel("Exit")]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabel("Target")]
        [PortLabelHidden]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            enter = ControlOutput(nameof(enter));
            exit = ControlOutput(nameof(exit));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;

            var go = Flow.FetchValue<GameObject>(target, stack.ToReference());
            if (go == null) return;

            data.target = go;

            // Ensure message listeners are on the target
            if (UnityThread.allowsAPI)
            {
                MessageListener.AddTo(typeof(UnityOnPointerEnterMessageListener), go);
                MessageListener.AddTo(typeof(UnityOnPointerExitMessageListener), go);
            }

            var reference = stack.ToReference();

            Action<PointerEventData> onEnter = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    flow.Invoke(enter);
                }
            };

            Action<PointerEventData> onExit = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    flow.Invoke(exit);
                }
            };

            var enterHook = new EventHook(EventHooks.OnPointerEnter, go);
            var exitHook = new EventHook(EventHooks.OnPointerExit, go);

            EventBus.Register(enterHook, onEnter);
            EventBus.Register(exitHook, onExit);

            data.enterHandler = onEnter;
            data.exitHandler = onExit;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;

            if (data.target != null)
            {
                var enterHook = new EventHook(EventHooks.OnPointerEnter, data.target);
                var exitHook = new EventHook(EventHooks.OnPointerExit, data.target);

                EventBus.Unregister(enterHook, data.enterHandler);
                EventBus.Unregister(exitHook, data.exitHandler);
            }

            stack.ClearReference();
            data.enterHandler = null;
            data.exitHandler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }
    }
}
