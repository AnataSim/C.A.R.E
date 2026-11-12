using System.Collections.Generic;
using UnityEngine;
using CARE.Story;
using CARE.UI;
using CARE.Gameplay;
using CARE.Audio;

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
                    // Setelah opening selesai, transisi ke Lobby Restorasi
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
        /// </summary>
        public void StartStage(int stageNumber)
        {
            currentStage = stageNumber;
            currentLevel = 1;
            StartHiddenObjectLevel(stageNumber, 1);
        }

        public void StartHiddenObjectLevel(int stage, int level)
        {
            currentState = GameState.HiddenObjectLevel;
            currentStage = stage;
            currentLevel = level;
            HideAllScreens();
            if (hiddenObjectPanel != null) hiddenObjectPanel.SetActive(true);

            string locationName = "";
            List<HiddenItemData> items = null;

            if (stage == 1 && level == 1)
            {
                locationName = "Area Tepi Hutan";
                items = CareStoryDatabase.Stage1_Level1_Items;
            }
            else if (stage == 1 && level == 2)
            {
                locationName = "Area Pepohonan";
                items = CareStoryDatabase.Stage1_Level2_Items;
            }
            else if (stage == 2 && level == 1)
            {
                locationName = "Jalur Hutan Terbuka";
                items = CareStoryDatabase.Stage2_Level1_Items;
            }
            else if (stage == 2 && level == 2)
            {
                locationName = "Pos Pemeriksaan";
                items = CareStoryDatabase.Stage2_Level2_Items;
            }
            else if (stage == 3 && level == 1)
            {
                locationName = "Area Evakuasi";
                items = CareStoryDatabase.Stage3_Level1_Items;
            }
            else if (stage == 3 && level == 2)
            {
                locationName = "Area Pemulihan";
                items = CareStoryDatabase.Stage3_Level2_Items;
            }

            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayInvestigationBGM();
            }

            if (HiddenObjectManager.Instance != null && items != null)
            {
                HiddenObjectManager.Instance.StartLevel(locationName, items, () =>
                {
                    OnHiddenObjectLevelComplete();
                });
            }
        }

        private void OnHiddenObjectLevelComplete()
        {
            if (currentLevel == 1)
            {
                // Lanjut ke Level 2 Hidden Object
                StartHiddenObjectLevel(currentStage, 2);
            }
            else if (currentLevel == 2)
            {
                // Lanjut ke Level 3 Boss Fight
                StartBossFight(currentStage);
            }
        }

        public void StartBossFight(int stage)
        {
            currentState = GameState.BossFight;
            HideAllScreens();
            if (bossFightPanel != null) bossFightPanel.SetActive(true);

            if (CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlayBossBGM();
            }

            List<DialogueLine> introDialogue = null;
            List<DialogueLine> defeatDialogue = null;

            if (stage == 1)
            {
                introDialogue = CareStoryDatabase.BossStage1_Intro;
                defeatDialogue = CareStoryDatabase.BossStage1_Defeat;
            }
            else if (stage == 2)
            {
                introDialogue = CareStoryDatabase.BossStage2_Intro;
                defeatDialogue = CareStoryDatabase.BossStage2_Defeat;
            }
            else
            {
                introDialogue = CareStoryDatabase.BossStage3_Intro;
                defeatDialogue = CareStoryDatabase.BossStage3_Defeat;
            }

            // Dialog Pengenalan Boss
            if (DialogueManager.Instance != null && introDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(introDialogue, () =>
                {
                    // Boss Battle sequence: setelah menang
                    OnBossDefeated(stage, defeatDialogue);
                });
            }
        }

        private void OnBossDefeated(int stage, List<DialogueLine> defeatDialogue)
        {
            if (DialogueManager.Instance != null && defeatDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(defeatDialogue, () =>
                {
                    // Unlock animal reward
                    if (stage == 1) UnlockAnimal("rusa_timor");
                    else if (stage == 2) UnlockAnimal("orangutan");
                    else if (stage == 3)
                    {
                        UnlockAnimal("gajah_asia");
                        ShowEnding();
                        return;
                    }

                    // Kembali ke Lobby
                    ShowLobbyRestorasi();
                });
            }
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

