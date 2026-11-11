using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIPanelPopup : MonoBehaviour
{
    [Header("Pengaturan Efek Pop-Up")]
    [Tooltip("Durasi animasi pop-up (detik)")]
    [SerializeField] private float duration = 0.3f;

    [Tooltip("Ukuran membal maksimum saat muncul")]
    [SerializeField] private float bounceScale = 1.1f;

    [Header("Audio Efek Suara")]
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private AudioSource audioSource;

    private Coroutine currentAnim;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Sound
        }
    }

    private void OnEnable()
    {
        // Setiap kali panel diaktifkan (misal lewat GameManager.ShowPanel)
        // Mulai animasi pop-up dan putar efek suara
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
        }
        currentAnim = StartCoroutine(AnimatePopup());

        PlaySound();
    }

    private void PlaySound()
    {
        if (soundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundEffect);
        }
    }

    private IEnumerator AnimatePopup()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;

        // Fase 1: Membesar dari 0 ke bounceScale (1.1x)
        float halfDuration = duration * 0.65f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one * bounceScale, t);
            yield return null;
        }

        // Fase 2: Kembali dari bounceScale (1.1x) ke ukuran normal (1.0x)
        elapsed = 0f;
        float returnDuration = duration - halfDuration;
        Vector3 startScale = Vector3.one * bounceScale;
        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / returnDuration;
            transform.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
        currentAnim = null;
    }
}

