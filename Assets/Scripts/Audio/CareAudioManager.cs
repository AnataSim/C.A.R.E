using UnityEngine;

namespace CARE.Audio
{
    public class CareAudioManager : MonoBehaviour
    {
        public static CareAudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Tracks (BGM)")]
        [SerializeField] private AudioClip bgmMainMenu;
        [SerializeField] private AudioClip bgmStorySad;
        [SerializeField] private AudioClip bgmRestorationHub;
        [SerializeField] private AudioClip bgmInvestigation;
        [SerializeField] private AudioClip bgmBossSawitMan;
        [SerializeField] private AudioClip bgmEnding;

        [Header("Ambience Loops")]
        [SerializeField] private AudioClip ambForestFire;
        [SerializeField] private AudioClip ambTropicalMorning;

        [Header("Sound Effects (SFX)")]
        [SerializeField] private AudioClip sfxTypewriter;
        [SerializeField] private AudioClip sfxItemFound;
        [SerializeField] private AudioClip sfxWrongClick;
        [SerializeField] private AudioClip sfxTransition;
        [SerializeField] private AudioClip sfxLevelClear;
        [SerializeField] private AudioClip sfxStageClear;
        [SerializeField] private AudioClip sfxAnimalUnlock;
        [SerializeField] private AudioClip sfxHit;
        [SerializeField] private AudioClip sfxBossDefeat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-setup AudioSources if missing
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.spatialBlend = 0f;
            }
            if (ambienceSource == null)
            {
                ambienceSource = gameObject.AddComponent<AudioSource>();
                ambienceSource.loop = true;
                ambienceSource.spatialBlend = 0f;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.spatialBlend = 0f;
            }
        }

        public void PlayBGM(AudioClip clip, float volume = 0.8f)
        {
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }

        public void PlayAmbience(AudioClip clip, float volume = 0.5f)
        {
            if (clip == null || ambienceSource == null) return;
            if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;

            ambienceSource.clip = clip;
            ambienceSource.volume = volume;
            ambienceSource.Play();
        }

        public void StopAmbience()
        {
            if (ambienceSource != null) ambienceSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        // Convenience methods based on Audio Cue Sheet
        public void PlayStorySadBGM() => PlayBGM(bgmStorySad);
        public void PlayRestorationHubBGM() => PlayBGM(bgmRestorationHub);
        public void PlayInvestigationBGM() => PlayBGM(bgmInvestigation);
        public void PlayBossBGM() => PlayBGM(bgmBossSawitMan);
        public void PlayFireAmbience() => PlayAmbience(ambForestFire);
        public void PlayMorningAmbience() => PlayAmbience(ambTropicalMorning);
        public void PlayItemFoundSFX() => PlaySFX(sfxItemFound);
        public void PlayTransitionSFX() => PlaySFX(sfxTransition);
        public void PlayLevelClearSFX() => PlaySFX(sfxLevelClear);
        public void PlayStageClearSFX() => PlaySFX(sfxStageClear);
        public void PlayAnimalUnlockSFX() => PlaySFX(sfxAnimalUnlock);
    }
}
