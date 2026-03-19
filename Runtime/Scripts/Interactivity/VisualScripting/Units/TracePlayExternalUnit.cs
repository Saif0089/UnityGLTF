using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Plays an external GLB animation on the target. Export-only for trace-viewer;
    /// in Unity runtime, logs a warning and fires exit/done immediately.
    /// </summary>
    [IncludeInSettings(true)]
    [UnitCategory("Trace/Animation")]
    [UnitTitle("Trace: Play External Animation")]
    [TypeIcon(typeof(Animation))]
    public class TracePlayExternalUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        [PortLabel("Done")]
        public ControlOutput done { get; private set; }

        [DoNotSerialize]
        [PortLabel("Target")]
        [PortLabelHidden]
        [NullMeansSelf]
        public ValueInput target { get; private set; }

        [DoNotSerialize]
        [PortLabel("Source GLB")]
        public ValueInput src { get; private set; }

        [DoNotSerialize]
        [PortLabel("Clip")]
        public ValueInput clip { get; private set; }

        [DoNotSerialize]
        [PortLabel("Speed")]
        public ValueInput speed { get; private set; }

        [DoNotSerialize]
        [PortLabel("Loop")]
        public ValueInput loop { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Execute);

            exit = ControlOutput(nameof(exit));
            done = ControlOutput(nameof(done));

            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            src = ValueInput<string>(nameof(src), "");
            clip = ValueInput<string>(nameof(clip), "");
            speed = ValueInput<float>(nameof(speed), 1f);
            loop = ValueInput<bool>(nameof(loop), false);

            Succession(enter, exit);
            Succession(enter, done);
            Requirement(target, enter);
            Requirement(src, enter);
        }

        private ControlOutput Execute(Flow flow)
        {
            var srcPath = flow.GetValue<string>(src);
            Debug.LogWarning($"[TracePlayExternal] External GLB playback ('{srcPath}') is not supported in Unity runtime. This node is for trace-viewer export only.");
            return exit;
        }
    }
}
