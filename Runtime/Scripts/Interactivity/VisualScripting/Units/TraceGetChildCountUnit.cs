using UnityEngine;

namespace Unity.VisualScripting
{
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Actions")]
    [UnitTitle("Trace: Get Child Count")]
    [TypeIcon(typeof(Transform))]
    public class TraceGetChildCountUnit : Unit
    {
        [DoNotSerialize]
        [PortLabel("Target")]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize]
        [PortLabel("Value")]
        public ValueOutput value { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null);
            target.NullMeansSelf();

            value = ValueOutput<int>(nameof(value), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                return go != null ? go.transform.childCount : 0;
            });

            Requirement(target, value);
        }
    }
}
