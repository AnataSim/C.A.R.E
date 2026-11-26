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
        [SerializeField] private float typingSpeed = 0.020f; // Dipercepat agar dialog lebih responsif & dinamis
        [SerializeField] private float agotiVoicePitch = 1.35f; // Kecepatan pitch Agoti dipercepat (dicepetin)
        [SerializeField] private float agotiVoiceVolume = 0.95f; // Volume voice Agoti diperjelas (dibanyakin)
        [SerializeField] private AudioClip typewriterSFX;
        [SerializeField] private AudioSource audioSource;

        [Header("Character Voice / Speech Tones")]
        [SerializeField] private AudioClip narratorMcTone;
        [SerializeField] private AudioClip drSawitNormalTone;
        [SerializeField] private AudioClip drSawitAngryTone;

        [Header("Agoti Boss Tones (Per Stage)")]
        [SerializeField] private AudioClip agotiStage1Tone;       // agotiText.ogg (Stage 1)
        [SerializeField] private AudioClip agotiStage2Tone;       // agotiScaredText.ogg (Stage 2)
        [SerializeField] private AudioClip agotiStage3Tone;       // agotiAngryText.ogg (Stage 3)
        [SerializeField] private AudioClip boyfriendTone;         // boyfriendText.ogg (Penjelajah / MC)
        [SerializeField] private AudioClip skipDialogSFX;         // skipDialog.ogg

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

            // Auto-load Agoti Boss speech tone clips for each stage
            if (agotiStage1Tone == null)
            {
                agotiStage1Tone = Resources.Load<AudioClip>("Audio/tone/agotiText")
                               ?? Resources.Load<AudioClip>("Audio/tone/agotitext");
            }
            if (agotiStage2Tone == null)
            {
                agotiStage2Tone = Resources.Load<AudioClip>("Audio/tone/agotiScaredText")
                               ?? Resources.Load<AudioClip>("Audio/tone/agotiscaredtext");
            }
            if (agotiStage3Tone == null)
            {
                agotiStage3Tone = Resources.Load<AudioClip>("Audio/tone/agotiAngryText")
                               ?? Resources.Load<AudioClip>("Audio/tone/agotiangrytext");
            }
            if (boyfriendTone == null)
            {
                boyfriendTone = Resources.Load<AudioClip>("Audio/tone/boyfriendText")
                             ?? Resources.Load<AudioClip>("Audio/tone/boyfriendtext");
            }
            if (skipDialogSFX == null)
            {
                skipDialogSFX = Resources.Load<AudioClip>("Audio/tone/skipDialog")
                             ?? Resources.Load<AudioClip>("Audio/tone/skipdialog");
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
            StopVoiceTone(); // Hentikan tone suara saat ini seketika
            if (skipDialogSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(skipDialogSFX, 0.75f);
            }
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

            StopVoiceTone();
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;

            currentDialogueLines = new List<DialogueLine>(lines);
            currentLineIndex = 0;
            onCompleteCallback = onComplete;

            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            DisplayCurrentLine();
        }

        private void DisplayCurrentLine()
        {
            StopVoiceTone(); // Hentikan sisa tone audio baris sebelumnya agar tidak tabrakan

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
        /// Khusus Boss (Dr. Sawit Man):
        /// - Stage 1: agotiText (nada Agoti normal)
        /// - Stage 2: agotiScaredText (nada Agoti cemas/terancam)
        /// - Stage 3: agotiAngryText (nada Agoti marah/konfrontasi puncak)
        /// Penjelajah (MC), Narator, dan UI Petunjuk: null (hanya typewriterSFX agar hening & tidak ada suara gumaman di background)
        /// </summary>
        private AudioClip GetToneForLine(DialogueLine line)
        {
            if (line == null) return null;

            string speaker = (line.speaker ?? "").ToUpperInvariant();
            string portrait = (line.portraitKey ?? "").ToLowerInvariant();

            bool isSawitMan = speaker.Contains("SAWIT") || speaker.Contains("DOCTOR") || portrait == "sawit_man";

            if (isSawitMan)
            {
                int stage = GetCurrentStageNumber(line);
                if (stage == 1)
                {
                    return agotiStage1Tone;
                }
                else if (stage == 2)
                {
                    return agotiStage2Tone;
                }
                else // Stage 3
                {
                    return agotiStage3Tone;
                }
            }

            // Karakter lain (Penjelajah / Narator / UI) tidak menggunakan tone suara looping (hanya ketikan halus agar hening dan jernih)
            return null;
        }

        /// <summary>
        /// Mendapatkan nomor stage saat ini untuk penyesuaian suara tone Boss.
        /// </summary>
        public int GetCurrentStageNumber(DialogueLine line = null)
        {
            // 1. Cek isi teks dialog spesifik jika ada
            if (line != null && !string.IsNullOrEmpty(line.text))
            {
                string t = line.text;
                if (t.Contains("mengumpulkan cukup banyak catatan") || t.Contains("belum memahami seluruh ceritanya")) return 1;
                if (t.Contains("masih mengikuti jejak itu") || t.Contains("semakin dekat, tetapi masih ada satu bagian")) return 2;
                if (t.Contains("sampai sejauh ini. Apa lagi") || t.Contains("mengakui bahwa tindakanku") || t.Contains("bersedia menyerahkan informasi")) return 3;
            }

            // 2. Cek alur GameFlowManager
            if (GameFlowManager.Instance != null && GameFlowManager.Instance.currentStage > 0)
            {
                return GameFlowManager.Instance.currentStage;
            }

            // 3. Cek local storage progress
            if (CARE.Data.CareSaveManager.Instance != null && CARE.Data.CareSaveManager.Instance.CurrentStage > 0)
            {
                return CARE.Data.CareSaveManager.Instance.CurrentStage;
            }

            // 4. Cek referensi daftar dialog boss yang sedang berjalan
            if (currentDialogueLines != null && currentDialogueLines.Count > 0)
            {
                var first = currentDialogueLines[0];
                if (CareStoryDatabase.BossStage1_Intro != null && CareStoryDatabase.BossStage1_Intro.Count > 0 && first == CareStoryDatabase.BossStage1_Intro[0]) return 1;
                if (CareStoryDatabase.BossStage1_Defeat != null && CareStoryDatabase.BossStage1_Defeat.Count > 0 && first == CareStoryDatabase.BossStage1_Defeat[0]) return 1;

                if (CareStoryDatabase.BossStage2_Intro != null && CareStoryDatabase.BossStage2_Intro.Count > 0 && first == CareStoryDatabase.BossStage2_Intro[0]) return 2;
                if (CareStoryDatabase.BossStage2_Defeat != null && CareStoryDatabase.BossStage2_Defeat.Count > 0 && first == CareStoryDatabase.BossStage2_Defeat[0]) return 2;

                if (CareStoryDatabase.BossStage3_Intro != null && CareStoryDatabase.BossStage3_Intro.Count > 0 && first == CareStoryDatabase.BossStage3_Intro[0]) return 3;
                if (CareStoryDatabase.BossStage3_Defeat != null && CareStoryDatabase.BossStage3_Defeat.Count > 0 && first == CareStoryDatabase.BossStage3_Defeat[0]) return 3;
            }

            return 1;
        }

        private void StartVoiceTone(AudioClip toneClip, bool isAgoti = false)
        {
            if (voiceAudioSource == null) return;

            // Jika tidak ada toneClip (misal Narator), hentikan total voice source agar hening
            if (toneClip == null)
            {
                StopVoiceTone();
                return;
            }

            // Hentikan suara sebelumnya sebelum berganti ke klip baru
            StopVoiceTone();

            voiceAudioSource.clip = toneClip;
            voiceAudioSource.time = 0f;
            voiceAudioSource.volume = isAgoti ? agotiVoiceVolume : 0.85f;
            voiceAudioSource.pitch = isAgoti ? agotiVoicePitch : 1.0f; // Dicepetin pitch-nya saat Agoti berbicara
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }

        private void StopVoiceTone()
        {
            if (voiceAudioSource != null)
            {
                if (voiceAudioSource.isPlaying)
                {
                    voiceAudioSource.Stop();
                }
                voiceAudioSource.loop = false;
                voiceAudioSource.clip = null; // Kosongkan agar buffer audio tidak bocor ke baris dialog berikutnya
                voiceAudioSource.time = 0f;
                voiceAudioSource.pitch = 1.0f; // Reset pitch kembali normal
            }
        }

        private IEnumerator TypeText(DialogueLine line)
        {
            isTyping = true;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            AudioClip toneClip = GetToneForLine(line);
            string speaker = (line != null && line.speaker != null) ? line.speaker.ToUpperInvariant() : "";
            string portrait = (line != null && line.portraitKey != null) ? line.portraitKey.ToLowerInvariant() : "";
            bool isBoss = speaker.Contains("SAWIT") || speaker.Contains("DOCTOR") || portrait == "sawit_man";

            StartVoiceTone(toneClip, isBoss);

            string text = line != null ? line.text : "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (dialogueBodyText != null)
                {
                    dialogueBodyText.text += c;
                }

                // Jeda alami percakapan saat bertemu tanda baca (dipercepat agar looping tidak terlalu lama terhenti)
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':')
                {
                    if (voiceAudioSource != null && voiceAudioSource.isPlaying)
                    {
                        voiceAudioSource.Pause();
                    }
                    yield return new WaitForSecondsRealtime(typingSpeed * 1.4f);
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
