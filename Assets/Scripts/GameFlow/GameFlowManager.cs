using System.Collections.Generic;
using UnityEngine;
using CARE.Story;
using CARE.UI;
using CARE.Gameplay;
using CARE.Audio;
using CARE.Data;

namespace CARE.Core
{
    public enum GameState
    {
        TitleScreen,
        OpeningStory,
        LobbyRestorasi,
        StageMap,
        HiddenObjectLevel,
        BossFight,
        Ending
    }

    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        [Header("State Tracker")]
        public GameState currentState = GameState.TitleScreen;
        public int currentStage = 1;
        public int currentLevel = 1;

        [Header("Screen Panels")]
        [SerializeField] private GameObject titleScreenPanel;
        [SerializeField] private GameObject openingStoryPanel;
        [SerializeField] private GameObject lobbyRestorasiPanel;
        [SerializeField] private GameObject stageMapPanel;
        [SerializeField] private GameObject hiddenObjectPanel;
        [SerializeField] private GameObject bossFightPanel;
        [SerializeField] private GameObject endingPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetupPanels(GameObject title, GameObject story, GameObject lobby, GameObject stageMap, GameObject hiddenObj, GameObject boss, GameObject ending)
        {
            titleScreenPanel = title;
            openingStoryPanel = story;
            lobbyRestorasiPanel = lobby;
            stageMapPanel = stageMap;
            hiddenObjectPanel = hiddenObj;
            bossFightPanel = boss;
            endingPanel = ending;
        }

        private void Start()
        {
            ShowTitleScreen();
        }

        public void ShowTitleScreen()
        {
            currentState = GameState.TitleScreen;
            HideAllScreens();
            if (titleScreenPanel != null) titleScreenPanel.SetActive(true);

            // BGM Main Menu
            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayStorySadBGM();
                CareAudioManager.Instance.StopAmbience();
            }

