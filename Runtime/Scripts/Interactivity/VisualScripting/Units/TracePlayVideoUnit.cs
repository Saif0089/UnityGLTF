using UnityEngine;
using UnityEngine.Video;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Controls video playback on the target's VideoPlayer component.
    /// </summary>
    [UnitCategory("Trace\\Actions")]
    [UnitTitle("Trace: Play Video")]
    [TypeIcon(typeof(VideoPlayer))]
    public class TracePlayVideoUnit : Unit
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
        [PortLabel("Play")]
        public ValueInput play { get; private set; }

        [DoNotSerialize]
        [PortLabel("Volume")]
        public ValueInput volume { get; private set; }

        [DoNotSerialize]
        [PortLabel("Seek")]
        public ValueInput seek { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Execute);

            exit = ControlOutput(nameof(exit));

            target = ValueInput<GameObject>(nameof(target), null).NullMeansSelf();
            play = ValueInput<bool>(nameof(play), true);
            volume = ValueInput<float>(nameof(volume), -1f);
            seek = ValueInput<float>(nameof(seek), -1f);

            Succession(enter, exit);
            Requirement(target, enter);
        }

        private ControlOutput Execute(Flow flow)
        {
            var go = flow.GetValue<GameObject>(target);
            if (go == null) return exit;

            var videoPlayer = go.GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogWarning($"[TracePlayVideo] No VideoPlayer on {go.name}");
                return exit;
            }

            var vol = flow.GetValue<float>(volume);
            if (vol >= 0f)
            {
                videoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(vol));
            }

            var seekTime = flow.GetValue<float>(seek);
            if (seekTime >= 0f)
            {
                videoPlayer.time = seekTime;
            }

            var shouldPlay = flow.GetValue<bool>(play);
            if (shouldPlay)
                videoPlayer.Play();
            else
                videoPlayer.Pause();

            return exit;
        }
    }
}
