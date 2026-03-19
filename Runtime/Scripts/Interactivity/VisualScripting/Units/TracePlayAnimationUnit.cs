using System;
using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Plays an animation clip on the target. Fires 'exit' immediately and 'done' on completion.
    /// Re-triggering while playing cancels the previous done callback.
    /// </summary>
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Animation")]
    [UnitTitle("Trace: Play Animation")]
    [TypeIcon(typeof(Animation))]
    public sealed class TracePlayAnimationUnit : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public bool running;
            public int generation;
            public Delegate updateHandler;
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [PortLabel("Done")]
        public ControlOutput done { get; private set; }

        [DoNotSerialize]
        [PortLabel("Target")]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize]
        [PortLabel("Clip")]
        public ValueInput clip { get; private set; }

        [DoNotSerialize]
        [PortLabel("Speed")]
        public ValueInput speed { get; private set; }

        [DoNotSerialize]
        [PortLabel("Loop")]
        public ValueInput loop { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Execute);

            exit = ControlOutput(nameof(exit));
            done = ControlOutput(nameof(done));

            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            clip = ValueInput<string>(nameof(clip), "");
            speed = ValueInput<float>(nameof(speed), 1f);
            loop = ValueInput<bool>(nameof(loop), false);

            Succession(enter, exit);
            Succession(enter, done);
            Requirement(target, enter);
        }

        private ControlOutput Execute(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            var go = flow.GetValue<GameObject>(target);
            if (go == null) return exit;

            var anim = go.GetComponent<Animation>();
            if (anim == null)
            {
                // Try Animator fallback
                var animator = go.GetComponent<Animator>();
                if (animator != null)
                {
                    var clipName = flow.GetValue<string>(clip);
                    if (!string.IsNullOrEmpty(clipName))
                        animator.Play(clipName);
                    else
                        animator.Play(0);
                }
                return exit;
            }

            var clipValue = flow.GetValue<string>(clip);
            var speedValue = flow.GetValue<float>(speed);
            var loopValue = flow.GetValue<bool>(loop);

            // Invalidate any previous done
            data.generation++;
            var gen = data.generation;

            if (!string.IsNullOrEmpty(clipValue))
            {
                anim.Play(clipValue);
                var state = anim[clipValue];
                if (state != null)
                {
                    state.speed = speedValue;
                    state.wrapMode = loopValue ? WrapMode.Loop : WrapMode.Once;
                }
            }
            else if (anim.clip != null)
            {
                anim.Play();
                var state = anim[anim.clip.name];
                if (state != null)
                {
                    state.speed = speedValue;
                    state.wrapMode = loopValue ? WrapMode.Loop : WrapMode.Once;
                }
            }

            data.running = !loopValue;
            return exit;
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
            Action<EmptyEventArgs> onUpdate = _ => CheckDone(reference);

            var hook = new EventHook(EventHooks.Update, stack.machine);
            EventBus.Register(hook, onUpdate);

            data.updateHandler = onUpdate;
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
            data.running = false;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }

        private void CheckDone(GraphReference reference)
        {
            using (var flow = Flow.New(reference))
            {
                var data = flow.stack.GetElementData<Data>(this);
                if (!data.running) return;

                var go = flow.GetValue<GameObject>(target);
                if (go == null) { data.running = false; return; }

                var anim = go.GetComponent<Animation>();
                if (anim == null || !anim.isPlaying)
                {
                    data.running = false;
                    flow.Invoke(done);
                }
            }
        }
    }
}
