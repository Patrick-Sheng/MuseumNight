using UnityEngine;

// The persistent scene owns the clips and the 2D audio channels.
public class GameAudio : MonoBehaviour
{
    public enum Cue
    {
        ChestOpen, DialogueBlip, DialogueNext, DoorOpen, FlashlightOn,
        FlashlightOff, ItemDropped, ItemPickup, Jump, LevelCompleted,
        PaintingView, StatueMove, FootLeft, FootRight, ChaserAppear
    }

    public static GameAudio Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip neutralMusic;
    [SerializeField] private AudioClip horrorMusic1;
    [SerializeField] private AudioClip horrorMusic2;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;

    [Header("Effects")]
    [SerializeField] private AudioClip chestOpen;
    [SerializeField] private AudioClip dialogueBlip;
    [SerializeField] private AudioClip dialogueNext;
    [SerializeField] private AudioClip doorOpen;
    [SerializeField, Min(0f)] private float doorOpenStartOffset = 0.8f;
    [SerializeField] private AudioClip flashlightOn;
    [SerializeField] private AudioClip flashlightOff;
    [SerializeField] private AudioClip itemDropped;
    [SerializeField] private AudioClip itemPickup;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip levelCompleted;
    [SerializeField] private AudioClip paintingView;
    [SerializeField] private AudioClip statueMove1;
    [SerializeField] private AudioClip statueMove2;
    [SerializeField] private AudioClip footLeft;
    [SerializeField] private AudioClip footRight;
    [SerializeField] private AudioClip chaserAppear;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.7f;

    private AudioSource musicSource;
    private AudioSource effectsSource;
    private AudioSource dialogueSource;
    private AudioSource doorSource;
    private bool nextStatueSound;

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = effectsVolume;

        dialogueSource = gameObject.AddComponent<AudioSource>();
        dialogueSource.playOnAwake = false;
        dialogueSource.spatialBlend = 0f;
        dialogueSource.volume = effectsVolume;

        doorSource = gameObject.AddComponent<AudioSource>();
        doorSource.playOnAwake = false;
        doorSource.spatialBlend = 0f;
        doorSource.volume = effectsVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Play(Cue cue)
    {
        if (Instance == null) return;
        AudioClip clip = Instance.GetClip(cue);
        if (clip == null) return;

        if (cue == Cue.DialogueNext || cue == Cue.DialogueBlip)
        {
            // A new dialogue click replaces the previous sound. The fallback blip
            // must not immediately replace the click when the next line starts typing.
            if (cue == Cue.DialogueBlip && Instance.dialogueSource.isPlaying) return;
            Instance.dialogueSource.Stop();
            Instance.dialogueSource.clip = clip;
            Instance.dialogueSource.Play();
            return;
        }

        if (cue == Cue.DoorOpen)
        {
            Instance.doorSource.Stop();
            Instance.doorSource.clip = clip;
            // Skip the silence at the beginning of door_open.mp3.
            Instance.doorSource.time = Mathf.Min(Instance.doorOpenStartOffset,
                Mathf.Max(0f, clip.length - 0.01f));
            Instance.doorSource.Play();
            return;
        }

        Instance.effectsSource.PlayOneShot(clip);
    }

    public static void PlayRoomMusic(string roomName)
    {
        if (Instance == null) return;
        AudioClip music = roomName switch
        {
            "Room1" => Instance.horrorMusic1,
            "Room2" => Instance.horrorMusic1,
            "Room3" => Instance.neutralMusic,
            "Room3Platformer" => Instance.neutralMusic,
            "Room4" => Instance.horrorMusic2,
            _ => null
        };
        if (Instance.musicSource.clip == music) return;
        Instance.musicSource.Stop();
        Instance.musicSource.clip = music;
        if (music != null) Instance.musicSource.Play();
    }

    public static void StopMusic()
    {
        if (Instance != null) Instance.musicSource.Stop();
    }

    private AudioClip GetClip(Cue cue)
    {
        switch (cue)
        {
            case Cue.ChestOpen: return chestOpen;
            case Cue.DialogueBlip: return dialogueBlip;
            case Cue.DialogueNext: return dialogueNext;
            case Cue.DoorOpen: return doorOpen;
            case Cue.FlashlightOn: return flashlightOn;
            case Cue.FlashlightOff: return flashlightOff;
            case Cue.ItemDropped: return itemDropped;
            case Cue.ItemPickup: return itemPickup;
            case Cue.Jump: return jump;
            case Cue.LevelCompleted: return levelCompleted;
            case Cue.PaintingView: return paintingView;
            case Cue.StatueMove:
                nextStatueSound = !nextStatueSound;
                return nextStatueSound ? statueMove1 : statueMove2;
            case Cue.FootLeft: return footLeft;
            case Cue.FootRight: return footRight;
            case Cue.ChaserAppear: return chaserAppear;
            default: return null;
        }
    }
}
