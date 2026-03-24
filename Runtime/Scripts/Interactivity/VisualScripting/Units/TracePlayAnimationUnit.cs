using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Plays an animation clip on the target. Fires 'exit' immediately and 'done' on completion.
    /// Re-triggering while playing always restarts from the beginning.
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
            public bool waitingForDone;
            public int generation;
            public int framesSincePlay;
            public string activeClip;
            public GameObject activeTarget;
            public Delegate updateHandler;
            // Transform snapshot — captured once on first play, restored on re-trigger
            public Dictionary<Transform, (Vector3 pos, Quaternion rot, Vector3 scl)> snapshot;
        }

        [DoNotSerialize] [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize] [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize] [PortLabel("Done")]
        public ControlOutput done { get; private set; }

        [DoNotSerialize] [PortLabel("Target")] [PortLabelHidden] [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize] [PortLabel("Clip")]
        public ValueInput clip { get; private set; }

        [DoNotSerialize] [PortLabel("Speed")]
        public ValueInput speed { get; private set; }

        [DoNotSerialize] [PortLabel("Loop")]
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
                var animator = go.GetComponent<Animator>();
                if (animator != null)
                {
                    var cn = flow.GetValue<string>(clip);
                    if (!string.IsNullOrEmpty(cn)) animator.Play(cn, 0, 0f);
                    else animator.Play(0, 0, 0f);
                }
                return exit;
            }

            var clipValue = flow.GetValue<string>(clip);
            var speedValue = flow.GetValue<float>(speed);
            var loopValue = flow.GetValue<bool>(loop);

            // Cancel any pending done from a previous play
            data.generation++;

            // Determine which clip to play
            string clipToPlay = !string.IsNullOrEmpty(clipValue) ? clipValue
                : (anim.clip != null ? anim.clip.name : null);

            if (clipToPlay != null)
            {
                // 1. Stop everything
                anim.Stop();

                // 2. Snapshot / restore all descendant transforms
                //    First play: save the initial state of all children
                //    Re-trigger: restore to that initial state
                if (data.snapshot == null)
                {
                    data.snapshot = new Dictionary<Transform, (Vector3 pos, Quaternion rot, Vector3 scl)>();
                    foreach (var t in go.GetComponentsInChildren<Transform>())
                        data.snapshot[t] = (t.localPosition, t.localRotation, t.localScale);
                }
                else
                {
                    foreach (var kvp in data.snapshot)
                        if (kvp.Key != null)
                        {
                            kvp.Key.localPosition = kvp.Value.pos;
                            kvp.Key.localRotation = kvp.Value.rot;
                            kvp.Key.localScale = kvp.Value.scl;
                        }
                }

                // 3. Configure and play
                var state = anim[clipToPlay];
                if (state != null)
                {
                    state.time = 0f;
                    state.speed = speedValue;
                    state.wrapMode = loopValue ? WrapMode.Loop : WrapMode.Once;
                }
                anim.Play(clipToPlay);

                data.activeClip = clipToPlay;
                data.activeTarget = go;
                data.waitingForDone = !loopValue;
                data.framesSincePlay = 0;
            }

            return exit;
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;

            var reference = stack.ToReference();
            var gen = data.generation;
            Action<EmptyEventArgs> onUpdate = _ =>
            {
                try { CheckDone(reference); }
                catch (Exception e) { Debug.LogException(e); }
            };

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
            data.waitingForDone = false;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) =>
            pointer.GetElementData<Data>(this).isListening;

        private void CheckDone(GraphReference reference)
        {
            // Access data without creating a flow first (cheaper, and avoids
            // issues with flow disposal before done invocation completes)
            Data data;
            using (var peek = Flow.New(reference))
            {
                data = peek.stack.GetElementData<Data>(this);
            }

            if (!data.waitingForDone) return;

            // Grace period — Animation.isPlaying is false on the frame Play() is called
            data.framesSincePlay++;
            if (data.framesSincePlay < 3) return;

            // Check if the animation finished
            if (data.activeTarget == null)
            {
                data.waitingForDone = false;
                return;
            }

            var anim = data.activeTarget.GetComponent<Animation>();
            if (anim == null)
            {
                data.waitingForDone = false;
                return;
            }

            // Still playing — wait
            if (anim.IsPlaying(data.activeClip)) return;

            // Animation finished — fire done
            var gen = data.generation;
            data.waitingForDone = false;

            // Create a NEW flow specifically for the done invocation
            // so it can properly chain to the next node in the sequence
            using (var flow = Flow.New(reference))
            {
                // Double-check generation hasn't changed (re-trigger during our check)
                var freshData = flow.stack.GetElementData<Data>(this);
                if (freshData.generation != gen) return;

                flow.Invoke(done);
            }
        }
    }
}
