using UnityEngine;

namespace Unity.VisualScripting
{
    // ═══════════════════════════════════════════════════════════════
    // EVENT
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Events")]
    [UnitTitle("Trace: On Start")]
    [TypeIcon(typeof(GameObject))]
    public sealed class TraceOnStartUnit : Unit, IGraphEventListener, IGraphElementWithData
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public System.Delegate handler;
        }

        [DoNotSerialize] [PortLabelHidden]
        public ControlOutput trigger { get; private set; }

        protected override void Definition()
        {
            trigger = ControlOutput(nameof(trigger));
        }

        public IGraphElementData CreateData() => new Data();

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (data.isListening) return;
            var reference = stack.ToReference();
            System.Action<EmptyEventArgs> handler = _ =>
            {
                using (var flow = Flow.New(reference))
                    flow.Invoke(trigger);
            };
            var hook = new EventHook(EventHooks.Start, stack.machine);
            EventBus.Register(hook, handler);
            data.handler = handler;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);
            if (!data.isListening) return;
            var hook = new EventHook(EventHooks.Start, stack.machine);
            EventBus.Unregister(hook, data.handler);
            stack.ClearReference();
            data.handler = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer) => pointer.GetElementData<Data>(this).isListening;
    }

    // ═══════════════════════════════════════════════════════════════
    // FLOW CONTROL
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Flow")]
    [UnitTitle("Trace: Branch")]
    [TypeIcon(typeof(If))]
    public sealed class TraceBranchUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabel("True")] public ControlOutput @true { get; private set; }
        [DoNotSerialize] [PortLabel("False")] public ControlOutput @false { get; private set; }
        [DoNotSerialize] [PortLabel("Condition")] public ValueInput condition { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
                flow.GetValue<bool>(condition) ? @true : @false);
            @true = ControlOutput(nameof(@true));
            @false = ControlOutput(nameof(@false));
            condition = ValueInput<bool>(nameof(condition), false);
            Succession(enter, @true);
            Succession(enter, @false);
            Requirement(condition, enter);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Flow")]
    [UnitTitle("Trace: Sequence")]
    [TypeIcon(typeof(Sequence))]
    public sealed class TraceSequenceUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabel("0")] public ControlOutput out0 { get; private set; }
        [DoNotSerialize] [PortLabel("1")] public ControlOutput out1 { get; private set; }
        [DoNotSerialize] [PortLabel("2")] public ControlOutput out2 { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                flow.Invoke(out0);
                flow.Invoke(out1);
                flow.Invoke(out2);
                return null;
            });
            out0 = ControlOutput(nameof(out0));
            out1 = ControlOutput(nameof(out1));
            out2 = ControlOutput(nameof(out2));
            Succession(enter, out0);
            Succession(enter, out1);
            Succession(enter, out2);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // VARIABLES
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Variables")]
    [UnitTitle("Trace: Set Variable")]
    [TypeIcon(typeof(SetVariable))]
    public sealed class TraceSetVariableUnit : Unit
    {
        [DoNotSerialize] [PortLabelHidden] public ControlInput enter { get; private set; }
        [DoNotSerialize] [PortLabelHidden] public ControlOutput exit { get; private set; }
        [DoNotSerialize] public ValueInput name { get; private set; }
        [DoNotSerialize] public ValueInput value { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var varName = flow.GetValue<string>(name);
                var varValue = flow.GetValue<object>(value);
                Variables.Graph(flow.stack).Set(varName, varValue);
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            name = ValueInput<string>(nameof(name), "");
            value = ValueInput<object>(nameof(value));
            Succession(enter, exit);
            Requirement(name, enter);
            Requirement(value, enter);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Variables")]
    [UnitTitle("Trace: Get Variable")]
    [TypeIcon(typeof(GetVariable))]
    public sealed class TraceGetVariableUnit : Unit
    {
        [DoNotSerialize] public ValueInput name { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }

        protected override void Definition()
        {
            name = ValueInput<string>(nameof(name), "");
            value = ValueOutput<object>(nameof(value), flow =>
            {
                var varName = flow.GetValue<string>(name);
                return Variables.Graph(flow.stack).Get(varName);
            });
            Requirement(name, value);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // MATH / LOGIC (data nodes)
    // ═══════════════════════════════════════════════════════════════

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: NOT")]
    public sealed class TraceNotUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<bool>(nameof(a), false);
            value = ValueOutput<bool>(nameof(value), flow => !flow.GetValue<bool>(a));
            Requirement(a, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: AND")]
    public sealed class TraceAndUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<bool>(nameof(a), false);
            b = ValueInput<bool>(nameof(b), false);
            value = ValueOutput<bool>(nameof(value), flow => flow.GetValue<bool>(a) && flow.GetValue<bool>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: OR")]
    public sealed class TraceOrUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<bool>(nameof(a), false);
            b = ValueInput<bool>(nameof(b), false);
            value = ValueOutput<bool>(nameof(value), flow => flow.GetValue<bool>(a) || flow.GetValue<bool>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Equal")]
    public sealed class TraceEqualUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<bool>(nameof(value), flow => Mathf.Approximately(flow.GetValue<float>(a), flow.GetValue<float>(b)));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Greater Than")]
    public sealed class TraceGreaterThanUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<bool>(nameof(value), flow => flow.GetValue<float>(a) > flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Add")]
    public sealed class TraceAddUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<float>(nameof(value), flow => flow.GetValue<float>(a) + flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Subtract")]
    public sealed class TraceSubtractUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<float>(nameof(value), flow => flow.GetValue<float>(a) - flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Select")]
    public sealed class TraceSelectUnit : Unit
    {
        [DoNotSerialize] public ValueInput condition { get; private set; }
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            condition = ValueInput<bool>(nameof(condition), false);
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<float>(nameof(value), flow =>
                flow.GetValue<bool>(condition) ? flow.GetValue<float>(a) : flow.GetValue<float>(b));
            Requirement(condition, value);
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Multiply")]
    public sealed class TraceMultiplyUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<float>(nameof(value), flow => flow.GetValue<float>(a) * flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Divide")]
    public sealed class TraceDivideUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 1);
            value = ValueOutput<float>(nameof(value), flow =>
            {
                var bv = flow.GetValue<float>(b);
                return bv != 0 ? flow.GetValue<float>(a) / bv : 0;
            });
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Greater Equal")]
    public sealed class TraceGreaterEqualUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<bool>(nameof(value), flow => flow.GetValue<float>(a) >= flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Less Than")]
    public sealed class TraceLessThanUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] public ValueInput b { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);
            value = ValueOutput<bool>(nameof(value), flow => flow.GetValue<float>(a) < flow.GetValue<float>(b));
            Requirement(a, value);
            Requirement(b, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Combine3")]
    public sealed class TraceCombine3Unit : Unit
    {
        [DoNotSerialize] [PortLabel("X")] public ValueInput x { get; private set; }
        [DoNotSerialize] [PortLabel("Y")] public ValueInput y { get; private set; }
        [DoNotSerialize] [PortLabel("Z")] public ValueInput z { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            x = ValueInput<float>(nameof(x), 0);
            y = ValueInput<float>(nameof(y), 0);
            z = ValueInput<float>(nameof(z), 0);
            value = ValueOutput<Vector3>(nameof(value), flow =>
                new Vector3(flow.GetValue<float>(x), flow.GetValue<float>(y), flow.GetValue<float>(z)));
            Requirement(x, value);
            Requirement(y, value);
            Requirement(z, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Floor")]
    public sealed class TraceFloorUnit : Unit
    {
        [DoNotSerialize] public ValueInput a { get; private set; }
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a), 0);
            value = ValueOutput<float>(nameof(value), flow => Mathf.Floor(flow.GetValue<float>(a)));
            Requirement(a, value);
        }
    }

    [IncludeInSettings(true)]
    [UnitCategory("Trace/Math")]
    [UnitTitle("Trace: Random")]
    public sealed class TraceRandomUnit : Unit
    {
        [DoNotSerialize] [PortLabel("Value")] public ValueOutput value { get; private set; }
        protected override void Definition()
        {
            value = ValueOutput<float>(nameof(value), _ => Random.value);
        }
    }
}
