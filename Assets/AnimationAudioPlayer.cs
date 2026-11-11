using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimationAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Daftar Klip Audio (Bisa dipilih berdasarkan Index)")]
    [SerializeField] private AudioClip[] audioClips;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Memutar AudioClip secara langsung (dapat diisi via Animation Event).
    /// </summary>
    /// <param name="clip">Klip audio yang ingin diputar.</param>
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Memutar AudioClip berdasarkan nomor urut (index) dari array audioClips.
    /// Cocok dipanggil dari Animation Event dengan parameter int (misal: 0, 1, 2).
    /// </summary>
    /// <param name="index">Indeks audio clip pada daftar.</param>
    public void PlaySoundByIndex(int index)
    {
        if (audioClips != null && index >= 0 && index < audioClips.Length)
        {
            PlaySound(audioClips[index]);
        }
        else
        {
            Debug.LogWarning($"[AnimationAudioPlayer] Index {index} di luar jangkauan audioClips.");
        }
    }

    /// <summary>
    /// Menghentikan audio yang sedang diputar.
    /// </summary>
    public void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}

