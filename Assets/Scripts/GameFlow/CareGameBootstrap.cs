using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CARE.Story;
using CARE.UI;
using CARE.Gameplay;
using CARE.Audio;
using CARE.Data;

namespace CARE.Core
{
    /// <summary>
    /// Master program bootstrapper for C.A.R.E.
    /// Automatically initializes, builds, and connects all game systems,
    /// audio managers, UI screens, and interactive gameplay at runtime.
    /// </summary>
    public class CareGameBootstrap : MonoBehaviour
    {
        public static CareGameBootstrap Instance { get; private set; }

        private Canvas mainCanvas;
        [SerializeField] private Font customFont;
        private Font gameFont;

        // UI Panels
        private GameObject titlePanel;
        private GameObject storyPanel;
        private GameObject lobbyPanel;
        private GameObject stageMapPanel;
        private GameObject hiddenObjectPanel;
        private GameObject bossPanel;
        private GameObject endingPanel;
        private GameObject encyclopediaModal;

        // Title Screen Interactive Buttons
        private Button titleLobbyButton;
        private Text titleLobbyButtonText;
        private Image titleLobbyButtonImage;
        private Button titlePlayButton;
        private Text titlePlayButtonText;

        // Lobby dynamic UI
        private Text lobbyProgressText;
        private Image rusaCardImage;
        private Text rusaCardStatus;
        private Image orangutanCardImage;
        private Text orangutanCardStatus;
        private Image gajahCardImage;
        private Text gajahCardStatus;

        // Boss fight dynamic UI
        private Text bossQuoteText;
        private Button bossEvidenceBtn1;
        private Button bossEvidenceBtn2;
        private Image bossArtImage;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeGame()
        {
            if (FindObjectOfType<CareGameBootstrap>() == null)
            {
                GameObject bootObj = new GameObject("CareGameBootstrap");
                bootObj.AddComponent<CareGameBootstrap>();
                DontDestroyOnLoad(bootObj);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load built-in font safely
            gameFont = GetDefaultFont();

            // Ensure Save Manager (Local Storage)
            EnsureSaveManager();

            // Ensure Audio Manager
            EnsureAudioManager();

            // Ensure Core Managers
            EnsureGameFlowManager();
            EnsureDialogueManager();
            EnsureEncyclopediaManager();
            EnsureMatch3CombatManager();
            EnsureHiddenObjectManager();

            // Setup Canvas & UI Hierarchy
            SetupCanvasAndUI();
        }

        private Font GetDefaultFont()
        {
            if (customFont != null) return customFont;
            return Match3CombatManager.GetGameFont();
        }

        private void Start()
        {
            // Connect Panels to GameFlowManager
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetupPanels(
                    titlePanel,
                    storyPanel,
                    lobbyPanel,
                    stageMapPanel,
                    hiddenObjectPanel,
                    bossPanel,
                    endingPanel
                );

                GameFlowManager.Instance.ShowTitleScreen();
            }

            UpdateLobbyUI();
            UpdateTitleScreenLobbyButton();
        }

        private void Update()
        {
            // Keyboard shortcuts for smooth testing & accessibility
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (encyclopediaModal != null && encyclopediaModal.activeSelf)
                {
                    EncyclopediaManager.Instance.ClosePanel();
                }
                else if (GameFlowManager.Instance != null && GameFlowManager.Instance.currentState != GameState.TitleScreen)
                {
                    GameFlowManager.Instance.ShowLobbyRestorasi();
                }
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.OpenStageMap();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartStage(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartStage(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartStage(3);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (EncyclopediaManager.Instance != null) EncyclopediaManager.Instance.OpenEncyclopedia("rusa_timor");
            }
        }

        #region Core Managers Setup

        private void EnsureSaveManager()
        {
            if (CareSaveManager.Instance == null)
            {
                GameObject saveObj = new GameObject("CareSaveManager");
                saveObj.AddComponent<CareSaveManager>();
            }
        }

        private void EnsureAudioManager()
        {
            if (CareAudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("CareAudioManager");
                audioObj.AddComponent<CareAudioManager>();
            }
        }

        private void EnsureGameFlowManager()
        {
            if (GameFlowManager.Instance == null)
            {
                GameObject flowObj = new GameObject("GameFlowManager");
                flowObj.AddComponent<GameFlowManager>();
            }
        }

        private void EnsureDialogueManager()
        {
            if (DialogueManager.Instance == null)
            {
                GameObject diagObj = new GameObject("DialogueManager");
                diagObj.AddComponent<DialogueManager>();
            }
        }

        private void EnsureEncyclopediaManager()
        {
            if (EncyclopediaManager.Instance == null)
            {
                GameObject encObj = new GameObject("EncyclopediaManager");
                encObj.AddComponent<EncyclopediaManager>();
            }
        }

        private void EnsureMatch3CombatManager()
        {
            if (Match3CombatManager.Instance == null)
            {
                GameObject m3Obj = new GameObject("Match3CombatManager");
                m3Obj.AddComponent<Match3CombatManager>();
            }
        }

        private void EnsureHiddenObjectManager()
        {
            if (HiddenObjectManager.Instance == null)
            {
                GameObject hoObj = new GameObject("HiddenObjectManager");
                hoObj.AddComponent<HiddenObjectManager>();
            }
        }

        #endregion

        #region UI Construction

        private void SetupCanvasAndUI()
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Pastikan CanvasScaler selalu terpasang dengan mode skala proporsional (1920x1080)
            CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Ensure EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 1. Title Screen (use existing MainPanel if available)
            Transform existingMainPanel = mainCanvas.transform.Find("MainPanel");
            if (existingMainPanel != null)
            {
                titlePanel = existingMainPanel.gameObject;
                WireExistingTitlePanel(existingMainPanel);
            }
            else
            {
                titlePanel = BuildTitlePanel(mainCanvas.transform);
            }

            // 2. Opening Story / Dialogue Panel
            storyPanel = BuildDialoguePanel(mainCanvas.transform);

            // 3. Lobby Restorasi Panel
            lobbyPanel = BuildLobbyPanel(mainCanvas.transform);

            // 4. Stage Map Panel
            stageMapPanel = BuildStageMapPanel(mainCanvas.transform);

            // 5. Hidden Object Panel
            hiddenObjectPanel = BuildHiddenObjectPanel(mainCanvas.transform);

            // 6. Boss Fight Panel
            bossPanel = BuildBossPanel(mainCanvas.transform);

            // 7. Ending Panel
            endingPanel = BuildEndingPanel(mainCanvas.transform);

            // 8. Encyclopedia Modal
            encyclopediaModal = BuildEncyclopediaModal(mainCanvas.transform);
        }

