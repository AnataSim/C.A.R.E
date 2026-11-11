using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("Daftar panel UI yang ada di dalam Canvas")]
    [SerializeField] private GameObject[] panels;

    [Header("BGM Default")]
    [Tooltip("Musik latar yang otomatis diputar saat game dimulai")]
    [SerializeField] private AudioClip defaultBGM;

    [Header("Audio Sources (Opsional / Otomatis)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Otomatis buat AudioSource jika belum ada
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        // Putar musik latar jika sudah ditentukan
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM, true);
        }

        // Aktifkan panel pertama saja saat game mulai, sembunyikan panel lainnya
        ShowPanel(0);
    }

    private void Update()
    {
        // Navigasi cepat untuk pengujian di Play Mode:
        // Tekan Panah Kanan atau Spasi untuk berpindah ke panel berikutnya
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            NextPanel();
        }
        // Tekan Panah Kiri untuk kembali ke panel sebelumnya
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPanel();
        }
    }

    /// <summary>
    /// Menampilkan panel tertentu berdasarkan index dan menyembunyikan panel lainnya.
    /// </summary>
    /// <param name="panelIndex">Nomor urut panel (0, 1, 2, dst.)</param>
    public void ShowPanel(int panelIndex)
    {
        if (panels == null || panels.Length == 0) return;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(i == panelIndex);
            }
        }
    }

    /// <summary>
    /// Berpindah ke panel berikutnya (next).
    /// </summary>
    public void NextPanel()
    {
        if (panels == null || panels.Length == 0) return;

        int currentActiveIndex = GetActivePanelIndex();
        int nextIndex = (currentActiveIndex + 1) % panels.Length;
        ShowPanel(nextIndex);
    }

    /// <summary>
    /// Berpindah ke panel sebelumnya (previous).
    /// </summary>
    public void PreviousPanel()
    {
        if (panels == null || panels.Length == 0) return;

        int currentActiveIndex = GetActivePanelIndex();
        int prevIndex = (currentActiveIndex - 1 + panels.Length) % panels.Length;
        ShowPanel(prevIndex);
    }

    /// <summary>
    /// Mendapatkan indeks panel yang sedang aktif.
    /// </summary>
    public int GetActivePanelIndex()
    {
        if (panels == null) return -1;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && panels[i].activeSelf)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Menyembunyikan semua panel yang terdaftar.
    /// </summary>
    public void HideAllPanels()
    {
        if (panels == null) return;

        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Memutar Backsound / BGM.
    /// </summary>
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// Menghentikan BGM.
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Memutar efek suara (SFX) sekali putar.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Menjeda game.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Melanjutkan game.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