            // Perbarui visual dan interaktivitas tombol Lobby sesuai progress local storage
            if (CareGameBootstrap.Instance != null)
            {
                CareGameBootstrap.Instance.UpdateTitleScreenLobbyButton();
            }
        }

        /// <summary>
        /// Dipanggil dari tombol PLAY di Title Screen.
        /// </summary>
        public void StartOpeningStory()
        {
            currentState = GameState.OpeningStory;
            HideAllScreens();
            if (openingStoryPanel != null) openingStoryPanel.SetActive(true);

            // Audio dasar sesuai narasi.txt: BGM Story Sad + Ambience Forest Fire loop
            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayStorySadBGM();
                CareAudioManager.Instance.PlayFireAmbience();
            }

            // Jalankan urutan dialog Scene 01 s/d Scene 05
            List<DialogueLine> fullOpening = new List<DialogueLine>();
            fullOpening.AddRange(CareStoryDatabase.Scene01_HutanTerbakar);
            fullOpening.AddRange(CareStoryDatabase.Scene02_SatwaTerdampak);
            fullOpening.AddRange(CareStoryDatabase.Scene03_PenjelajahTiba);
            fullOpening.AddRange(CareStoryDatabase.Scene04_SatwaTerluka);
            fullOpening.AddRange(CareStoryDatabase.Scene05_MisiDimulai);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(fullOpening, () =>
                {
                    // Setelah seluruh narasi awal selesai:
                    // 1. Simpan status bahwa prologue telah selesai ke Local Storage
                    if (CareSaveManager.Instance != null)
                    {
                        CareSaveManager.Instance.SetPrologueCompleted(true);
                    }

                    // 2. Buka gembok tombol Lobby Restorasi di Title Screen
                    if (CareGameBootstrap.Instance != null)
                    {
                        CareGameBootstrap.Instance.UpdateTitleScreenLobbyButton();
                        CareGameBootstrap.Instance.UpdateLobbyUI();
                    }

                    // 3. Audio transition menuju Lobby Restorasi sesuai narasi.txt:
                    // "BGM berpindah dari bgm_story_sad.ogg ke bgm_restoration_hub.ogg. Tambahkan SFX transisi singkat."
                    if (CareAudioManager.Instance != null)
                    {
                        CareAudioManager.Instance.PlayTransitionSFX();
                        CareAudioManager.Instance.StopAmbience();
                        CareAudioManager.Instance.PlayRestorationHubBGM();
                        CareAudioManager.Instance.PlayMorningAmbience();
                    }

                    // 4. Masuk ke Lobby Restorasi Utama
                    ShowLobbyRestorasi();
                });
            }
        }

        /// <summary>
        /// Menampilkan Lobby Restorasi (Habitat Satwa & Ensiklopedia).
        /// </summary>
        public void ShowLobbyRestorasi()
        {
            currentState = GameState.LobbyRestorasi;
            HideAllScreens();
            if (lobbyRestorasiPanel != null) lobbyRestorasiPanel.SetActive(true);

            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayRestorationHubBGM();
                CareAudioManager.Instance.PlayMorningAmbience();
            }
        }

        /// <summary>
        /// Dipanggil dari tombol PLAY di Lobby Restorasi untuk membuka Peta Stage.
        /// </summary>
        public void OpenStageMap()
        {
            currentState = GameState.StageMap;
            HideAllScreens();
            if (stageMapPanel != null) stageMapPanel.SetActive(true);
        }

        /// <summary>
        /// Memulai Stage tertentu (1, 2, atau 3).
        /// Sesuai instruksi dan naskah BOSS STAGE:
        /// Saat pemain menekan MULAI pada Stage 1/2/3, permainan puzzle tidak langsung dimulai,
        /// melainkan menampilkan adegan percakapan antara MC dan Boss terlebih dahulu.
        /// Setelah percakapan selesai -> ScreenFader (fade-out ke hitam lalu fade-in) -> mulai pertarungan Match-3.
        /// Setelah pertarungan selesai (Boss kalah) -> ScreenFader -> tampilkan percakapan Dr. Sawit Man (defeat dialogue).
        /// Setelah percakapan kekalahan selesai -> Buka reward satwa & kembali ke Lobby atau Ending.
        /// </summary>
        public void StartStage(int stageNumber)
        {
            currentStage = stageNumber;
            currentLevel = 1;

            StartStageIntroDialogue(stageNumber);
        }

        public void StartStageIntroDialogue(int stageNumber)
        {
            currentState = GameState.BossFight;
            currentStage = stageNumber;
            currentLevel = 1;
            HideAllScreens();

            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayBossBGM();
            }

            List<DialogueLine> introDialogue = null;
            if (stageNumber == 1) introDialogue = CareStoryDatabase.BossStage1_Intro;
            else if (stageNumber == 2) introDialogue = CareStoryDatabase.BossStage2_Intro;
            else if (stageNumber == 3) introDialogue = CareStoryDatabase.BossStage3_Intro;

            if (DialogueManager.Instance != null && introDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(introDialogue, () =>
                {
                    // Setelah percakapan selesai: fade-out ke hitam, lalu fade-in ke permainan!
                    ScreenFader.Do(() =>
                    {
                        StartCombatGameplay(stageNumber);
                    });
                });
            }
            else
            {
                ScreenFader.Do(() =>
                {
                    StartCombatGameplay(stageNumber);
                });
            }
        }

        public void StartCombatGameplay(int stageNumber)
        {
            currentState = GameState.HiddenObjectLevel;
            currentStage = stageNumber;
            currentLevel = 1;
            HideAllScreens();
            if (hiddenObjectPanel != null) hiddenObjectPanel.SetActive(true);

            string locationName = "";
            if (stageNumber == 1) locationName = "Area Tepi Hutan";
            else if (stageNumber == 2) locationName = "Jalur Hutan Terbuka";
            else if (stageNumber == 3) locationName = "Area Pemulihan";

            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayBossBGM();
            }

            if (Match3CombatManager.Instance != null)
            {
                Match3CombatManager.Instance.StartCombat(stageNumber, 1, locationName, () =>
                {
                    // Setelah permainan selesai (Boss dikalahkan):
                    OnStageCombatWon(stageNumber);
                });
            }
        }

        public void StartHiddenObjectLevel(int stage, int level)
        {
            StartCombatGameplay(stage);
        }

        private void OnStageCombatWon(int stageNumber)
        {
            // Setelah permainan selesai, muncul lagi scene percakapan Dr. Sawit Man melalui transisi fade
            ScreenFader.Do(() =>
            {
                ShowStageDefeatDialogue(stageNumber);
            });
        }

        public void ShowStageDefeatDialogue(int stageNumber)
        {
            HideAllScreens();

            List<DialogueLine> defeatDialogue = null;
            if (stageNumber == 1) defeatDialogue = CareStoryDatabase.BossStage1_Defeat;
            else if (stageNumber == 2) defeatDialogue = CareStoryDatabase.BossStage2_Defeat;
            else if (stageNumber == 3) defeatDialogue = CareStoryDatabase.BossStage3_Defeat;

            if (DialogueManager.Instance != null && defeatDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(defeatDialogue, () =>
                {
                    OnStageDefeatDialogueComplete(stageNumber);
                });
            }
            else
            {
                OnStageDefeatDialogueComplete(stageNumber);
            }
        }

        private void OnStageDefeatDialogueComplete(int stageNumber)
        {
            // Simpan progres ke Local Storage
            if (CareSaveManager.Instance != null)
            {
                CareSaveManager.Instance.MarkLevelCompleted(stageNumber, 1);
            }

            if (stageNumber == 1)
            {
                UnlockAnimal("rusa_timor");
                ScreenFader.Do(() =>
                {
                    if (CareGameBootstrap.Instance != null) CareGameBootstrap.Instance.UpdateLobbyUI();
                    ShowLobbyRestorasi();
                });
            }
            else if (stageNumber == 2)
            {
                UnlockAnimal("orangutan");
                ScreenFader.Do(() =>
                {
                    if (CareGameBootstrap.Instance != null) CareGameBootstrap.Instance.UpdateLobbyUI();
                    ShowLobbyRestorasi();
                });
            }
            else if (stageNumber == 3)
            {
                UnlockAnimal("gajah_asia");
                ScreenFader.Do(() =>
                {
                    ShowEnding();
                });
            }
        }

        public void StartBossFight(int stage)
        {
            StartStage(stage);
        }

        public void SubmitBossEvidence(int evidenceIndex)
        {
            OnStageCombatWon(currentStage);
        }

        private void OnBossDefeated(int stage, List<DialogueLine> defeatDialogue)
        {
            ShowStageDefeatDialogue(stage);
        }

        public void UnlockAnimal(string animalKey)
        {
            var animal = CareStoryDatabase.Animals.Find(a => a.animalKey == animalKey);
            if (animal != null)
            {
                animal.isUnlocked = true;
                if (CareAudioManager.Instance != null)
                {
                    CareAudioManager.Instance.PlayAnimalUnlockSFX();
                }
            }

            // Simpan satwa terbuka ke Local Storage
            if (CareSaveManager.Instance != null)
            {
                CareSaveManager.Instance.UnlockAnimal(animalKey);
            }
        }

        public void ShowEnding()
        {
            currentState = GameState.Ending;
            HideAllScreens();
            if (endingPanel != null) endingPanel.SetActive(true);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(CareStoryDatabase.Ending_Scene, () =>
                {
                    ShowLobbyRestorasi();
                });
            }
        }

        private void HideAllScreens()
        {
            if (titleScreenPanel != null) titleScreenPanel.SetActive(false);
            if (openingStoryPanel != null) openingStoryPanel.SetActive(false);
            if (lobbyRestorasiPanel != null) lobbyRestorasiPanel.SetActive(false);
            if (stageMapPanel != null) stageMapPanel.SetActive(false);
            if (hiddenObjectPanel != null) hiddenObjectPanel.SetActive(false);
            if (bossFightPanel != null) bossFightPanel.SetActive(false);
            if (endingPanel != null) endingPanel.SetActive(false);
        }
    }
}

