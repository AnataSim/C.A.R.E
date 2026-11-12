using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CARE.Story;
using CARE.UI;
using CARE.Gameplay;
using CARE.Audio;

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

        // Lobby dynamic UI
        private Text lobbyProgressText;
        private Image rusaCardImage;
        private Text rusaCardStatus;
        private Image orangutanCardImage;
        private Text orangutanCardStatus;
        private Image gajahCardImage;
        private Text gajahCardStatus;

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

            // Ensure Audio Manager
            EnsureAudioManager();

            // Ensure Core Managers
            EnsureGameFlowManager();
            EnsureDialogueManager();
            EnsureEncyclopediaManager();
            EnsureHiddenObjectManager();

            // Setup Canvas & UI Hierarchy
            SetupCanvasAndUI();
        }

        private Font GetDefaultFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch {}

            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch {}
            }

            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            return font;
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
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

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
            Transform playBtn = panel.Find("Button_Play");
            if (playBtn != null)
            {
                Button btn = playBtn.GetComponent<Button>();
                if (btn == null) btn = playBtn.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[C.A.R.E.] Tombol Mulai Petualangan diklik!");
                    if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                    if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartOpeningStory();
                });
            }

            Transform groupBtn = panel.Find("Button_Group");
            if (groupBtn != null)
            {
                Button btn = groupBtn.GetComponent<Button>();
                if (btn == null) btn = groupBtn.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[C.A.R.E.] Tombol Lobby Restorasi diklik!");
                    if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                    if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
                });
            }
        }

        private GameObject BuildTitlePanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("TitlePanel", parent, new Color(0.08f, 0.14f, 0.1f, 1f));
            CreateUIText("TitleText", panel.transform, "C.A.R.E.", 80, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 150), new Vector2(900, 120));
            CreateUIText("SubtitleText", panel.transform, "Cegah. Api. Rawat. Ekosistem.", 28, Color.white, FontStyle.Normal, new Vector2(0, 60), new Vector2(900, 60));

            CreateUIButton("BtnPlay", panel.transform, "▶ MULAI PETUALANGAN", new Vector2(0, -60), new Vector2(360, 65), new Color(0.18f, 0.65f, 0.32f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartOpeningStory();
            });

            CreateUIButton("BtnLobby", panel.transform, "🐾 LOBBY RESTORASI", new Vector2(0, -145), new Vector2(360, 60), new Color(0.2f, 0.45f, 0.65f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowLobbyRestorasi();
            });

            return panel;
        }

        private GameObject BuildDialoguePanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("DialoguePanel", parent, new Color(0.05f, 0.08f, 0.06f, 0.96f));

            // Background scenery art if available
            Image bgImg = panel.GetComponent<Image>();
            Sprite forestBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (forestBg != null)
            {
                bgImg.sprite = forestBg;
                bgImg.color = new Color(0.55f, 0.55f, 0.55f, 1f);
            }

            // Large Portrait (Left/Center)
            GameObject portraitObj = new GameObject("PortraitImage");
            portraitObj.transform.SetParent(panel.transform, false);
            RectTransform portraitRT = portraitObj.AddComponent<RectTransform>();
            portraitRT.anchorMin = new Vector2(0.06f, 0.32f);
            portraitRT.anchorMax = new Vector2(0.36f, 0.92f);
            portraitRT.offsetMin = Vector2.zero;
            portraitRT.offsetMax = Vector2.zero;
            Image portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.preserveAspect = true;

            // Dialogue Box (Bottom Full-Width)
            GameObject boxObj = CreateUIBox("DialogueBox", panel.transform, new Color(0.08f, 0.12f, 0.1f, 0.95f));
            RectTransform boxRT = boxObj.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.04f, 0.04f);
            boxRT.anchorMax = new Vector2(0.96f, 0.32f);
            boxRT.offsetMin = Vector2.zero;
            boxRT.offsetMax = Vector2.zero;

            // Speaker Nameplate
            Text speakerText = CreateUIText("SpeakerName", boxObj.transform, "NARATOR", 28, new Color(0.98f, 0.82f, 0.28f), FontStyle.Bold, new Vector2(30, -30), new Vector2(600, 45));
            RectTransform spkRT = speakerText.GetComponent<RectTransform>();
            spkRT.anchorMin = new Vector2(0f, 1f);
            spkRT.anchorMax = new Vector2(0f, 1f);
            spkRT.pivot = new Vector2(0f, 1f);
            speakerText.alignment = TextAnchor.MiddleLeft;

            // Dialogue Body Text
            Text bodyText = CreateUIText("DialogueBody", boxObj.transform, "Menyiapkan narasi...", 24, Color.white, FontStyle.Normal, new Vector2(30, -85), new Vector2(1000, 160));
            RectTransform bodyRT = bodyText.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 1f);
            bodyRT.anchorMax = new Vector2(1f, 0f);
            bodyRT.pivot = new Vector2(0f, 1f);
            bodyRT.offsetMin = new Vector2(35, 25);
            bodyRT.offsetMax = new Vector2(-35, -80);
            bodyText.alignment = TextAnchor.UpperLeft;

            // Continue Indicator
            Text continueIndicator = CreateUIText("ContinueIndicator", boxObj.transform, "Tekan [SPASI] atau Klik untuk lanjut ▶", 20, new Color(0.7f, 0.95f, 0.7f), FontStyle.Italic, new Vector2(-30, 25), new Vector2(450, 35));
            RectTransform ciRT = continueIndicator.GetComponent<RectTransform>();
            ciRT.anchorMin = new Vector2(1f, 0f);
            ciRT.anchorMax = new Vector2(1f, 0f);
            ciRT.pivot = new Vector2(1f, 0f);
            continueIndicator.alignment = TextAnchor.MiddleRight;

            // Clickable continue button covering the box
            Button continueBtn = boxObj.AddComponent<Button>();

            // Register with DialogueManager
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.SetupUI(panel, speakerText, bodyText, portraitImg, continueBtn, continueIndicator.gameObject);
            }

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildLobbyPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("LobbyRestorasiPanel", parent, new Color(0.08f, 0.16f, 0.11f, 1f));

            // Background
            Image bgImg = panel.GetComponent<Image>();
            Sprite forestBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (forestBg != null) { bgImg.sprite = forestBg; bgImg.color = new Color(0.4f, 0.4f, 0.4f, 1f); }

            // Header
            CreateUIText("LobbyTitle", panel.transform, "LOBBY RESTORASI EKOSISTEM", 46, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 470), new Vector2(1200, 60));
            CreateUIText("LobbySubtitle", panel.transform, "Pusat Konservasi, Evakuasi, dan Rehabilitasi Satwa Liar", 22, new Color(0.85f, 0.95f, 0.85f), FontStyle.Normal, new Vector2(0, 420), new Vector2(1200, 40));

            lobbyProgressText = CreateUIText("ProgressText", panel.transform, "Satwa Terselamatkan: 0 / 3", 26, new Color(0.4f, 0.95f, 0.5f), FontStyle.Bold, new Vector2(0, 360), new Vector2(600, 40));

            // 3 Habitat Cards Container
            GameObject cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);
            RectTransform ccRT = cardsContainer.AddComponent<RectTransform>();
            ccRT.sizeDelta = new Vector2(1400, 520);
            ccRT.anchoredPosition = new Vector2(0, 40);

            // Card 1: Rusa Timor
            BuildAnimalCard(cardsContainer.transform, -450, "rusa_timor", "Rusa Timor", "Stage 1", out rusaCardImage, out rusaCardStatus);

            // Card 2: Orangutan
            BuildAnimalCard(cardsContainer.transform, 0, "orangutan", "Orangutan", "Stage 2", out orangutanCardImage, out orangutanCardStatus);

            // Card 3: Gajah Asia
            BuildAnimalCard(cardsContainer.transform, 450, "gajah_asia", "Gajah Asia", "Stage 3", out gajahCardImage, out gajahCardStatus);

            // Bottom Buttons
            CreateUIButton("BtnMulaiMisi", panel.transform, "🗺️ MULAI MISI (PETA TAHAPAN)", new Vector2(0, -390), new Vector2(460, 75), new Color(0.18f, 0.65f, 0.32f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.OpenStageMap();
            });

            CreateUIButton("BtnMenuUtama", panel.transform, "🏠 MENU UTAMA", new Vector2(-540, -390), new Vector2(260, 60), new Color(0.28f, 0.35f, 0.38f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.ShowTitleScreen();
            });

            CreateUIButton("BtnEnsiklopediaAll", panel.transform, "📖 ENSIKLOPEDIA", new Vector2(540, -390), new Vector2(260, 60), new Color(0.2f, 0.45f, 0.65f), () =>
            {
                if (EncyclopediaManager.Instance != null) EncyclopediaManager.Instance.OpenEncyclopedia("rusa_timor");
            });

            panel.SetActive(false);
            return panel;
        }

        private void BuildAnimalCard(Transform container, float posX, string animalKey, string animalName, string stageTag, out Image cardImg, out Text statusTxt)
        {
            GameObject cardObj = CreateUIBox("Card_" + animalKey, container, new Color(0.12f, 0.2f, 0.15f, 0.95f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(380, 480);
            cardRT.anchoredPosition = new Vector2(posX, 0);

            // Animal Preview Image
            GameObject imgObj = new GameObject("AnimalPreview");
            imgObj.transform.SetParent(cardObj.transform, false);
            RectTransform imgRT = imgObj.AddComponent<RectTransform>();
            imgRT.sizeDelta = new Vector2(280, 240);
            imgRT.anchoredPosition = new Vector2(0, 80);
            cardImg = imgObj.AddComponent<Image>();
            cardImg.preserveAspect = true;

            // Name
            CreateUIText("AnimalName", cardObj.transform, animalName, 26, Color.white, FontStyle.Bold, new Vector2(0, -65), new Vector2(340, 40));

            // Status
            statusTxt = CreateUIText("AnimalStatus", cardObj.transform, "🔒 Terancam di " + stageTag, 20, new Color(0.95f, 0.5f, 0.4f), FontStyle.Normal, new Vector2(0, -105), new Vector2(340, 35));

            // Button View Encyclopedia
            string key = animalKey;
            CreateUIButton("BtnView", cardObj.transform, "📖 Buka Info Satwa", new Vector2(0, -170), new Vector2(280, 50), new Color(0.2f, 0.5f, 0.4f), () =>
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

            CreateUIText("MapTitle", panel.transform, "PETA EKOSISTEM & MISI PENYELAMATAN", 44, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 440), new Vector2(1300, 60));
            CreateUIText("MapSubtitle", panel.transform, "Pilih kawasan terdampak untuk memulai investigasi dan rehabilitasi satwa", 22, Color.white, FontStyle.Normal, new Vector2(0, 390), new Vector2(1300, 40));

            // Stage 1 Card
            BuildStageCard(panel.transform, -450, "STAGE 1", "HUTAN DATARAN RENDAH", "Target: Rusa Timor\nInvestigasi Area Hutan Terbakar", 1, new Color(0.18f, 0.55f, 0.3f));

            // Stage 2 Card
            BuildStageCard(panel.transform, 0, "STAGE 2", "HUTAN RAWA GAMBUT", "Target: Orangutan\nInvestigasi Kanal & Pembalakan Ilegal", 2, new Color(0.55f, 0.4f, 0.15f));

            // Stage 3 Card
            BuildStageCard(panel.transform, 450, "STAGE 3", "KORIDOR PEGUNUNGAN", "Target: Gajah Asia & Doctor Sawit Man\nEvakuasi & Konfrontasi Terakhir", 3, new Color(0.65f, 0.2f, 0.2f));

            // Back button
            CreateUIButton("BtnBackLobby", panel.transform, "⬅️ KEMBALI KE LOBBY", new Vector2(0, -410), new Vector2(360, 65), new Color(0.3f, 0.35f, 0.4f), () =>
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
            cardRT.sizeDelta = new Vector2(390, 520);
            cardRT.anchoredPosition = new Vector2(posX, 20);

            CreateUIText("Tag", cardObj.transform, tag, 28, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 200), new Vector2(350, 40));
            CreateUIText("Title", cardObj.transform, title, 24, Color.white, FontStyle.Bold, new Vector2(0, 140), new Vector2(350, 60));
            CreateUIText("Desc", cardObj.transform, desc, 20, new Color(0.85f, 0.9f, 0.85f), FontStyle.Normal, new Vector2(0, 30), new Vector2(340, 120));

            int sNum = stageNum;
            CreateUIButton("BtnStartStage", cardObj.transform, "▶ MULAI TAHAP INI", new Vector2(0, -180), new Vector2(320, 65), themeColor, () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.StartStage(sNum);
            });
        }

        private GameObject BuildHiddenObjectPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("HiddenObjectPanel", parent, new Color(0.08f, 0.12f, 0.1f, 1f));

            // Background
            Image bgImg = panel.GetComponent<Image>();
            Sprite forestBg = Resources.Load<Sprite>("Environtment/image 2614");
            if (forestBg != null) { bgImg.sprite = forestBg; bgImg.color = new Color(0.5f, 0.5f, 0.5f, 1f); }

            // Top Header Bar
            GameObject headerBar = CreateUIBox("HeaderBar", panel.transform, new Color(0.05f, 0.08f, 0.06f, 0.92f));
            RectTransform headerRT = headerBar.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 0.9f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            Text titleText = CreateUIText("LocationTitle", headerBar.transform, "AREA INVESTIGASI: TEPI HUTAN", 28, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(40, 0), new Vector2(900, 50));
            RectTransform ttRT = titleText.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(0f, 0.5f);
            ttRT.anchorMax = new Vector2(0f, 0.5f);
            ttRT.pivot = new Vector2(0f, 0.5f);
            titleText.alignment = TextAnchor.MiddleLeft;

            CreateUIButton("BtnExitMission", headerBar.transform, "⬅️ Keluar Misi", new Vector2(-40, 0), new Vector2(220, 50), new Color(0.4f, 0.25f, 0.25f), () =>
            {
                if (GameFlowManager.Instance != null) GameFlowManager.Instance.OpenStageMap();
            });

            // Checklist container (Under header)
            GameObject listContainerObj = new GameObject("ChecklistContainer");
            listContainerObj.transform.SetParent(panel.transform, false);
            RectTransform lcRT = listContainerObj.AddComponent<RectTransform>();
            lcRT.anchorMin = new Vector2(0.05f, 0.82f);
            lcRT.anchorMax = new Vector2(0.95f, 0.9f);
            lcRT.offsetMin = Vector2.zero;
            lcRT.offsetMax = Vector2.zero;
            HorizontalLayoutGroup hlg = listContainerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Playfield Spawn Area (Where items are placed on screen)
            GameObject spawnAreaObj = new GameObject("ItemSpawnArea");
            spawnAreaObj.transform.SetParent(panel.transform, false);
            RectTransform saRT = spawnAreaObj.AddComponent<RectTransform>();
            saRT.anchorMin = new Vector2(0.08f, 0.08f);
            saRT.anchorMax = new Vector2(0.92f, 0.8f);
            saRT.offsetMin = Vector2.zero;
            saRT.offsetMax = Vector2.zero;

            // Educational Popup Modal
            GameObject popupPanel = CreateFullScreenPanel("ItemFoundPopup", panel.transform, new Color(0f, 0f, 0f, 0.78f));

            GameObject popupCard = CreateUIBox("PopupCard", popupPanel.transform, new Color(0.12f, 0.18f, 0.15f, 0.98f));
            RectTransform cardRT = popupCard.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(740, 540);
            cardRT.anchoredPosition = Vector2.zero;

            Text popupTitle = CreateUIText("PopupTitle", popupCard.transform, "OBJEK DITEMUKAN!", 30, new Color(0.95f, 0.85f, 0.3f), FontStyle.Bold, new Vector2(0, 205), new Vector2(650, 50));

            GameObject popImgObj = new GameObject("PopupItemImage");
            popImgObj.transform.SetParent(popupCard.transform, false);
            RectTransform popImgRT = popImgObj.AddComponent<RectTransform>();
            popImgRT.sizeDelta = new Vector2(160, 160);
            popImgRT.anchoredPosition = new Vector2(0, 95);
            Image popupImg = popImgObj.AddComponent<Image>();
            popupImg.preserveAspect = true;

            Text popupNote = CreateUIText("PopupNote", popupCard.transform, "Catatan edukasi...", 22, Color.white, FontStyle.Normal, new Vector2(0, -55), new Vector2(640, 140));

            Button popupContinueBtn = CreateUIButton("BtnPopupContinue", popupCard.transform, "LANJUT INVESTIGASI ▶", new Vector2(0, -195), new Vector2(360, 60), new Color(0.18f, 0.65f, 0.32f), null);

            popupPanel.SetActive(false);

            // Register with HiddenObjectManager
            if (HiddenObjectManager.Instance != null)
            {
                HiddenObjectManager.Instance.SetupUI(
                    panel,
                    titleText,
                    listContainerObj.transform,
                    spawnAreaObj.transform,
                    popupPanel,
                    popupTitle,
                    popupNote,
                    popupImg,
                    popupContinueBtn
                );
            }

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildBossPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("BossFightPanel", parent, new Color(0.18f, 0.08f, 0.08f, 1f));

            CreateUIText("BossTitle", panel.transform, "KONFRONTASI: DOCTOR SAWIT MAN", 46, new Color(0.95f, 0.3f, 0.3f), FontStyle.Bold, new Vector2(0, 440), new Vector2(1200, 60));

            // Doctor Sawit Man Character Artwork
            GameObject bossArtObj = new GameObject("BossArt");
            bossArtObj.transform.SetParent(panel.transform, false);
            RectTransform artRT = bossArtObj.AddComponent<RectTransform>();
            artRT.sizeDelta = new Vector2(480, 600);
            artRT.anchoredPosition = new Vector2(0, 70);
            Image bossImg = bossArtObj.AddComponent<Image>();
            bossImg.sprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man");
            bossImg.preserveAspect = true;

            // Confrontation Dialogue Box
            GameObject cardObj = CreateUIBox("BossBox", panel.transform, new Color(0.1f, 0.05f, 0.05f, 0.95f));
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(1200, 240);
            cardRT.anchoredPosition = new Vector2(0, -320);

            CreateUIText("BossQuote", cardObj.transform, "\"Hutan ini hanya lahan kosong! Industri kami adalah masa depan kemajuan!\"", 24, Color.white, FontStyle.Italic, new Vector2(0, 40), new Vector2(1100, 80));

            CreateUIButton("BtnEvidence1", cardObj.transform, "⚖️ Sodorkan Sampel Tanah & Drum Forensik!", new Vector2(-280, -45), new Vector2(500, 60), new Color(0.75f, 0.25f, 0.25f), () =>
            {
                if (DialogueManager.Instance != null) DialogueManager.Instance.OnContinuePressed();
            });

            CreateUIButton("BtnEvidence2", cardObj.transform, "📜 Paparkan Data Kepunahan Satwa & Karbon!", new Vector2(280, -45), new Vector2(500, 60), new Color(0.2f, 0.5f, 0.65f), () =>
            {
                if (DialogueManager.Instance != null) DialogueManager.Instance.OnContinuePressed();
            });

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildEndingPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenPanel("EndingPanel", parent, new Color(0.06f, 0.15f, 0.1f, 1f));

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

            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = new Color(Mathf.Min(1f, bgColor.r * 1.25f), Mathf.Min(1f, bgColor.g * 1.25f), Mathf.Min(1f, bgColor.b * 1.25f), 1f);
            cb.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = cb;

            Text txt = CreateUIText("Label", obj.transform, label, (int)(size.y * 0.42f), Color.white, FontStyle.Bold, Vector2.zero, size);

            if (onClick != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (CareAudioManager.Instance != null) CareAudioManager.Instance.PlayTransitionSFX();
                    onClick.Invoke();
                });
            }

            return btn;
        }

        #endregion
    }
}
