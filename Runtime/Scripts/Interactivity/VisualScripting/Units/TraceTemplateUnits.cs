using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting
{
    // ═══════════════════════════════════════════════════════════════
    // 1. Toggle Visibility
    //    Click this object to toggle another object on/off
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Toggle Visibility")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceToggleVisibilityTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Target")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    if (t != null) t.SetActive(!t.activeSelf);
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 2. Show on Start
    //    Make an object visible when the scene starts
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Show on Start")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceShowOnStartTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Target")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var reference = stack.ToReference();
            Action<EmptyEventArgs> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    if (t != null) t.SetActive(true);
                }
            };
            EventBus.Register(new EventHook(EventHooks.Start, stack.machine), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            EventBus.Unregister(new EventHook(EventHooks.Start, stack.machine), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 3. Hide on Start
    //    Hide an object when the scene starts
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Hide on Start")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceHideOnStartTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Target")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var reference = stack.ToReference();
            Action<EmptyEventArgs> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    if (t != null) t.SetActive(false);
                }
            };
            EventBus.Register(new EventHook(EventHooks.Start, stack.machine), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            EventBus.Unregister(new EventHook(EventHooks.Start, stack.machine), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 4. Show A or B
    //    Click to show A if active, or B if inactive
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Show A or B")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceShowAOrBTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("A")] public ValueInput a { get; private set; }
        [DoNotSerialize] [PortLabel("B")] public ValueInput b { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<GameObject>(nameof(a), null);
            b = ValueInput<GameObject>(nameof(b), null);
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var objA = flow.GetValue<GameObject>(a);
                    var objB = flow.GetValue<GameObject>(b);
                    if (objA != null && objB != null)
                    {
                        bool aActive = objA.activeSelf;
                        objA.SetActive(!aActive);
                        objB.SetActive(aActive);
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 5. Show Multiple
    //    Click to show two objects at once
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Show Multiple")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceShowMultipleTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Object A")] public ValueInput a { get; private set; }
        [DoNotSerialize] [PortLabel("Object B")] public ValueInput b { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<GameObject>(nameof(a), null);
            b = ValueInput<GameObject>(nameof(b), null);
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var objA = flow.GetValue<GameObject>(a);
                    var objB = flow.GetValue<GameObject>(b);
                    if (objA != null) objA.SetActive(true);
                    if (objB != null) objB.SetActive(true);
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 6. Random Child
    //    Click to activate a random child (hides siblings)
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Random Child")]
    [TypeIcon(typeof(Transform))]
    public sealed class TraceRandomChildTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Parent")] [NullMeansSelf]
        public ValueInput parent { get; private set; }

        protected override void Definition()
        {
            parent = ValueInput<GameObject>(nameof(parent), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var p = flow.GetValue<GameObject>(parent);
                    if (p != null && p.transform.childCount > 0)
                    {
                        int idx = UnityEngine.Random.Range(0, p.transform.childCount);
                        for (int i = 0; i < p.transform.childCount; i++)
                            p.transform.GetChild(i).gameObject.SetActive(i == idx);
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 7. Play Video
    //    Click to play/resume a video on an object
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Play Video (Template)")]
    [TypeIcon(typeof(UnityEngine.Video.VideoPlayer))]
    public sealed class TracePlayVideoTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Video Object")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    if (t != null)
                    {
                        var vp = t.GetComponent<UnityEngine.Video.VideoPlayer>();
                        if (vp != null) vp.Play();
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 8. Play Animation (Template)
    //    Click to play an animation clip on an object
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Play Animation (Template)")]
    [TypeIcon(typeof(Animation))]
    public sealed class TracePlayAnimationTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public GameObject clickTarget;
            public Delegate handler;
        }

        [DoNotSerialize] [PortLabel("Target")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize] [PortLabel("Clip")]
        public ValueInput clip { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            clip = ValueInput<string>(nameof(clip), "");
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();
            Action<PointerEventData> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    var clipName = flow.GetValue<string>(clip);
                    if (t != null)
                    {
                        var anim = t.GetComponent<Animation>();
                        if (anim != null)
                        {
                            if (!string.IsNullOrEmpty(clipName))
                                anim.Play(clipName);
                            else
                                anim.Play();
                        }
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // 9. Animation Sequence
    //    Click to play two animations in sequence (A finishes, then B)
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Templates")]
    [UnitTitle("Animation Sequence")]
    [TypeIcon(typeof(Animation))]
    public sealed class TraceAnimationSequenceTemplate : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public bool waitingForA;
            public GameObject clickTarget;
            public Delegate clickHandler;
            public Delegate updateHandler;
        }

        [DoNotSerialize] [PortLabel("Target")] [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize] [PortLabel("Clip A")]
        public ValueInput clipA { get; private set; }

        [DoNotSerialize] [PortLabel("Clip B")]
        public ValueInput clipB { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            clipA = ValueInput<string>(nameof(clipA), "");
            clipB = ValueInput<string>(nameof(clipB), "");
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var go = ((Component)stack.machine).gameObject;
            data.clickTarget = go;
            if (UnityThread.allowsAPI)
                MessageListener.AddTo(typeof(UnityOnPointerClickMessageListener), go);
            var reference = stack.ToReference();

            // Click handler: play clip A
            Action<PointerEventData> clickHandler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var t = flow.GetValue<GameObject>(target);
                    var nameA = flow.GetValue<string>(clipA);
                    if (t != null)
                    {
                        var anim = t.GetComponent<Animation>();
                        if (anim != null && !string.IsNullOrEmpty(nameA))
                        {
                            anim.Play(nameA);
                            var d = flow.stack.GetElementData<Data>(this);
                            d.waitingForA = true;
                        }
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.OnPointerClick, go), clickHandler);
            data.clickHandler = clickHandler;

            // Update handler: check if A finished, then play B
            Action<EmptyEventArgs> updateHandler = _ =>
            {
                using (var flow = Flow.New(reference))
                {
                    var d = flow.stack.GetElementData<Data>(this);
                    if (!d.waitingForA) return;

                    var t = flow.GetValue<GameObject>(target);
                    if (t == null) { d.waitingForA = false; return; }

                    var anim = t.GetComponent<Animation>();
                    if (anim == null || !anim.isPlaying)
                    {
                        d.waitingForA = false;
                        var nameB = flow.GetValue<string>(clipB);
                        if (anim != null && !string.IsNullOrEmpty(nameB))
                            anim.Play(nameB);
                    }
                }
            };
            EventBus.Register(new EventHook(EventHooks.Update, stack.machine), updateHandler);
            data.updateHandler = updateHandler;

            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            if (data.clickTarget != null)
                EventBus.Unregister(new EventHook(EventHooks.OnPointerClick, data.clickTarget), data.clickHandler);
            EventBus.Unregister(new EventHook(EventHooks.Update, stack.machine), data.updateHandler);
            stack.ClearReference();
            data.clickHandler = null;
            data.updateHandler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }
}