        private void WireExistingTitlePanel(Transform panel)
        {
            // 1. Format background MainPanel dengan warna langit merah bencana alam agar tidak ada celah transparan
            Image mainPanelImg = panel.GetComponent<Image>();
            if (mainPanelImg != null)
            {
                mainPanelImg.sprite = null;
                mainPanelImg.color = new Color(0.35f, 0.12f, 0.07f, 1f); // Reddish-maroon wildfire sky
                mainPanelImg.enabled = true;
            }

            // 2. Format Environment (Latar belakang hutan & asap kebakaran) menggunakan background bersih tanpa logo ganda
            Transform env = panel.Find("Environment");
            if (env != null)
            {
                RectTransform envRT = env.GetComponent<RectTransform>();
                if (envRT != null)
                {
                    envRT.anchorMin = Vector2.zero;
                    envRT.anchorMax = Vector2.one;
                    envRT.offsetMin = Vector2.zero;
                    envRT.offsetMax = Vector2.zero;
                    envRT.anchoredPosition = Vector2.zero;
                }
                Image envImg = env.GetComponent<Image>();
                if (envImg != null)
                {
                    envImg.preserveAspect = false;
                    envImg.color = Color.white;
                    Sprite cleanBg = Resources.Load<Sprite>("Environtment/title_screen_bg");
                    if (cleanBg != null) envImg.sprite = cleanBg;
                }
            }

            // 3. Posisikan Logo C.A.R.E. (Button_Group) di tengah atas (Upper-Center) di atas pohon & asap
            Transform groupBtn = panel.Find("Button_Group");
            if (groupBtn != null)
            {
                groupBtn.SetAsLastSibling();
                RectTransform grpRT = groupBtn.GetComponent<RectTransform>();
                if (grpRT != null)
                {
                    grpRT.anchorMin = new Vector2(0.5f, 0.68f);
                    grpRT.anchorMax = new Vector2(0.5f, 0.68f);
                    grpRT.pivot = new Vector2(0.5f, 0.5f);
                    grpRT.anchoredPosition = Vector2.zero;
                    grpRT.sizeDelta = new Vector2(760, 270);
                }
                Image grpImg = groupBtn.GetComponent<Image>();
                if (grpImg != null)
                {
                    grpImg.preserveAspect = true;
                }
                Button btn = groupBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayItemFoundSFX();
                    });
                }
            }

            // 4. Posisikan Tombol Mulai (Button_Play) secara proporsional di tengah layar
            Transform playBtn = panel.Find("Button_Play");
            if (playBtn != null)
            {
                playBtn.SetAsLastSibling();
                RectTransform playRT = playBtn.GetComponent<RectTransform>();
                if (playRT != null)
                {
                    playRT.anchorMin = new Vector2(0.5f, 0.38f);
                    playRT.anchorMax = new Vector2(0.5f, 0.38f);
                    playRT.pivot = new Vector2(0.5f, 0.5f);
                    playRT.anchoredPosition = Vector2.zero;
                    playRT.sizeDelta = new Vector2(380, 125);
                }
                Image playImg = playBtn.GetComponent<Image>();
                if (playImg != null)
                {
                    playImg.preserveAspect = true;
                }
                Button btn = playBtn.GetComponent<Button>();
                if (btn == null) btn = playBtn.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[C.A.R.E.] Tombol Mulai Petualangan diklik!");
                    if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                    if (GameFlowManager.Instance != null)
                    {
                        // Jika pemain baru (belum menyelesaikan prolog), jalankan narasi pembukaan
                        if (CareSaveManager.Instance != null && !CareSaveManager.Instance.IsPrologueCompleted)
                        {
                            GameFlowManager.Instance.StartOpeningStory();
                        }
                        else
                        {
                            // Pemain yang sudah menyelesaikan pembukaan langsung menuju Lobby Restorasi
                            GameFlowManager.Instance.ShowLobbyRestorasi();
                        }
                    }
                });

                titlePlayButton = btn;
                titlePlayButtonText = playBtn.GetComponentInChildren<Text>();
            }

            // 5. Tombol Navigasi Menuju Lobby Restorasi
            Transform existingLobbyBtn = panel.Find("BtnNavLobby");
            Button lbBtn = null;
            if (existingLobbyBtn == null)
            {
                lbBtn = CreateUIButton("BtnNavLobby", panel, "🐾 LOBBY RESTORASI", new Vector2(0, 0), new Vector2(360, 65), new Color(0.18f, 0.65f, 0.32f), () =>
                {
                    OnTitleLobbyButtonClicked();
                });
                RectTransform lbRT = lbBtn.GetComponent<RectTransform>();
                lbRT.anchorMin = new Vector2(0.5f, 0.23f);
                lbRT.anchorMax = new Vector2(0.5f, 0.23f);
                lbRT.pivot = new Vector2(0.5f, 0.5f);
                lbRT.anchoredPosition = Vector2.zero;
            }
            else
            {
                existingLobbyBtn.SetAsLastSibling();
                lbBtn = existingLobbyBtn.GetComponent<Button>();
                if (lbBtn == null) lbBtn = existingLobbyBtn.gameObject.AddComponent<Button>();
                lbBtn.onClick.RemoveAllListeners();
                lbBtn.onClick.AddListener(() =>
                {
                    OnTitleLobbyButtonClicked();
                });
            }

            titleLobbyButton = lbBtn;
            if (lbBtn != null)
            {
                titleLobbyButtonImage = lbBtn.GetComponent<Image>();
                titleLobbyButtonText = lbBtn.GetComponentInChildren<Text>();
            }

            UpdateTitleScreenLobbyButton();
        }

        private GameObject BuildTitlePanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("TitlePanel", parent, new Color(0.08f, 0.14f, 0.1f, 1f));
            Image bgImg = panel.GetComponent<Image>();
            Sprite titleBg = Resources.Load<Sprite>("Environtment/title_screen_bg");
            if (titleBg == null) titleBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (titleBg != null)
            {
                bgImg.sprite = titleBg;
                bgImg.color = Color.white;
            }

            // Jika titleBg belum ada, tampilkan teks judul alternatif
            if (titleBg == null)
            {
                CreateUIText("TitleText", panel.transform, "C.A.R.E.", 80, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 150), new Vector2(900, 120));
                CreateUIText("SubtitleText", panel.transform, "Cegah. Api. Rawat. Ekosistem.", 28, Color.white, FontStyle.Normal, new Vector2(0, 60), new Vector2(900, 60));
            }

            // Tombol Mulai dengan gaya gradien emas-oranye sesuai desain Figma
            Button playBtnObj = CreateUIButton("BtnPlay", panel.transform, "▶ MULAI PETUALANGAN", new Vector2(0, -180), new Vector2(440, 75), new Color(0.96f, 0.72f, 0.16f), () =>
            {
                if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                if (GameFlowManager.Instance != null)
                {
                    if (CareSaveManager.Instance != null && !CareSaveManager.Instance.IsPrologueCompleted)
                    {
                        GameFlowManager.Instance.StartOpeningStory();
                    }
                    else
                    {
                        GameFlowManager.Instance.ShowLobbyRestorasi();
                    }
                }
            });
            titlePlayButton = playBtnObj;
            titlePlayButtonText = playBtnObj.GetComponentInChildren<Text>();

            // Tombol Lobby Restorasi
            Button lobbyBtnObj = CreateUIButton("BtnLobby", panel.transform, "🐾 LOBBY RESTORASI", new Vector2(0, -270), new Vector2(400, 65), new Color(0.18f, 0.65f, 0.32f), () =>
            {
                OnTitleLobbyButtonClicked();
            });
            titleLobbyButton = lobbyBtnObj;
            titleLobbyButtonImage = lobbyBtnObj.GetComponent<Image>();
            titleLobbyButtonText = lobbyBtnObj.GetComponentInChildren<Text>();

            UpdateTitleScreenLobbyButton();

            return panel;
        }

        private void OnTitleLobbyButtonClicked()
        {
            bool isUnlocked = CareSaveManager.Instance != null && CareSaveManager.Instance.IsPrologueCompleted;
            if (isUnlocked)
            {
                Debug.Log("[C.A.R.E.] Tombol Lobby Restorasi diklik! Membuka Lobby.");
                if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
            }
            else
            {
                Debug.Log("[C.A.R.E.] Tombol Lobby Restorasi terkunci. Pemain harus menyelesaikan narasi awal terlebih dahulu!");
                if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayWrongClickSFX();
            }
        }

        /// <summary>
        /// Mengatur apakah tombol Lobby di Title Screen aktif atau terkunci.
        /// Jika user baru pertama kali buka game (belum tamat narasi awal), tombol ini TIDAK BISA DI-KLIK (interactable = false)
        /// dan berwarna abu-abu gelap dengan teks terkunci.
        /// </summary>
        public void UpdateTitleScreenLobbyButton()
        {
            if (titleLobbyButton == null) return;

            bool isUnlocked = CareSaveManager.Instance != null && CareSaveManager.Instance.IsPrologueCompleted;

            if (titleLobbyButtonText == null) titleLobbyButtonText = titleLobbyButton.GetComponentInChildren<Text>();
            if (titleLobbyButtonImage == null) titleLobbyButtonImage = titleLobbyButton.GetComponent<Image>();

            if (isUnlocked)
            {
                // Terbuka: Aktif, berwarna hijau cerah, dan bisa diklik
                titleLobbyButton.interactable = true;
                if (titleLobbyButtonText != null)
                {
                    titleLobbyButtonText.text = "🐾 LOBBY RESTORASI";
                    titleLobbyButtonText.color = Color.white;
                }
                if (titleLobbyButtonImage != null)
                {
                    titleLobbyButtonImage.color = new Color(0.18f, 0.65f, 0.32f, 1f);
                }
            }
            else
            {
                // Terkunci bagi pemain baru: tidak bisa diklik sama sekali (interactable = false)
                titleLobbyButton.interactable = false;
                if (titleLobbyButtonText != null)
                {
                    titleLobbyButtonText.text = "🔒 LOBBY RESTORASI (Terkunci)";
                    titleLobbyButtonText.color = new Color(0.7f, 0.7f, 0.7f, 0.65f);
                }
                if (titleLobbyButtonImage != null)
                {
                    titleLobbyButtonImage.color = new Color(0.24f, 0.28f, 0.25f, 0.85f);
                }
            }

            // Update teks tombol Play jika sudah menyelesaikan prolog
            if (titlePlayButtonText != null)
            {
                titlePlayButtonText.text = isUnlocked ? "▶ LANJUTKAN PETUALANGAN" : "▶ MULAI PETUALANGAN";
            }
        }

        private GameObject BuildDialoguePanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("DialoguePanel", parent, new Color(0.05f, 0.08f, 0.06f, 0.96f));

            // Background scenery art if available (default hutan terbakar)
            Image bgImg = panel.GetComponent<Image>();
            Sprite forestBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (forestBg != null)
            {
                bgImg.sprite = forestBg;
                bgImg.color = Color.white;
            }

            // Top-Right SKIP Button (sesuai referensi Figma / Image 2)
            GameObject skipObj = new GameObject("BtnSkipStory");
            skipObj.transform.SetParent(panel.transform, false);
            RectTransform skipRT = skipObj.AddComponent<RectTransform>();
            skipRT.anchorMin = new Vector2(1f, 1f);
            skipRT.anchorMax = new Vector2(1f, 1f);
            skipRT.pivot = new Vector2(1f, 1f);
            skipRT.anchoredPosition = new Vector2(-40f, -30f);
            skipRT.sizeDelta = new Vector2(130f, 52f);

            Image skipImg = skipObj.AddComponent<Image>();
            skipImg.color = new Color(0.14f, 0.12f, 0.12f, 0.94f);
            skipImg.raycastTarget = true;

            Button skipBtn = skipObj.AddComponent<Button>();
            skipBtn.targetGraphic = skipImg;
            ColorBlock sColors = skipBtn.colors;
            sColors.normalColor = new Color(0.14f, 0.12f, 0.12f, 0.94f);
            sColors.highlightedColor = new Color(0.35f, 0.20f, 0.20f, 1f);
            sColors.pressedColor = new Color(0.08f, 0.06f, 0.06f, 1f);
            skipBtn.colors = sColors;

            Text skipTxt = CreateUIText("Label", skipObj.transform, "SKIP", 26, Color.white, FontStyle.Bold, Vector2.zero, new Vector2(130, 52));
            skipTxt.alignment = TextAnchor.MiddleCenter;
            skipTxt.raycastTarget = false;

            // Large Portrait (Left/Center untuk dialog karakter)
            GameObject portraitObj = new GameObject("PortraitImage");
            portraitObj.transform.SetParent(panel.transform, false);
            RectTransform portraitRT = portraitObj.AddComponent<RectTransform>();
            portraitRT.anchorMin = new Vector2(0.07f, 0.38f);
            portraitRT.anchorMax = new Vector2(0.32f, 0.90f);
            portraitRT.offsetMin = Vector2.zero;
            portraitRT.offsetMax = Vector2.zero;
            Image portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;
            portraitObj.SetActive(false);

            // Dialogue Card Border (Kotak luar dengan aksen merah koral)
            GameObject cardBorderObj = CreateUIBox("DialogueCardBorder", panel.transform, new Color(0.85f, 0.40f, 0.36f, 1f));
            RectTransform cardBorderRT = cardBorderObj.GetComponent<RectTransform>();
            cardBorderRT.anchorMin = new Vector2(0.07f, 0.06f);
            cardBorderRT.anchorMax = new Vector2(0.93f, 0.36f);
            cardBorderRT.offsetMin = Vector2.zero;
            cardBorderRT.offsetMax = Vector2.zero;

            // Dialogue Card Fill (Kartu putih bersih di dalam border)
            GameObject cardFillObj = CreateUIBox("DialogueCardFill", cardBorderObj.transform, Color.white);
            RectTransform cardFillRT = cardFillObj.GetComponent<RectTransform>();
            cardFillRT.anchorMin = Vector2.zero;
            cardFillRT.anchorMax = Vector2.one;
            cardFillRT.offsetMin = new Vector2(4f, 4f);
            cardFillRT.offsetMax = new Vector2(-4f, -4f);
            Image cardFillImg = cardFillObj.GetComponent<Image>();
            cardFillImg.raycastTarget = true;

            // Speaker Badge (Tag nama pembicara jika bukan narator murni)
            GameObject speakerBadgeObj = CreateUIBox("SpeakerBadge", cardBorderObj.transform, new Color(0.28f, 0.10f, 0.10f, 0.95f));
            RectTransform spkBadgeRT = speakerBadgeObj.GetComponent<RectTransform>();
            spkBadgeRT.anchorMin = new Vector2(0f, 1f);
            spkBadgeRT.anchorMax = new Vector2(0f, 1f);
            spkBadgeRT.pivot = new Vector2(0f, 0f);
            spkBadgeRT.anchoredPosition = new Vector2(15f, 6f);
            spkBadgeRT.sizeDelta = new Vector2(240f, 40f);

            Text speakerText = CreateUIText("SpeakerName", speakerBadgeObj.transform, "NARATOR", 20, new Color(0.98f, 0.85f, 0.35f, 1f), FontStyle.Bold, Vector2.zero, new Vector2(240, 40));
            speakerText.alignment = TextAnchor.MiddleCenter;
            speakerBadgeObj.SetActive(false);

            // Dialogue Body Text (Teks cokelat gelap/maroon tebal di dalam kartu putih)
            GameObject bodyObj = new GameObject("DialogueBody");
            bodyObj.transform.SetParent(cardFillObj.transform, false);
            RectTransform bodyRT = bodyObj.AddComponent<RectTransform>();
            bodyRT.anchorMin = Vector2.zero;
            bodyRT.anchorMax = Vector2.one;
            bodyRT.pivot = new Vector2(0.5f, 0.5f);
            bodyRT.offsetMin = new Vector2(50f, 25f);
            bodyRT.offsetMax = new Vector2(-90f, -25f);

            Text bodyText = bodyObj.AddComponent<Text>();
            bodyText.text = "Menyiapkan narasi...";
            bodyText.font = gameFont;
            bodyText.fontSize = 28;
            bodyText.fontStyle = FontStyle.Bold;
            bodyText.color = new Color(0.28f, 0.10f, 0.10f, 1f); // #471A1A
            bodyText.alignment = TextAnchor.MiddleLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.raycastTarget = false;

            // Continue Indicator (Ikon segitiga merah ▶ di sudut kanan bawah kartu)
            GameObject indObj = new GameObject("ContinueIndicator");
            indObj.transform.SetParent(cardFillObj.transform, false);
            RectTransform indRT = indObj.AddComponent<RectTransform>();
            indRT.anchorMin = new Vector2(1f, 0f);
            indRT.anchorMax = new Vector2(1f, 0f);
            indRT.pivot = new Vector2(1f, 0f);
            indRT.anchoredPosition = new Vector2(-30f, 25f);
            indRT.sizeDelta = new Vector2(45f, 45f);

            Text continueIndicator = indObj.AddComponent<Text>();
            continueIndicator.text = "▶";
            continueIndicator.font = gameFont;
            continueIndicator.fontSize = 32;
            continueIndicator.fontStyle = FontStyle.Bold;
            continueIndicator.color = new Color(0.88f, 0.20f, 0.20f, 1f);
            continueIndicator.alignment = TextAnchor.MiddleCenter;
            continueIndicator.raycastTarget = false;

            // Tombol klik untuk lanjut pada kartu dialog
            Button continueBtn = cardFillObj.AddComponent<Button>();
            continueBtn.targetGraphic = cardFillImg;
            ColorBlock cColors = continueBtn.colors;
            cColors.normalColor = Color.white;
            cColors.highlightedColor = new Color(0.98f, 0.98f, 0.98f, 1f);
            cColors.pressedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            continueBtn.colors = cColors;

            // Registrasi komponen ke DialogueManager
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.SetupUI(panel, speakerText, bodyText, portraitImg, continueBtn, continueIndicator.gameObject, bgImg, skipBtn);
            }

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildLobbyPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("LobbyRestorasiPanel", parent, new Color(0.08f, 0.16f, 0.11f, 1f));

            // Background: Posko Restorasi & Klinik Hewan dari Figma
            Image bgImg = panel.GetComponent<Image>();
            Sprite clinicBg = Resources.Load<Sprite>("Environtment/restoration_clinic");
            if (clinicBg == null) clinicBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (clinicBg != null) { bgImg.sprite = clinicBg; bgImg.color = Color.white; }

            // Header
            CreateUIText("LobbyTitle", panel.transform, "LOBBY RESTORASI EKOSISTEM", 40, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 420), new Vector2(1200, 50));
            CreateUIText("LobbySubtitle", panel.transform, "Posko Evakuasi, Klinik Perawatan, dan Konservasi Satwa Liar", 20, new Color(0.85f, 0.95f, 0.85f), FontStyle.Normal, new Vector2(0, 375), new Vector2(1200, 35));

            lobbyProgressText = CreateUIText("ProgressText", panel.transform, "Satwa Terselamatkan: 0 / 3", 24, new Color(0.4f, 0.95f, 0.5f), FontStyle.Bold, new Vector2(0, 335), new Vector2(600, 35));

            // 3 Habitat Cards Container
            GameObject cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);
            RectTransform ccRT = cardsContainer.AddComponent<RectTransform>();
            ccRT.sizeDelta = new Vector2(1260, 460);
            ccRT.anchoredPosition = new Vector2(0, 25);

            // Card 1: Rusa Timor
            BuildAnimalCard(cardsContainer.transform, -420, "rusa_timor", "Rusa Timor", "Stage 1", out rusaCardImage, out rusaCardStatus);

            // Card 2: Orangutan
            BuildAnimalCard(cardsContainer.transform, 0, "orangutan", "Orangutan", "Stage 2", out orangutanCardImage, out orangutanCardStatus);

            // Card 3: Gajah Asia
            BuildAnimalCard(cardsContainer.transform, 420, "gajah_asia", "Gajah Asia", "Stage 3", out gajahCardImage, out gajahCardStatus);

            // Bottom Buttons
            CreateUIButton("BtnMulaiMisi", panel.transform, "🗺️ MULAI MISI (PETA TAHAPAN)", new Vector2(0, -320), new Vector2(460, 70), new Color(0.18f, 0.65f, 0.32f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.OpenStageMap();
            });

            CreateUIButton("BtnMenuUtama", panel.transform, "🏠 MENU UTAMA", new Vector2(-460, -320), new Vector2(240, 58), new Color(0.28f, 0.35f, 0.38f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowTitleScreen();
            });

            CreateUIButton("BtnEnsiklopediaAll", panel.transform, "📖 ENSIKLOPEDIA", new Vector2(460, -320), new Vector2(240, 58), new Color(0.2f, 0.45f, 0.65f), () =>
            {
                if (EncyclopediaManager.Instance != null) EncyclopediaManager.Instance.OpenEncyclopedia("rusa_timor");
            });

            CreateUIButton("BtnReplayPrologue", panel.transform, "📜 BACA ULANG CERITA AWAL", new Vector2(0, -395), new Vector2(360, 48), new Color(0.22f, 0.45f, 0.38f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartOpeningStory();
            });

            panel.SetActive(false);
            return panel;
        }

        private void BuildAnimalCard(Transform container, float posX, string animalKey, string animalName, string stageTag, out Image cardImg, out Text statusTxt)
        {
            GameObject cardObj = CreateUIBox("Card_" + animalKey, container, new Color(0.1f, 0.18f, 0.14f, 0.95f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(360, 440);
            cardRT.anchoredPosition = new Vector2(posX, 0);

            // Animal Preview Image
            GameObject imgObj = new GameObject("AnimalPreview");
            imgObj.transform.SetParent(cardObj.transform, false);
            RectTransform imgRT = imgObj.AddComponent<RectTransform>();
            imgRT.sizeDelta = new Vector2(240, 210);
            imgRT.anchoredPosition = new Vector2(0, 85);
            cardImg = imgObj.AddComponent<Image>();
            cardImg.preserveAspect = true;

            // Name
            CreateUIText("AnimalName", cardObj.transform, animalName, 24, Color.white, FontStyle.Bold, new Vector2(0, -45), new Vector2(320, 35));

            // Status
            statusTxt = CreateUIText("AnimalStatus", cardObj.transform, "🔒 Terancam di " + stageTag, 18, new Color(0.95f, 0.5f, 0.4f), FontStyle.Normal, new Vector2(0, -85), new Vector2(320, 30));

            // Button View Encyclopedia
            string key = animalKey;
            CreateUIButton("BtnView", cardObj.transform, "📖 Buka Info Satwa", new Vector2(0, -150), new Vector2(260, 48), new Color(0.18f, 0.48f, 0.38f), () =>
            {
                if (EncyclopediaManager.Instance != null) EncyclopediaManager.Instance.OpenEncyclopedia(key);
            });
        }

        public void UpdateLobbyUI()
        {
            if (CareStoryDatabase.Animals == null) return;

            int unlockedCount = 0;
            foreach (var a in CareStoryDatabase.Animals)
            {
                if (a.isUnlocked) unlockedCount++;

                if (a.animalKey == "rusa_timor" && rusaCardImage != null && rusaCardStatus != null)
                {
                    rusaCardImage.sprite = Resources.Load<Sprite>(a.isUnlocked ? "TemporaryAssets/Animals/rusa_timor" : "TemporaryAssets/Animals/rusa_timor_silhouette");
                    rusaCardStatus.text = a.isUnlocked ? "✔ Berhasil Diselamatkan!" : "🔒 Terancam di Stage 1";
                    rusaCardStatus.color = a.isUnlocked ? new Color(0.4f, 0.95f, 0.5f) : new Color(0.95f, 0.5f, 0.4f);
                }
                else if (a.animalKey == "orangutan" && orangutanCardImage != null && orangutanCardStatus != null)
                {
                    orangutanCardImage.sprite = Resources.Load<Sprite>(a.isUnlocked ? "TemporaryAssets/Animals/orangutan" : "TemporaryAssets/Animals/orangutan_silhouette");
                    orangutanCardStatus.text = a.isUnlocked ? "✔ Berhasil Diselamatkan!" : "🔒 Terancam di Stage 2";
                    orangutanCardStatus.color = a.isUnlocked ? new Color(0.4f, 0.95f, 0.5f) : new Color(0.95f, 0.5f, 0.4f);
                }
                else if (a.animalKey == "gajah_asia" && gajahCardImage != null && gajahCardStatus != null)
                {
                    gajahCardImage.sprite = Resources.Load<Sprite>(a.isUnlocked ? "TemporaryAssets/Animals/gajah_asia" : "TemporaryAssets/Animals/gajah_asia_silhouette");
                    gajahCardStatus.text = a.isUnlocked ? "✔ Berhasil Diselamatkan!" : "🔒 Terancam di Stage 3";
                    gajahCardStatus.color = a.isUnlocked ? new Color(0.4f, 0.95f, 0.5f) : new Color(0.95f, 0.5f, 0.4f);
                }
            }

            if (lobbyProgressText != null)
            {
                lobbyProgressText.text = $"Satwa Terselamatkan: {unlockedCount} / {CareStoryDatabase.Animals.Count}";
            }
        }

        private GameObject BuildStageMapPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("StageMapPanel", parent, new Color(0.06f, 0.12f, 0.16f, 1f));

            CreateUIText("MapTitle", panel.transform, "PETA EKOSISTEM & MISI PENYELAMATAN", 40, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 420), new Vector2(1300, 50));
            CreateUIText("MapSubtitle", panel.transform, "Pilih kawasan terdampak untuk memulai investigasi dan rehabilitasi satwa", 20, Color.white, FontStyle.Normal, new Vector2(0, 375), new Vector2(1300, 35));

            // Stage 1 Card
            BuildStageCard(panel.transform, -420, "STAGE 1", "HUTAN DATARAN RENDAH", "Target: Rusa Timor\nInvestigasi Area Hutan Terbakar", 1, new Color(0.18f, 0.55f, 0.3f));

            // Stage 2 Card
            BuildStageCard(panel.transform, 0, "STAGE 2", "HUTAN RAWA GAMBUT", "Target: Orangutan\nInvestigasi Kanal & Pembalakan Ilegal", 2, new Color(0.55f, 0.4f, 0.15f));

            // Stage 3 Card
            BuildStageCard(panel.transform, 420, "STAGE 3", "KORIDOR PEGUNUNGAN", "Target: Gajah Asia & Doctor Sawit Man\nEvakuasi & Konfrontasi Terakhir", 3, new Color(0.65f, 0.2f, 0.2f));

            // Back button
            CreateUIButton("BtnBackLobby", panel.transform, "⬅️ KEMBALI KE LOBBY", new Vector2(0, -320), new Vector2(340, 65), new Color(0.3f, 0.35f, 0.4f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
            });

            panel.SetActive(false);
            return panel;
        }

        private void BuildStageCard(Transform container, float posX, string tag, string title, string desc, int stageNum, Color themeColor)
        {
            GameObject cardObj = CreateUIBox("StageCard_" + stageNum, container, new Color(themeColor.r * 0.4f, themeColor.g * 0.4f, themeColor.b * 0.4f, 0.95f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(360, 440);
            cardRT.anchoredPosition = new Vector2(posX, 20);

            CreateUIText("Tag", cardObj.transform, tag, 26, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 170), new Vector2(330, 40));
            CreateUIText("Title", cardObj.transform, title, 22, Color.white, FontStyle.Bold, new Vector2(0, 115), new Vector2(330, 55));
            CreateUIText("Desc", cardObj.transform, desc, 18, new Color(0.85f, 0.9f, 0.85f), FontStyle.Normal, new Vector2(0, 15), new Vector2(320, 110));

            int sNum = stageNum;
            CreateUIButton("BtnStartStage", cardObj.transform, "▶ MULAI TAHAP INI", new Vector2(0, -150), new Vector2(300, 60), themeColor, () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartStage(sNum);
            });
        }

        private GameObject BuildHiddenObjectPanel(Transform parent)
        {
            // Match-3 RPG Combat Panel (Ganti mekanik lama menjadi Match-3 Combat seperti Candy Crush & Dungeon Puzzles)
            GameObject panel = CreateFullScreenPanel("Match3CombatPanel", parent, new Color(0.08f, 0.12f, 0.1f, 1f));

            // Background
            Image bgImg = panel.GetComponent<Image>();
            Sprite forestBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (forestBg != null) { bgImg.sprite = forestBg; bgImg.color = new Color(0.35f, 0.38f, 0.35f, 1f); }

            // 1. Top Area (Location Title & Boss HP Bar)
            Text titleText = CreateUIText("LocationTitle", panel.transform, "⚔️ AREA INVESTIGASI: TEPI HUTAN", 20, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(30, -25), new Vector2(460, 40));
            RectTransform ttRT = titleText.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0f, 1f);
            ttRT.anchorMax = new Vector2(0f, 1f);
            ttRT.pivot = new Vector2(0f, 1f);
            titleText.alignment = TextAnchor.MiddleLeft;

            // Enemy Name Tag (Top Center)
            Text enemyNameText = CreateUIText("EnemyName", panel.transform, "DOCTOR SAWIT MAN", 18, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, -15), new Vector2(700, 30));
            RectTransform entRT = enemyNameText.GetComponent<RectTransform>();
            entRT.anchorMin = new Vector2(0.5f, 1f);
            entRT.anchorMax = new Vector2(0.5f, 1f);
            entRT.pivot = new Vector2(0.5f, 1f);

            // Enemy HP Bar Container (Top Center, below Name)
            GameObject hpBarObj = CreateUIBox("HPBarContainer", panel.transform, new Color(0.12f, 0.05f, 0.05f, 0.95f));
            RectTransform hpBarRT = hpBarObj.GetComponent<RectTransform>();
            hpBarRT.anchorMin = new Vector2(0.5f, 1f);
            hpBarRT.anchorMax = new Vector2(0.5f, 1f);
            hpBarRT.pivot = new Vector2(0.5f, 1f);
            hpBarRT.anchoredPosition = new Vector2(0, -45);
            hpBarRT.sizeDelta = new Vector2(780, 26);

            // HP Red Background Bar (Revealed when HP decreases!)
            GameObject hpBgObj = new GameObject("HPBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hpBgObj.transform.SetParent(hpBarObj.transform, false);
            RectTransform hpBgRT = hpBgObj.GetComponent<RectTransform>();
            hpBgRT.anchorMin = Vector2.zero;
            hpBgRT.anchorMax = Vector2.one;
            hpBgRT.offsetMin = new Vector2(2, 2);
            hpBgRT.offsetMax = new Vector2(-2, -2);
            Image hpBgImg = hpBgObj.GetComponent<Image>();
            hpBgImg.sprite = Match3CombatManager.GetWhiteSprite();
            hpBgImg.color = new Color(0.45f, 0.12f, 0.12f, 1f);
            hpBgImg.raycastTarget = false;

            // HP Fill (Front Health Bar that shrinks dynamically)
            GameObject hpFillObj = new GameObject("HPFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hpFillObj.transform.SetParent(hpBarObj.transform, false);
            RectTransform hpFillRT = hpFillObj.GetComponent<RectTransform>();
            hpFillRT.anchorMin = Vector2.zero;
            hpFillRT.anchorMax = Vector2.one;
            hpFillRT.offsetMin = new Vector2(2, 2);
            hpFillRT.offsetMax = new Vector2(-2, -2);
            Image hpFillImg = hpFillObj.GetComponent<Image>();
            hpFillImg.sprite = Match3CombatManager.GetWhiteSprite();
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Horizontal;
            hpFillImg.fillAmount = 1f;
            hpFillImg.color = new Color(0.2f, 0.88f, 0.38f);
            hpFillImg.raycastTarget = false;

            // HP Text
            Text hpText = CreateUIText("HPText", hpBarObj.transform, "HP: 500 / 500 (100%)", 16, Color.white, FontStyle.Bold, Vector2.zero, new Vector2(500, 24));

            // 2. Right Side Live Combat HUD (Real-time Live Stats)
            GameObject rightHud = CreateUIBox("RightCombatHUD", panel.transform, new Color(0.04f, 0.08f, 0.06f, 0.94f));
            RectTransform hudRT = rightHud.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(1f, 0.5f);
            hudRT.anchorMax = new Vector2(1f, 0.5f);
            hudRT.pivot = new Vector2(1f, 0.5f);
            hudRT.sizeDelta = new Vector2(275, 545);
            hudRT.anchoredPosition = new Vector2(-20, -30);

            // Sidebar Header Banner
            GameObject hudHeader = CreateUIBox("HudHeader", rightHud.transform, new Color(0.08f, 0.18f, 0.12f, 0.98f));
            RectTransform hhRT = hudHeader.GetComponent<RectTransform>();
            hhRT.anchorMin = new Vector2(0.5f, 1f);
            hhRT.anchorMax = new Vector2(0.5f, 1f);
            hhRT.pivot = new Vector2(0.5f, 1f);
            hhRT.anchoredPosition = new Vector2(0, -8);
            hhRT.sizeDelta = new Vector2(255, 38);
            CreateUIText("HudTitle", hudHeader.transform, "📊 STATUS MISI", 17, new Color(1f, 0.88f, 0.25f), FontStyle.Bold, Vector2.zero, new Vector2(250, 32));

            // Stat 1: ⏱️ TIMER COUNTDOWN
            GameObject timerBox = CreateUIBox("TimerBox", rightHud.transform, new Color(0.07f, 0.14f, 0.11f, 0.95f));
            RectTransform tboxRT = timerBox.GetComponent<RectTransform>();
            tboxRT.anchorMin = new Vector2(0.5f, 1f);
            tboxRT.anchorMax = new Vector2(0.5f, 1f);
            tboxRT.pivot = new Vector2(0.5f, 1f);
            tboxRT.anchoredPosition = new Vector2(0, -56);
            tboxRT.sizeDelta = new Vector2(255, 68);
            CreateUIText("TimerLbl", timerBox.transform, "⏱️ SISA WAKTU", 13, new Color(0.6f, 0.85f, 0.8f), FontStyle.Bold, new Vector2(0, 15), new Vector2(240, 22));
            Text timerText = CreateUIText("TimerText", timerBox.transform, "00:30", 26, new Color(0.4f, 0.95f, 0.95f), FontStyle.Bold, new Vector2(0, -11), new Vector2(240, 32));

            // Stat 2: 🎯 TURNS COUNTER
            GameObject turnsBox = CreateUIBox("TurnsBox", rightHud.transform, new Color(0.07f, 0.14f, 0.11f, 0.95f));
            RectTransform turnRT = turnsBox.GetComponent<RectTransform>();
            turnRT.anchorMin = new Vector2(0.5f, 1f);
            turnRT.anchorMax = new Vector2(0.5f, 1f);
            turnRT.pivot = new Vector2(0.5f, 1f);
            turnRT.anchoredPosition = new Vector2(0, -132);
            turnRT.sizeDelta = new Vector2(255, 68);
            CreateUIText("TurnsLbl", turnsBox.transform, "🎯 SISA LANGKAH", 13, new Color(0.85f, 0.85f, 0.85f), FontStyle.Bold, new Vector2(0, 15), new Vector2(240, 22));
            Text turnsText = CreateUIText("TurnsText", turnsBox.transform, "24 / 24", 24, Color.white, FontStyle.Bold, new Vector2(0, -11), new Vector2(240, 30));

            // Stat 3: 💥 TOTAL DAMAGE ACCUMULATED
            GameObject dmgBox = CreateUIBox("DmgBox", rightHud.transform, new Color(0.18f, 0.08f, 0.08f, 0.95f));
            RectTransform dmgRT = dmgBox.GetComponent<RectTransform>();
            dmgRT.anchorMin = new Vector2(0.5f, 1f);
            dmgRT.anchorMax = new Vector2(0.5f, 1f);
            dmgRT.pivot = new Vector2(0.5f, 1f);
            dmgRT.anchoredPosition = new Vector2(0, -208);
            dmgRT.sizeDelta = new Vector2(255, 68);
            CreateUIText("DmgLbl", dmgBox.transform, "💥 TOTAL DAMAGE", 13, new Color(1f, 0.65f, 0.65f), FontStyle.Bold, new Vector2(0, 15), new Vector2(240, 22));
            Text totalDmgText = CreateUIText("TotalDmgText", dmgBox.transform, "0 DMG", 24, new Color(1f, 0.38f, 0.38f), FontStyle.Bold, new Vector2(0, -11), new Vector2(240, 30));

            // Stat 4: ⚡ PATROL SPEED
            GameObject speedBox = CreateUIBox("SpeedBox", rightHud.transform, new Color(0.16f, 0.13f, 0.05f, 0.95f));
            RectTransform speedRT = speedBox.GetComponent<RectTransform>();
            speedRT.anchorMin = new Vector2(0.5f, 1f);
            speedRT.anchorMax = new Vector2(0.5f, 1f);
            speedRT.pivot = new Vector2(0.5f, 1f);
            speedRT.anchoredPosition = new Vector2(0, -284);
            speedRT.sizeDelta = new Vector2(255, 68);
            CreateUIText("SpeedLbl", speedBox.transform, "⚡ PATROL SPEED", 13, new Color(1f, 0.88f, 0.5f), FontStyle.Bold, new Vector2(0, 15), new Vector2(240, 22));
            Text patrolSpeedText = CreateUIText("PatrolSpeedText", speedBox.transform, "220 px/s", 22, new Color(1f, 0.82f, 0.2f), FontStyle.Bold, new Vector2(0, -11), new Vector2(240, 30));

            // Stat 5: 🌟 EXP GAINED
            GameObject expBox = CreateUIBox("ExpBox", rightHud.transform, new Color(0.07f, 0.15f, 0.09f, 0.95f));
            RectTransform expRT = expBox.GetComponent<RectTransform>();
            expRT.anchorMin = new Vector2(0.5f, 1f);
            expRT.anchorMax = new Vector2(0.5f, 1f);
            expRT.pivot = new Vector2(0.5f, 1f);
            expRT.anchoredPosition = new Vector2(0, -360);
            expRT.sizeDelta = new Vector2(255, 68);
            CreateUIText("ExpLbl", expBox.transform, "🌟 EXP TERKUMPUL", 13, new Color(0.65f, 1f, 0.65f), FontStyle.Bold, new Vector2(0, 15), new Vector2(240, 22));
            Text scoreText = CreateUIText("ScoreText", expBox.transform, "+0 EXP", 22, new Color(0.4f, 1f, 0.45f), FontStyle.Bold, new Vector2(0, -11), new Vector2(240, 30));

            // Exit Mission Button at the bottom of the Right Sidebar
            Button exitMissionBtn = CreateUIButton("BtnExitMission", rightHud.transform, "⬅️ KELUAR MISI", new Vector2(0, -450), new Vector2(255, 44), new Color(0.48f, 0.2f, 0.2f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.OpenStageMap();
            });
            RectTransform exitRT = exitMissionBtn.GetComponent<RectTransform>();
            exitRT.anchorMin = new Vector2(0.5f, 1f);
            exitRT.anchorMax = new Vector2(0.5f, 1f);
            exitRT.pivot = new Vector2(0.5f, 1f);

            Text comboText = CreateUIText("ComboBanner", panel.transform, "⚡ COMBO x2!", 32, new Color(1f, 0.92f, 0.2f), FontStyle.Bold, new Vector2(0, 480), new Vector2(500, 50));
            comboText.gameObject.SetActive(false);

            // 3. Enemy Character Sprite (Moving Boss with Hitbox)
            GameObject bossArtObj = new GameObject("BossTargetArt");
            bossArtObj.transform.SetParent(panel.transform, false);
            RectTransform artRT = bossArtObj.AddComponent<RectTransform>();
            artRT.sizeDelta = new Vector2(240, 240);
            artRT.anchoredPosition = new Vector2(0, 220);
            artRT.localScale = Vector3.one;
            artRT.localEulerAngles = Vector3.zero;
            Image enemyImg = bossArtObj.AddComponent<Image>();
            enemyImg.sprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man_idle");
            if (enemyImg.sprite == null) enemyImg.sprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man");
            enemyImg.preserveAspect = true;
            enemyImg.raycastTarget = false;

            // 3. Lower Match-3 Board Area (15x6 Grid)
            GameObject boardFrameObj = CreateUIBox("BoardFrame", panel.transform, new Color(0.06f, 0.12f, 0.09f, 0.96f));
            RectTransform boardFrameRT = boardFrameObj.GetComponent<RectTransform>();
            boardFrameRT.anchoredPosition = new Vector2(0, -250);
            boardFrameRT.sizeDelta = new Vector2(1260, 520);

            // Grid Container (Inside Board Frame: 15 columns x 76px + 14 * 6px = 1224px width, 6 rows * 76px + 5 * 6px = 486px height)
            GameObject gridContainerObj = new GameObject("GridContainer", typeof(RectTransform));
            gridContainerObj.transform.SetParent(boardFrameObj.transform, false);
            RectTransform gridRT = gridContainerObj.GetComponent<RectTransform>();
            gridRT.sizeDelta = new Vector2(1224, 486);
            gridRT.anchoredPosition = Vector2.zero;

            // Pre-spawn cell slot backings for clear visual grid layout
            for (int x = 0; x < Match3CombatManager.COLUMNS; x++)
            {
                for (int y = 0; y < Match3CombatManager.ROWS; y++)
                {
                    GameObject slotObj = new GameObject($"SlotBg_{x}_{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    slotObj.transform.SetParent(gridRT, false);
                    RectTransform sRT = slotObj.GetComponent<RectTransform>();
                    sRT.sizeDelta = new Vector2(Match3CombatManager.TILE_SIZE, Match3CombatManager.TILE_SIZE);
                    sRT.anchoredPosition = Match3CombatManager.Instance != null ?
                        Match3CombatManager.Instance.CalculateTilePosition(x, y) :
                        new Vector2(-574f + x * 82f, -205f + y * 82f);
                    Image slotImg = slotObj.GetComponent<Image>();
                    slotImg.color = new Color(0.12f, 0.2f, 0.16f, 0.72f);
                    slotImg.raycastTarget = false;
                }
            }

            // 4. Transparent Projectile & Damage Text Layers
            GameObject projLayerObj = new GameObject("ProjectileLayer", typeof(RectTransform));
            projLayerObj.transform.SetParent(panel.transform, false);
            RectTransform projLayerRT = projLayerObj.GetComponent<RectTransform>();
            projLayerRT.anchorMin = Vector2.zero;
            projLayerRT.anchorMax = Vector2.one;
            projLayerRT.offsetMin = Vector2.zero;
            projLayerRT.offsetMax = Vector2.zero;

            GameObject dmgLayerObj = new GameObject("DamageTextLayer", typeof(RectTransform));
            dmgLayerObj.transform.SetParent(panel.transform, false);
            RectTransform dmgLayerRT = dmgLayerObj.GetComponent<RectTransform>();
            dmgLayerRT.anchorMin = Vector2.zero;
            dmgLayerRT.anchorMax = Vector2.one;
            dmgLayerRT.offsetMin = Vector2.zero;
            dmgLayerRT.offsetMax = Vector2.zero;

            // 5. Victory Modal
            GameObject winModal = CreateFullScreenPanel("VictoryModal", panel.transform, new Color(0f, 0f, 0f, 0.82f));
            GameObject winCard = CreateUIBox("WinCard", winModal.transform, new Color(0.08f, 0.18f, 0.12f, 0.98f));
            RectTransform winCardRT = winCard.GetComponent<RectTransform>();
            winCardRT.sizeDelta = new Vector2(760, 520);
            winCardRT.anchoredPosition = Vector2.zero;

            CreateUIText("WinTitle", winCard.transform, "🎯 TARGET BERHASIL DIKALAHKAN!", 32, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 190), new Vector2(700, 50));
            Text winStatsText = CreateUIText("WinStats", winCard.transform, "Hasil kombinasimu berhasil memukul mundur lawan!", 20, Color.white, FontStyle.Normal, new Vector2(0, 10), new Vector2(660, 220));
            Button winContinueBtn = CreateUIButton("BtnWinContinue", winCard.transform, "LANJUTKAN MISI ▶", new Vector2(0, -180), new Vector2(360, 65), new Color(0.18f, 0.65f, 0.32f), null);
            winModal.SetActive(false);

            // 6. Defeat Modal
            GameObject loseModal = CreateFullScreenPanel("DefeatModal", panel.transform, new Color(0f, 0f, 0f, 0.82f));
            GameObject loseCard = CreateUIBox("LoseCard", loseModal.transform, new Color(0.18f, 0.08f, 0.08f, 0.98f));
            RectTransform loseCardRT = loseCard.GetComponent<RectTransform>();
            loseCardRT.sizeDelta = new Vector2(760, 480);
            loseCardRT.anchoredPosition = Vector2.zero;

            Text loseTitleText = CreateUIText("LoseTitle", loseCard.transform, "⚠️ LANGKAH HABIS!", 34, new Color(0.95f, 0.3f, 0.3f), FontStyle.Bold, new Vector2(0, 160), new Vector2(700, 50));
            Text loseDescText = CreateUIText("LoseDesc", loseCard.transform, "Doctor Sawit Man belum berhasil dipukul mundur.\nAtur kombinasi puzzle dan strategi yang lebih efektif untuk menumbangkannya!", 20, Color.white, FontStyle.Normal, new Vector2(0, 40), new Vector2(660, 120));
            Button loseRetryBtn = CreateUIButton("BtnLoseRetry", loseCard.transform, "🔄 COBA LAGI", new Vector2(-160, -140), new Vector2(280, 60), new Color(0.72f, 0.22f, 0.22f), null);
            Button loseExitBtn = CreateUIButton("BtnLoseExit", loseCard.transform, "🗺️ PETA MISI", new Vector2(160, -140), new Vector2(260, 60), new Color(0.3f, 0.35f, 0.4f), null);
            loseModal.SetActive(false);

            // Register with Match3CombatManager
            if (Match3CombatManager.Instance != null)
            {
                Match3CombatManager.Instance.SetupUI(
                    panel,
                    gridRT,
                    projLayerRT,
                    dmgLayerRT,
                    titleText,
                    totalDmgText,
                    turnsText,
                    timerText,
                    patrolSpeedText,
                    scoreText,
                    comboText,
                    exitMissionBtn,
                    enemyImg,
                    hpFillImg,
                    hpText,
                    enemyNameText,
                    winModal,
                    winStatsText,
                    winContinueBtn,
                    loseModal,
                    loseTitleText,
                    loseDescText,
                    loseRetryBtn,
                    loseExitBtn
                );
            }

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildBossPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("BossFightPanel", parent, new Color(0.18f, 0.08f, 0.08f, 1f));
            Image bgImg = panel.GetComponent<Image>();
            Sprite bossBg = Resources.Load<Sprite>("Environtment/scene_sawit_man");
            if (bossBg != null) { bgImg.sprite = bossBg; bgImg.color = Color.white; }
            bgImg.raycastTarget = false;

            CreateUIText("BossTitle", panel.transform, "KONFRONTASI: DOCTOR SAWIT MAN", 44, new Color(0.95f, 0.3f, 0.3f), FontStyle.Bold, new Vector2(0, 440), new Vector2(1200, 60));

            // Doctor Sawit Man Character Artwork
            GameObject bossArtObj = new GameObject("BossArt");
            bossArtObj.transform.SetParent(panel.transform, false);
            RectTransform artRT = bossArtObj.AddComponent<RectTransform>();
            artRT.sizeDelta = new Vector2(480, 600);
            artRT.anchoredPosition = new Vector2(0, 70);
            bossArtImage = bossArtObj.AddComponent<Image>();
            bossArtImage.sprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man");
            bossArtImage.preserveAspect = true;
            bossArtImage.raycastTarget = false;

            // Confrontation Dialogue Box
            GameObject cardObj = CreateUIBox("BossBox", panel.transform, new Color(0.1f, 0.05f, 0.05f, 0.95f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(1200, 240);
            cardRT.anchoredPosition = new Vector2(0, -320);

            bossQuoteText = CreateUIText("BossQuote", cardObj.transform, "\"Hutan ini hanya lahan kosong! Industri kami adalah masa depan kemajuan!\"", 24, Color.white, FontStyle.Italic, new Vector2(0, 40), new Vector2(1100, 80));

            bossEvidenceBtn1 = CreateUIButton("BtnEvidence1", cardObj.transform, "⚖️ Sodorkan Sampel Tanah & Drum Forensik!", new Vector2(-280, -45), new Vector2(520, 65), new Color(0.75f, 0.22f, 0.22f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.SubmitBossEvidence(1);
            });

            bossEvidenceBtn2 = CreateUIButton("BtnEvidence2", cardObj.transform, "📜 Paparkan Data Kepunahan Satwa & Karbon!", new Vector2(280, -45), new Vector2(520, 65), new Color(0.18f, 0.45f, 0.65f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.SubmitBossEvidence(2);
            });

            panel.SetActive(false);
            return panel;
        }

        public void SetupBossFight(int stage)
        {
            if (bossPanel == null) return;
            bossPanel.SetActive(true);

            if (bossArtImage != null)
            {
                bossArtImage.color = Color.white;
            }

            if (bossQuoteText != null)
            {
                if (stage == 1)
                {
                    bossQuoteText.text = "\"Kalian sudah mengumpulkan cukup banyak catatan. Hutan ini hanya lahan kosong untuk kemajuan industri kami!\"";
                }
                else if (stage == 2)
                {
                    bossQuoteText.text = "\"Kalian masih mengikuti jejak itu? Satu kawasan rusak bukan berarti kalian bisa menghentikan rencana kami!\"";
                }
                else
                {
                    bossQuoteText.text = "\"Kalian berhasil sampai sejauh ini... Tapi kalian tidak tahu kekuatan apa yang kalian hadapi di perkebunan ini!\"";
                }
            }

            if (bossEvidenceBtn1 != null)
            {
                bossEvidenceBtn1.interactable = true;
                bossEvidenceBtn1.gameObject.SetActive(true);
            }
            if (bossEvidenceBtn2 != null)
            {
                bossEvidenceBtn2.interactable = true;
                bossEvidenceBtn2.gameObject.SetActive(true);
            }
        }

        public void ShowBossDefeatedResponse(int stage, Action onComplete)
        {
            if (bossEvidenceBtn1 != null) bossEvidenceBtn1.interactable = false;
            if (bossEvidenceBtn2 != null) bossEvidenceBtn2.interactable = false;

            if (bossArtImage != null)
            {
                bossArtImage.color = new Color(0.85f, 0.7f, 0.7f, 1f);
            }

            if (bossQuoteText != null)
            {
                if (stage == 1)
                {
                    bossQuoteText.text = "💥 DOCTOR SAWIT MAN TERPOJOK!\n\"Kalian belum memahami seluruh ceritanya... Tapi bukti ini tidak bisa kusangkal!\"";
                }
                else if (stage == 2)
                {
                    bossQuoteText.text = "💥 BUKTI PERUSAKAN HAKIKI DIBONGKAR!\n\"Kalian semakin dekat... Aku harus mundur untuk saat ini!\"";
                }
                else
                {
                    bossQuoteText.text = "🏆 DOCTOR SAWIT MAN MENYERAH!\n\"Aku mengakui tindakanku telah menyebabkan kehancuran... Aku bersedia bertanggung jawab.\"";
                }
            }

            StartCoroutine(BossDefeatDelayCoroutine(onComplete));
        }

        private System.Collections.IEnumerator BossDefeatDelayCoroutine(Action onComplete)
        {
            yield return new WaitForSeconds(2.0f);
            if (bossPanel != null) bossPanel.SetActive(false);
            onComplete?.Invoke();
        }

        private GameObject BuildEndingPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("EndingPanel", parent, new Color(0.06f, 0.15f, 0.1f, 1f));
            Image bgImg = panel.GetComponent<Image>();
            Sprite victoryBg = Resources.Load<Sprite>("Environtment/scene_victory");
            if (victoryBg != null) { bgImg.sprite = victoryBg; bgImg.color = Color.white; }

            CreateUIText("VictoryTitle", panel.transform, "MISI SELESAI!", 60, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 380), new Vector2(1000, 80));
            CreateUIText("VictorySubtitle", panel.transform, "EKOSISTEM NUSANTARA BERHASIL DISELAMATKAN", 28, Color.white, FontStyle.Bold, new Vector2(0, 310), new Vector2(1200, 50));

            // Certificate Box
            GameObject certObj = CreateUIBox("CertificateBox", panel.transform, new Color(0.12f, 0.22f, 0.16f, 0.95f));
            RectTransform certRT = certObj.GetComponent<RectTransform>();
            certRT.sizeDelta = new Vector2(1000, 380);
            certRT.anchoredPosition = new Vector2(0, 30);

            CreateUIText("CertHeading", certObj.transform, "PIAGAM PENJAGA RIMBA KEHORMATAN C.A.R.E.", 28, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 130), new Vector2(900, 50));
            string certText = "Diberikan kepada Penjelajah Konservasi atas dedikasi dan keberanian dalam memadamkan kebakaran, mengungkap perusakan ilegal oleh Doctor Sawit Man, serta merehabilitasi Rusa Timor, Orangutan, dan Gajah Asia kembali ke habitat aslinya.";
            CreateUIText("CertBody", certObj.transform, certText, 22, Color.white, FontStyle.Normal, new Vector2(0, 10), new Vector2(900, 160));

            CreateUIButton("BtnEndingLobby", panel.transform, "🐾 KEMBALI KE LOBBY RESTORASI", new Vector2(-240, -320), new Vector2(420, 70), new Color(0.18f, 0.65f, 0.32f), () =>
            {
                UpdateLobbyUI();
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
            });

            CreateUIButton("BtnEndingTitle", panel.transform, "🏠 MENU UTAMA", new Vector2(240, -320), new Vector2(360, 70), new Color(0.25f, 0.35f, 0.45f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowTitleScreen();
            });

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildEncyclopediaModal(Transform parent)
        {
            GameObject modal = CreateFullScreenPanel("EncyclopediaModal", parent, new Color(0f, 0f, 0f, 0.85f));

            // Content Card
            GameObject cardObj = CreateUIBox("ModalCard", modal.transform, new Color(0.1f, 0.16f, 0.12f, 0.98f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(1200, 780);
            cardRT.anchoredPosition = Vector2.zero;

            // Names
            Text commonNameText = CreateUIText("CommonName", cardObj.transform, "NAMA SATWA", 38, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(-200, 320), new Vector2(700, 50));
            Text scientificNameText = CreateUIText("ScientificName", cardObj.transform, "Scientific Name", 24, new Color(0.7f, 0.9f, 0.7f), FontStyle.Italic, new Vector2(-200, 270), new Vector2(700, 40));

            // Close button (X)
            Button closeBtn = CreateUIButton("BtnClose", cardObj.transform, "✕ TUTUP", new Vector2(480, 320), new Vector2(160, 50), new Color(0.6f, 0.2f, 0.2f), null);

            // Left Image Preview
            GameObject previewObj = new GameObject("AnimalPreviewImage");
            previewObj.transform.SetParent(cardObj.transform, false);
            RectTransform prRT = previewObj.AddComponent<RectTransform>();
            prRT.sizeDelta = new Vector2(360, 360);
            prRT.anchoredPosition = new Vector2(-360, 40);
            Image previewImg = previewObj.AddComponent<Image>();
            previewImg.preserveAspect = true;

            // Locked Overlay (on image)
            GameObject lockedObj = CreateFullScreenPanel("LockedOverlay", previewObj.transform, new Color(0f, 0f, 0f, 0.7f));
            CreateUIText("LockText", lockedObj.transform, "🔒\nBELUM DISELEMATKAN", 26, new Color(0.95f, 0.4f, 0.4f), FontStyle.Bold, Vector2.zero, new Vector2(300, 100));

            // Quote Text
            Text quoteText = CreateUIText("OpeningQuote", cardObj.transform, "\"Penjaga rimba...\"", 22, new Color(0.85f, 0.95f, 0.85f), FontStyle.Italic, new Vector2(160, 160), new Vector2(640, 60));

            // 3 Tabs
            Button tabTax = CreateUIButton("TabTaxonomy", cardObj.transform, "🏷️ Taksonomi", new Vector2(-40, 80), new Vector2(200, 45), new Color(0.2f, 0.45f, 0.35f), null);
            Button tabHab = CreateUIButton("TabHabitat", cardObj.transform, "🌲 Habitat", new Vector2(170, 80), new Vector2(200, 45), new Color(0.2f, 0.45f, 0.35f), null);
            Button tabDiet = CreateUIButton("TabDiet", cardObj.transform, "🌿 Pakan", new Vector2(380, 80), new Vector2(200, 45), new Color(0.2f, 0.45f, 0.35f), null);

            // Content Body Box
            GameObject contentBox = CreateUIBox("ContentBox", cardObj.transform, new Color(0.06f, 0.1f, 0.08f, 0.95f));
            RectTransform cboxRT = contentBox.GetComponent<RectTransform>();
            cboxRT.sizeDelta = new Vector2(660, 320);
            cboxRT.anchoredPosition = new Vector2(170, -110);

            Text bodyText = CreateUIText("ContentBody", contentBox.transform, "Memuat data ensiklopedia...", 22, Color.white, FontStyle.Normal, Vector2.zero, new Vector2(600, 280));
            bodyText.alignment = TextAnchor.UpperLeft;

            // Register with EncyclopediaManager
            if (EncyclopediaManager.Instance != null)
            {
                EncyclopediaManager.Instance.SetupUI(
                    modal,
                    commonNameText,
                    scientificNameText,
                    quoteText,
                    bodyText,
                    previewImg,
                    lockedObj,
                    tabTax,
                    tabHab,
                    tabDiet,
                    closeBtn
                );
            }

            modal.SetActive(false);
            return modal;
        }

        #endregion

        #region UI Factory Helpers

        private GameObject CreateFullScreenPanel(string name, Transform parent, Color bgColor)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;
            return obj;
        }

        private GameObject CreateUIBox(string name, Transform parent, Color bgColor)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = false;
            return obj;
        }

        private Text CreateUIText(string name, Transform parent, string content, int fontSize, Color color, FontStyle style, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = gameFont;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }

        private Button CreateUIButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Color bgColor, Action onClick)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = new Color(Mathf.Min(1f, bgColor.r * 1.35f), Mathf.Min(1f, bgColor.g * 1.35f), Mathf.Min(1f, bgColor.b * 1.35f), 1f);
            cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
            cb.selectedColor = bgColor;
            cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            btn.colors = cb;

            Text txt = CreateUIText("Label", obj.transform, label, (int)(size.y * 0.38f), Color.white, FontStyle.Bold, Vector2.zero, size);
            txt.raycastTarget = false;

            if (onClick != null)
            {
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[C.A.R.E. UI] Tombol '{name}' berhasil diklik!");
                    if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                    onClick.Invoke();
                });
            }

            return btn;
        }

        #endregion
    }
}
