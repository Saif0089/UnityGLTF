using UnityEngine;

namespace Unity.VisualScripting
{
    [UnitCategory("Trace\\Actions")]
    [UnitTitle("Trace: Get Active")]
    [TypeIcon(typeof(GameObject))]
    public class TraceGetActiveUnit : Unit
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

            value = ValueOutput<bool>(nameof(value), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                return go != null && go.activeSelf;
            });

            Requirement(target, value);
        }
    }
}
