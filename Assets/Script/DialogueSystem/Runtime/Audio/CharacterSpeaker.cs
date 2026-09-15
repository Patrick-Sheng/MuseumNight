using DialogueSystem.Runtime.UI;
using DialogueSystem.Utility;
using UnityEngine;

// Modify this script so that character can repeatedly speak in a dialogue. Including a slider to control talking speed.
namespace DialogueSystem.Runtime.Audio
{
    public class CharacterSpeaker : MonoBehaviour
    {
        private enum SpeakingPlaybackMode
        {
            LoopWhileTyping,
            OneShotAtStart
        }

        [SerializeField] private TextTyper textTyper;
        [Header("Audio Sources"), SerializeField] private AudioSource speakingAudioSource;
        [SerializeField] private AudioSource reactionAudioSource;
        [Header("Speaking Playback")]
        [SerializeField] private SpeakingPlaybackMode speakingPlaybackMode = SpeakingPlaybackMode.LoopWhileTyping;
        [SerializeField, Min(0.1f)] private float speakingSpeedMultiplier = 1f;

        private float _basePitch = 1f;
        
        private void Awake()
        {
            textTyper.OnTypingStart += Speak;
            textTyper.OnTypingEnd += StopSpeaking;
        }

        private void OnDestroy()
        {
            if (textTyper == null)
            {
                return;
            }

            textTyper.OnTypingStart -= Speak;
            textTyper.OnTypingEnd -= StopSpeaking;
        }

        private void Speak()
        {
            if (speakingAudioSource == null || speakingAudioSource.clip == null)
            {
                return;
            }

            speakingAudioSource.Stop();

            switch (speakingPlaybackMode)
            {
                case SpeakingPlaybackMode.OneShotAtStart:
                    speakingAudioSource.loop = false;
                    speakingAudioSource.PlayOneShot(speakingAudioSource.clip);
                    break;
                case SpeakingPlaybackMode.LoopWhileTyping:
                    speakingAudioSource.loop = true;
                    speakingAudioSource.Play();
                    break;
            }
        }
        
        public void ChangePitch(float newPitch)
        {
            _basePitch = Mathf.Max(0.01f, newPitch);

            if (speakingAudioSource == null)
            {
                return;
            }

            speakingAudioSource.pitch = _basePitch * speakingSpeedMultiplier;
        }

        public void React(Optional<AudioClip> reactionClip)
        {
            if (!reactionClip.Enabled)
            {
                return;
            }
            reactionAudioSource.PlayOneShot(reactionClip.Value);
        }

        public void ChangeVoice(AudioClip newVoice) => speakingAudioSource.clip = newVoice;

        private void StopSpeaking()
        {
            if (speakingAudioSource == null)
            {
                return;
            }

            speakingAudioSource.Stop();
            speakingAudioSource.loop = false;
        }
    }
}