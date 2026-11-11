using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [Header("Audio Efek Suara Klik")]
    [Tooltip("Klip audio yang diputar saat tombol ini diklik (default: chutter-click)")]
    [SerializeField] private AudioClip clickSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (clickSound != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlaySFX(clickSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clickSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }
        }
    }
}

