using UnityEngine;
using System.Collections;

namespace Unity.VisualScripting
{
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Actions")]
    [UnitTitle("Trace: Get Property")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceGetPropertyUnit : Unit
    {
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }
        [DoNotSerialize] [PortLabel("Property")]
        public ValueInput property { get; private set; }
        [DoNotSerialize] [PortLabel("Value")]
        public ValueOutput value { get; private set; }

        protected override void Definition()
        {
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            property = ValueInput<string>(nameof(property), "");
            value = ValueOutput<object>(nameof(value), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                var propName = flow.GetValue<string>(property);
                if (go == null || string.IsNullOrEmpty(propName)) return null;

                var comp = go.GetComponent("TraceProperties");
                if (comp == null) return null;

                // Access properties list via reflection
                var listField = comp.GetType().GetField("properties");
                if (listField == null) return null;
                var list = listField.GetValue(comp) as IList;
                if (list == null) return null;

                foreach (var item in list)
                {
                    var nameField = item.GetType().GetField("name");
                    var valueField = item.GetType().GetField("value");
                    if (nameField != null && (string)nameField.GetValue(item) == propName)
                        return valueField?.GetValue(item);
                }
                return null;
            });
            Requirement(target, value);
            Requirement(property, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Actions")]
    [UnitTitle("Trace: Set Property")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceSetPropertyUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] [NullMeansSelf] [PortLabel("Target")] [PortLabelHidden]
        public ValueInput target { get; private set; }
        [DoNotSerialize] [PortLabel("Property")]
        public ValueInput property { get; private set; }
        [DoNotSerialize] [PortLabel("Value")]
        public ValueInput value { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var go = flow.GetValue<GameObject>(target);
                var propName = flow.GetValue<string>(property);
                var val = flow.GetValue<object>(value);
                if (go == null || string.IsNullOrEmpty(propName)) return exit;

                var comp = go.GetComponent("TraceProperties");
                if (comp == null) return exit;

                var listField = comp.GetType().GetField("properties");
                if (listField == null) return exit;
                var list = listField.GetValue(comp) as IList;
                if (list == null) return exit;

                foreach (var item in list)
                {
                    var nameField = item.GetType().GetField("name");
                    if (nameField != null && (string)nameField.GetValue(item) == propName)
                    {
                        var valueField = item.GetType().GetField("value");
                        valueField?.SetValue(item, val?.ToString() ?? "");
                        break;
                    }
                }
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            property = ValueInput<string>(nameof(property), "");
            value = ValueInput<object>(nameof(value));
            Succession(enter, exit);
            Requirement(target, enter);
            Requirement(property, enter);
        }
    }
}
