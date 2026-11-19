using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CARE.Story;
using CARE.Core;
using CARE.Gameplay;

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
        [SerializeField] private Button skipButton;

        [Header("Portraits")]
        [SerializeField] private Sprite penjelajahPortrait;
        [SerializeField] private Sprite sawitManPortrait;
        [SerializeField] private Sprite monyetPortrait;
        [SerializeField] private Sprite harimauPortrait;

        [Header("Speech Bubble & Avatar Layout")]
        [SerializeField] private RectTransform cardBorderRT;
        [SerializeField] private Image cardBorderImage;
        [SerializeField] private Image cardFillImage;
        [SerializeField] private Image headAvatarImage;
        [SerializeField] private Image speechTailImage;

        [Header("Scene Stage Actors")]
        [SerializeField] private Image stageActorMC;
        [SerializeField] private Image stageActorAnimal;
        [SerializeField] private Image stageActorAnimal2;

        [Header("Avatar Heads")]
        [SerializeField] private Sprite penjelajahHead;
        [SerializeField] private Sprite sawitManHead;
        [SerializeField] private Sprite monyetHead;
        [SerializeField] private Sprite harimauHead;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.035f;
        [SerializeField] private AudioClip typewriterSFX;
        [SerializeField] private AudioSource audioSource;

        [Header("Character Voice / Speech Tones")]
        [SerializeField] private AudioClip narratorMcTone;
        [SerializeField] private AudioClip drSawitNormalTone;
        [SerializeField] private AudioClip drSawitAngryTone;
        [SerializeField] private AudioSource voiceAudioSource;

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

            // Audio source for clicks / SFX
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }

            // Audio source dedicated to character voice tones
            if (voiceAudioSource == null)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length > 1)
                {
                    voiceAudioSource = sources[1];
                }
                else
                {
                    voiceAudioSource = gameObject.AddComponent<AudioSource>();
                }
                voiceAudioSource.playOnAwake = false;
                voiceAudioSource.spatialBlend = 0f;
                voiceAudioSource.loop = true;
            }

            // Auto-load portraits if not assigned
            if (penjelajahPortrait == null) penjelajahPortrait = Resources.Load<Sprite>("Character/Karakter_Utama");
            if (penjelajahPortrait == null) penjelajahPortrait = Resources.Load<Sprite>("Character/penjelajah");
            if (penjelajahPortrait == null) penjelajahPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/penjelajah_portrait");

            if (sawitManPortrait == null) sawitManPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man_portrait");

            if (monyetPortrait == null) monyetPortrait = Resources.Load<Sprite>("Animalia/MonyetKesakitan");
            if (monyetPortrait == null) monyetPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/monyet_terdampak");

            if (harimauPortrait == null) harimauPortrait = Resources.Load<Sprite>("Animalia/HarimauSumateraKesakitan");
            if (harimauPortrait == null) harimauPortrait = Resources.Load<Sprite>("TemporaryAssets/Characters/harimau_terdampak");

            // Auto-load avatar head sprites ("ada kepala gitu")
            if (penjelajahHead == null) penjelajahHead = Resources.Load<Sprite>("Character/mc_head");
            if (penjelajahHead == null) penjelajahHead = Resources.Load<Sprite>("TemporaryAssets/Characters/mc_head");
            if (sawitManHead == null) sawitManHead = Resources.Load<Sprite>("TemporaryAssets/Characters/sawit_man_head");
            if (monyetHead == null) monyetHead = Resources.Load<Sprite>("Animalia/monyet_head");
            if (harimauHead == null) harimauHead = Resources.Load<Sprite>("Animalia/harimau_head");
            if (typewriterSFX == null) typewriterSFX = Resources.Load<AudioClip>("Audio/sfx_click");

            // Auto-load voice speech tone clips from Resources/Audio/tone/
            if (narratorMcTone == null)
            {
                narratorMcTone = Resources.Load<AudioClip>("Audio/tone/narrator_mc_tone");
                if (narratorMcTone == null) narratorMcTone = Resources.Load<AudioClip>("Audio/tone/Narrator + MC Tone");
            }
            if (drSawitNormalTone == null)
            {
                drSawitNormalTone = Resources.Load<AudioClip>("Audio/tone/dr_sawit_normal_tone");
                if (drSawitNormalTone == null) drSawitNormalTone = Resources.Load<AudioClip>("Audio/tone/Normal Tone - Dr Sawit man");
            }
            if (drSawitAngryTone == null)
            {
                drSawitAngryTone = Resources.Load<AudioClip>("Audio/tone/dr_sawit_angry_tone");
                if (drSawitAngryTone == null) drSawitAngryTone = Resources.Load<AudioClip>("Audio/tone/Angry Tone - Dr Sawit Man");
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }

            // Hide on start
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void SetupUI(GameObject panel, Text speaker, Text body, Image portrait, Button btn, GameObject indicator = null, Image bg = null, Button skipBtn = null)
        {
            dialoguePanel = panel;
            speakerNameText = speaker;
            dialogueBodyText = body;
            portraitImage = portrait;
            continueButton = btn;
            continueIndicator = indicator;
            skipButton = skipBtn;
            if (bg != null) backgroundImage = bg;
            else if (panel != null) backgroundImage = panel.GetComponent<Image>();

            // Terapkan font Lilita One
            Font f = Match3CombatManager.GetGameFont();
            if (f != null)
            {
                if (speakerNameText != null) speakerNameText.font = f;
                if (dialogueBodyText != null) dialogueBodyText.font = f;
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinuePressed);
            }
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(SkipDialogue);
            }
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void SkipDialogue()
        {
            Debug.Log("[DialogueManager] SkipDialogue dipanggil oleh pemain. Mengakhiri seluruh narasi.");
            EndDialogue();
        }

        public void SetupSpeechBubbleUI(
            RectTransform cardRT,
            Image cardBorder,
            Image cardFill,
            Image headImg,
            Image tailImg,
            Image actorMC,
            Image actorAnimal,
            Image actorAnimal2)
        {
            cardBorderRT = cardRT;
            cardBorderImage = cardBorder;
            cardFillImage = cardFill;
            headAvatarImage = headImg;
            speechTailImage = tailImg;
            stageActorMC = actorMC;
            stageActorAnimal = actorAnimal;
            stageActorAnimal2 = actorAnimal2;
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
                if (speakerNameText.transform.parent != null && speakerNameText.transform.parent.name == "SpeakerBadge")
                {
                    bool showBadge = !string.IsNullOrEmpty(line.speaker) && line.speaker != "NARATOR" && line.backgroundKey != "color_maroon";
                    speakerNameText.transform.parent.gameObject.SetActive(showBadge);
                }
            }

            bool isMaroon = (line.backgroundKey == "color_maroon");

            // Background handling
            if (isMaroon)
            {
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = null;
                    backgroundImage.color = new Color(0.13f, 0.04f, 0.04f, 1f); // #210909 dark maroon
                }
            }
            else if (!string.IsNullOrEmpty(line.backgroundKey))
            {
                string bgKey = line.backgroundKey;
                if (bgKey.EndsWith(".png") || bgKey.EndsWith(".jpg") || bgKey.EndsWith(".jpeg"))
                {
                    bgKey = System.IO.Path.ChangeExtension(bgKey, null);
                }
                Sprite bgSprite = Resources.Load<Sprite>(bgKey);
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

            // Head Avatar handling ("ada kepala gitu")
            bool hasHead = !isMaroon && !string.IsNullOrEmpty(line.portraitKey) && line.portraitKey != "none" && line.speaker != "NARATOR";
            if (headAvatarImage != null)
            {
                if (hasHead)
                {
                    headAvatarImage.gameObject.SetActive(true);
                    if (line.portraitKey == "penjelajah") headAvatarImage.sprite = penjelajahHead != null ? penjelajahHead : penjelajahPortrait;
                    else if (line.portraitKey == "sawit_man") headAvatarImage.sprite = sawitManHead != null ? sawitManHead : sawitManPortrait;
                    else if (line.portraitKey == "monyet") headAvatarImage.sprite = monyetHead != null ? monyetHead : monyetPortrait;
                    else if (line.portraitKey == "harimau") headAvatarImage.sprite = harimauHead != null ? harimauHead : harimauPortrait;
                }
                else
                {
                    headAvatarImage.gameObject.SetActive(false);
                }
            }

            // Speech Bubble Pointer Tail
            if (speechTailImage != null)
            {
                speechTailImage.gameObject.SetActive(hasHead);
            }

            // Positioning of Speech Card and Head Avatar (Top vs Bottom vs Center)
            if (cardBorderRT != null)
            {
                if (isMaroon)
                {
                    // Center Card: Borderless, centered text on dark maroon
                    cardBorderRT.anchorMin = new Vector2(0.10f, 0.30f);
                    cardBorderRT.anchorMax = new Vector2(0.90f, 0.70f);
                    cardBorderRT.offsetMin = Vector2.zero;
                    cardBorderRT.offsetMax = Vector2.zero;

                    if (cardBorderImage != null) cardBorderImage.color = Color.clear;
                    if (cardFillImage != null) cardFillImage.color = Color.clear;

                    if (dialogueBodyText != null)
                    {
                        dialogueBodyText.color = Color.white;
                        dialogueBodyText.alignment = TextAnchor.MiddleCenter;
                        dialogueBodyText.fontSize = (line.text == "PERJALANAN DIMULAI!") ? 54 : 36;
                    }
                }
                else
                {
                    if (cardBorderImage != null) cardBorderImage.color = new Color(0.88f, 0.48f, 0.48f, 1f); // soft reddish border
                    if (cardFillImage != null) cardFillImage.color = Color.white; // pure white card

                    if (dialogueBodyText != null)
                    {
                        dialogueBodyText.color = new Color(0.24f, 0.09f, 0.08f, 1f); // #3E1814 dark maroon
                        dialogueBodyText.alignment = TextAnchor.MiddleLeft;
                        dialogueBodyText.fontSize = 32;
                    }

                    if (line.cardPosition == "top")
                    {
                        // Speech Bubble at TOP (so it doesn't cover animals on the ground)
                        cardBorderRT.anchorMin = new Vector2(hasHead ? 0.22f : 0.05f, 0.63f);
                        cardBorderRT.anchorMax = new Vector2(0.95f, 0.94f);
                        cardBorderRT.offsetMin = Vector2.zero;
                        cardBorderRT.offsetMax = Vector2.zero;

                        if (headAvatarImage != null)
                        {
                            RectTransform hRT = headAvatarImage.rectTransform;
                            hRT.anchorMin = new Vector2(0.04f, 0.63f);
                            hRT.anchorMax = new Vector2(0.20f, 0.94f);
                            hRT.offsetMin = Vector2.zero;
                            hRT.offsetMax = Vector2.zero;
                        }
                    }
                    else
                    {
                        // Speech Bubble at BOTTOM (Scene 01, Scene 03)
                        cardBorderRT.anchorMin = new Vector2(hasHead ? 0.22f : 0.05f, 0.06f);
                        cardBorderRT.anchorMax = new Vector2(0.95f, 0.37f);
                        cardBorderRT.offsetMin = Vector2.zero;
                        cardBorderRT.offsetMax = Vector2.zero;

                        if (headAvatarImage != null)
                        {
                            RectTransform hRT = headAvatarImage.rectTransform;
                            hRT.anchorMin = new Vector2(0.04f, 0.06f);
                            hRT.anchorMax = new Vector2(0.20f, 0.37f);
                            hRT.offsetMin = Vector2.zero;
                            hRT.offsetMax = Vector2.zero;
                        }
                    }
                }
            }

            // Stage Actors (Chibi MC, Harimau, Monyet/Orangutan in scene)
            if (stageActorMC != null) stageActorMC.gameObject.SetActive(!isMaroon && (line.stageActor == "mc" || line.stageActor == "both"));
            if (stageActorAnimal != null) stageActorAnimal.gameObject.SetActive(!isMaroon && (line.stageActor == "animal" || line.stageActor == "both"));
            if (stageActorAnimal2 != null) stageActorAnimal2.gameObject.SetActive(!isMaroon && (line.stageActor == "both"));

            // Classic Full Portrait fallback if head avatar is not used
            if (portraitImage != null && headAvatarImage == null)
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
                else if (line.portraitKey == "harimau" && harimauPortrait != null)
                {
                    portraitImage.gameObject.SetActive(true);
                    portraitImage.sprite = harimauPortrait;
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
            typingCoroutine = StartCoroutine(TypeText(line));
        }

        /// <summary>
        /// Menentukan AudioClip voice tone yang cocok untuk baris percakapan saat ini.
        /// Dr. Sawit Man: menggunakan Normal Tone diawal, dan Angry Tone saat marah / konfrontasi.
        /// Narrator & Penjelajah (MC): berbagi tone yang sama karena MC mengingat memori sebagai narator.
        /// </summary>
        private AudioClip GetToneForLine(DialogueLine line)
        {
            if (line == null) return narratorMcTone;

            string speaker = (line.speaker ?? "").ToUpperInvariant();
            string portrait = (line.portraitKey ?? "").ToLowerInvariant();

            bool isSawitMan = speaker.Contains("SAWIT") || speaker.Contains("DOCTOR") || portrait == "sawit_man";

            if (isSawitMan)
            {
                if (IsAngryTone(line))
                {
                    return drSawitAngryTone != null ? drSawitAngryTone : drSawitNormalTone;
                }
                return drSawitNormalTone != null ? drSawitNormalTone : narratorMcTone;
            }

            // Narrator dan Penjelajah (MC) menggunakan Narrator + MC tone
            return narratorMcTone;
        }

        /// <summary>
        /// Mendeteksi apakah baris percakapan Dr. Sawit Man bernada marah/emosi tinggi.
        /// </summary>
        private bool IsAngryTone(DialogueLine line)
        {
            if (line == null) return false;

            if (!string.IsNullOrEmpty(line.sfxCue) &&
                (line.sfxCue.IndexOf("angry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 line.sfxCue.IndexOf("rage", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            string txt = (line.text ?? "").ToUpperInvariant();

            // Kata kunci atau tanda baca konfrontasi marah
            if (txt.Contains("!") ||
                txt.Contains("MEMPERINGATKAN") ||
                txt.Contains("MENGHADANG") ||
                txt.Contains("GERAM") ||
                txt.Contains("MARAH") ||
                txt.Contains("HENTIKAN") ||
                txt.Contains("LAWAN") ||
                txt.Contains("HANCUR"))
            {
                return true;
            }

            // Konfrontasi stage lanjut (Stage 2 atau 3) bernada tegang/marah kecuali dialog mengaku kalah
            if (GameFlowManager.Instance != null && GameFlowManager.Instance.currentStage >= 2)
            {
                if (!txt.Contains("MENGAKUI") && !txt.Contains("BERSEDIA MENYERAHKAN"))
                {
                    return true;
                }
            }

            return false;
        }

        private void StartVoiceTone(AudioClip toneClip)
        {
            if (voiceAudioSource == null || toneClip == null) return;

            voiceAudioSource.clip = toneClip;
            float clipLen = toneClip.length;

            // Random pick offset agar suara percakapan dinamis dan tidak selalu mulai dari detik ke-0
            try
            {
                if (clipLen > 4f)
                {
                    float maxStart = Mathf.Max(0f, clipLen - 4f);
                    voiceAudioSource.time = UnityEngine.Random.Range(0f, maxStart);
                }
                else
                {
                    voiceAudioSource.time = 0f;
                }
            }
            catch (Exception)
            {
                // Fallback jika platform membatasi seek time sebelum decompress
            }

            voiceAudioSource.volume = 0.85f;
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }

        private void StopVoiceTone()
        {
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }

        private IEnumerator TypeText(DialogueLine line)
        {
            isTyping = true;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            AudioClip toneClip = GetToneForLine(line);
            StartVoiceTone(toneClip);

            string text = line != null ? line.text : "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (dialogueBodyText != null)
                {
                    dialogueBodyText.text += c;
                }

                // Jeda alami percakapan saat bertemu tanda baca
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':')
                {
                    if (voiceAudioSource != null && voiceAudioSource.isPlaying)
                    {
                        voiceAudioSource.Pause();
                    }
                    yield return new WaitForSecondsRealtime(typingSpeed * 2.5f);
                    if (isTyping && voiceAudioSource != null && voiceAudioSource.clip != null)
                    {
                        voiceAudioSource.UnPause();
                    }
                }
                else
                {
                    // Fallback SFX ketikan halus jika tone audio belum tersedia
                    if (toneClip == null && i % 2 == 0 && typewriterSFX != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(typewriterSFX, 0.4f);
                    }
                    yield return new WaitForSecondsRealtime(typingSpeed);
                }
            }

            isTyping = false;
            StopVoiceTone();
            if (continueIndicator != null) continueIndicator.SetActive(true);
        }

        public void OnContinuePressed()
        {
            if (isTyping)
            {
                // Jika sedang mengetik, tampilkan seluruh teks sekaligus dan hentikan tone suara
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                if (dialogueBodyText != null) dialogueBodyText.text = fullCurrentText;
                isTyping = false;
                StopVoiceTone();
                if (continueIndicator != null) continueIndicator.SetActive(true);
            }
            else
            {
                // Pindah ke kalimat dialog berikutnya
                StopVoiceTone();
                currentLineIndex++;
                DisplayCurrentLine();
            }
        }

        private void EndDialogue()
        {
            StopVoiceTone();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            onCompleteCallback?.Invoke();
        }

        private void OnDisable()
        {
            StopVoiceTone();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;
        }

        private void Update()
        {
            // Aksesibilitas keyboard (Spasi / Enter untuk lanjut, Escape untuk Skip)
            if (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnContinuePressed();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SkipDialogue();
                }
            }
        }
    }
}
