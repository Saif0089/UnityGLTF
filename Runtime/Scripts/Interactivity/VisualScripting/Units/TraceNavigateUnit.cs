namespace Unity.VisualScripting
{
    [UnitCategory("Trace\\Actions")]
    [UnitTitle("Trace: Navigate")]
    [TypeIcon(typeof(UnityEngine.Application))]
    public class TraceNavigateUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [PortLabel("URL")]
        public ValueInput url { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                var u = flow.GetValue<string>(url);
                if (!string.IsNullOrEmpty(u))
                    UnityEngine.Application.OpenURL(u);
                return exit;
            });

            exit = ControlOutput(nameof(exit));

            url = ValueInput<string>(nameof(url), "");

            Succession(enter, exit);
            Requirement(url, enter);
        }
    }
}
