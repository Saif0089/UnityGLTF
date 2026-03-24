using UnityEngine;

namespace Unity.VisualScripting
{
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Pages")]
    [UnitTitle("Trace: Pages Next")]
    [TypeIcon(typeof(Transform))]
    public sealed class TracePagesNextUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                if (go != null) go.SendMessage("NextPage", SendMessageOptions.DontRequireReceiver);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            Succession(enter, exit);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Pages")]
    [UnitTitle("Trace: Pages Prev")]
    [TypeIcon(typeof(Transform))]
    public sealed class TracePagesPrevUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                if (go != null) go.SendMessage("PrevPage", SendMessageOptions.DontRequireReceiver);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            Succession(enter, exit);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Pages")]
    [UnitTitle("Trace: Get Active Page")]
    [TypeIcon(typeof(Transform))]
    public sealed class TracePagesGetActiveIndexUnit : Unit
    {
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }
        [DoNotSerialize] [PortLabel("Value")]
        public ValueOutput value { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            value = ValueOutput<int>(nameof(value), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                if (go == null) return 0;
                var comp = go.GetComponent("TracePages");
                if (comp == null) return 0;
                var prop = comp.GetType().GetProperty("CurrentPage");
                return prop != null ? (int)prop.GetValue(comp) : 0;
            });
            Requirement(target, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Pages")]
    [UnitTitle("Trace: Get Page Count")]
    [TypeIcon(typeof(Transform))]
    public sealed class TracePagesGetPageCountUnit : Unit
    {
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }
        [DoNotSerialize] [PortLabel("Value")]
        public ValueOutput value { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            value = ValueOutput<int>(nameof(value), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                if (go == null) return 0;
                var comp = go.GetComponent("TracePages");
                if (comp == null) return 0;
                var prop = comp.GetType().GetProperty("PageCount");
                return prop != null ? (int)prop.GetValue(comp) : 0;
            });
            Requirement(target, value);
        }
    }
}
