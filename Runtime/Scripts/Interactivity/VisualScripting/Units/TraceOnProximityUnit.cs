using System;
using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Fires enter/exit when the camera crosses a distance threshold to the target.
    /// Checks distance every frame.
    /// </summary>
    [UnitCategory("Trace\\Events")]
    [UnitTitle("Trace: On Proximity")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceOnProximityUnit : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public bool wasInside;
            public float currentDistance;
            public Delegate updateHandler;
        }

        [DoNotSerialize]
        [PortLabel("Enter")]
        public ControlOutput enter { get; private set; }

        [DoNotSerialize]
        [PortLabel("Exit")]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [PortLabel("Distance")]
        public ValueOutput distance { get; private set; }

        [DoNotSerialize]
        [NullMeansSelf]
        [PortLabel("Target")]
        [PortLabelHidden]
        public ValueInput target { get; private set; }

        [DoNotSerialize]
        [PortLabel("Threshold")]
        public ValueInput threshold { get; private set; }

        protected override void Definition()
        {
            enter = ControlOutput(nameof(enter));
            exit = ControlOutput(nameof(exit));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            threshold = ValueInput<float>(nameof(threshold), 5f);
            distance = ValueOutput<float>(nameof(distance));
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;

            var reference = stack.ToReference();

            Action<EmptyEventArgs> onUpdate = _ =>
            {
                UpdateProximity(reference);
            };

            var hook = new EventHook(EventHooks.Update, stack.machine);
            EventBus.Register(hook, onUpdate);

            data.updateHandler = onUpdate;
            data.wasInside = false;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;

            var hook = new EventHook(EventHooks.Update, stack.machine);
            EventBus.Unregister(hook, data.updateHandler);

            stack.ClearReference();
            data.updateHandler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }

        private void UpdateProximity(GraphReference reference)
        {
            using (var flow = Flow.New(reference))
            {
                var data = flow.stack.GetElementData<Data>(this);
                var cam = Camera.main;
                if (cam == null) return;

                var go = flow.GetValue<GameObject>(target);
                if (go == null) return;

                var dist = Vector3.Distance(cam.transform.position, go.transform.position);
                data.currentDistance = dist;

                var thresh = flow.GetValue<float>(threshold);
                var isInside = dist <= thresh;

                if (isInside && !data.wasInside)
                {
                    data.wasInside = true;
                    flow.SetValue(distance, dist);
                    flow.Invoke(enter);
                }
                else if (!isInside && data.wasInside)
                {
                    data.wasInside = false;
                    flow.SetValue(distance, dist);
                    flow.Invoke(exit);
                }
            }
        }
    }
}
