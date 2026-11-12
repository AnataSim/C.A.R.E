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
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Button continueButton;

        [Header("Portraits")]
        [SerializeField] private Sprite penjelajahPortrait;
        [SerializeField] private Sprite sawitManPortrait;

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

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }

            // Hide on start
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
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
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
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

