using UnityEngine;

namespace Unity.VisualScripting
{
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Actions")]
    [UnitTitle("Trace: Set Active")]
    [TypeIcon(typeof(GameObject))]
    public class TraceSetActiveUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [PortLabel("Target")]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize]
        [PortLabel("State")]
        public ValueInput state { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                var active = flow.GetValue<bool>(state);
                if (go != null)
                    go.SetActive(active);
                return exit;
            });

            exit = ControlOutput(nameof(exit));

            target = ValueInput<GameObject>(nameof(target), null);
            target.NullMeansSelf();

            state = ValueInput<bool>(nameof(state), true);

            Succession(enter, exit);
            Requirement(target, enter);
            Requirement(state, enter);
        }
    }
}
