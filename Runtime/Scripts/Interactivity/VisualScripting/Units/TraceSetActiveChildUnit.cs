using UnityEngine;

namespace Unity.VisualScripting
{
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Actions")]
    [UnitTitle("Trace: Set Active Child")]
    [TypeIcon(typeof(Transform))]
    public class TraceSetActiveChildUnit : Unit
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
        [PortLabel("Index")]
        public ValueInput index { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                var idx = flow.GetValue<int>(index);
                if (go != null)
                {
                    var t = go.transform;
                    for (int i = 0; i < t.childCount; i++)
                        t.GetChild(i).gameObject.SetActive(i == idx);
                }
                return exit;
            });

            exit = ControlOutput(nameof(exit));

            target = ValueInput<GameObject>(nameof(target), null);
            target.NullMeansSelf();

            index = ValueInput<int>(nameof(index), 0);

            Succession(enter, exit);
            Requirement(target, enter);
            Requirement(index, enter);
        }
    }
}
