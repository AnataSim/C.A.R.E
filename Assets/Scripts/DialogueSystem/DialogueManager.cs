using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CARE.Story;

namespace CARE.UI
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Button continueButton;

        [Header("Portraits")]
        [SerializeField] private Sprite penjelajahPortrait;
        [SerializeField] private Sprite sawitManPortrait;
        [SerializeField] private Sprite monyetPortrait;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.035f;
        [SerializeField] private AudioClip typewriterSFX;
        [SerializeField] private AudioSource audioSource;

        private List<DialogueLine> currentDialogueLines = new List<DialogueLine>();
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private string fullCurrentText = "";
        private Action onCompleteCallback;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }

            // Auto-load resources if not assigned
            if (penjelajahPortrait == null) penjelajahPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/penjelajah_portrait");
            if (sawitManPortrait == null) sawitManPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man_portrait");
            if (monyetPortrait == null) monyetPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/monyet_terdampak");
            if (typewriterSFX == null) typewriterSFX = Resources.Load<AudioClip>("Audio/sfx_click");

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }

            // Hide on start
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void SetupUI(GameObject panel, Text speaker, Text body, Image portrait, Button btn, GameObject indicator = null, Image bg = null)
        {
            dialoguePanel = panel;
            speakerNameText = speaker;
            dialogueBodyText = body;
            portraitImage = portrait;
            continueButton = btn;
            continueIndicator = indicator;
            if (bg != null) backgroundImage = bg;
            else if (panel != null) backgroundImage = panel.GetComponent<Image>();

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinuePressed);
            }
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void SetBackground(Sprite bg)
        {
            if (backgroundImage != null && bg != null)
            {
                backgroundImage.sprite = bg;
                backgroundImage.color = Color.white;
            }
        }

        /// <summary>
        /// Memulai urutan dialog berdasarkan list DialogueLine.
        /// </summary>
        public void StartDialogue(List<DialogueLine> lines, Action onComplete = null)
        {
            if (lines == null || lines.Count == 0) return;

            currentDialogueLines = new List<DialogueLine>(lines);
            currentLineIndex = 0;
            onCompleteCallback = onComplete;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            if (currentLineIndex >= currentDialogueLines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = currentDialogueLines[currentLineIndex];
            fullCurrentText = line.text;

            // Set Speaker Name
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speaker;
            }

            // Set Background if specified in line
            if (!string.IsNullOrEmpty(line.backgroundKey))
            {
                Sprite bgSprite = Resources.Load<Sprite>(line.backgroundKey);
                if (bgSprite != null)
                {
                    if (backgroundImage == null && dialoguePanel != null)
                    {
                        backgroundImage = dialoguePanel.GetComponent<Image>();
                    }
                    if (backgroundImage != null)
                    {
                        backgroundImage.sprite = bgSprite;
                        backgroundImage.color = Color.white;
                    }
                }
            }

            // Set Portrait
            if (portraitImage != null)
            {
                if (line.portraitKey == "penjelajah" && penjelajahPortrait != null)
                {
                    portraitImage.gameObject.SetActive(true);
                    portraitImage.sprite = penjelajahPortrait;
                }
                else if (line.portraitKey == "sawit_man" && sawitManPortrait != null)
                {
                    portraitImage.gameObject.SetActive(true);
                    portraitImage.sprite = sawitManPortrait;
                }
                else if (line.portraitKey == "monyet" && monyetPortrait != null)
                {
                    portraitImage.gameObject.SetActive(true);
                    portraitImage.sprite = monyetPortrait;
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            // SFX Cue handling
            if (!string.IsNullOrEmpty(line.sfxCue) && CARE.Audio.CareAudioManager.Instance != null)
            {
                if (line.sfxCue.Contains("fire")) CARE.Audio.CareAudioManager.Instance.PlayFireAmbience();
                else if (line.sfxCue.Contains("nature") || line.sfxCue.Contains("morning")) CARE.Audio.CareAudioManager.Instance.PlayMorningAmbience();
                else if (line.sfxCue.Contains("bell") || line.sfxCue.Contains("item")) CARE.Audio.CareAudioManager.Instance.PlayItemFoundSFX();
            }

            if (continueIndicator != null) continueIndicator.SetActive(false);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(line.text));
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (dialogueBodyText != null)
                {
                    dialogueBodyText.text += text[i];
                }

                // Bunyikan SFX ketikan sesekali agar nyaman didengar
                if (i % 2 == 0 && typewriterSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(typewriterSFX, 0.4f);
                }

                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            isTyping = false;
            if (continueIndicator != null) continueIndicator.SetActive(true);
        }

        public void OnContinuePressed()
        {
            if (isTyping)
            {
                // Jika sedang mengetik, tampilkan seluruh teks sekaligus
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                if (dialogueBodyText != null) dialogueBodyText.text = fullCurrentText;
                isTyping = false;
                if (continueIndicator != null) continueIndicator.SetActive(true);
            }
            else
            {
                // Pindah ke kalimat dialog berikutnya
                currentLineIndex++;
                DisplayCurrentLine();
            }
        }

        private void EndDialogue()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            onCompleteCallback?.Invoke();
        }

        private void Update()
        {
            // Aksesibilitas keyboard (Spasi / Enter untuk lanjut)
            if (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnContinuePressed();
                }
            }
        }
    }
}

