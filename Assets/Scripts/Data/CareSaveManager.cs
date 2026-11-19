using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CARE.Story;

namespace CARE.Data
{
    [Serializable]
    public class CareSaveData
    {
        public bool isPrologueCompleted = false;
        public int currentStage = 1;
        public int currentLevel = 1;
        public int totalExp = 0;
        public List<string> unlockedAnimals = new List<string>();
        public List<string> completedLevels = new List<string>();
        public string lastSaveTime = "";
    }

    /// <summary>
    /// Local Storage Manager untuk C.A.R.E.
    /// Menyimpan progress game (Prologue status, Stage/Level, Satwa yang diselamatkan, Total EXP)
    /// Menggunakan JSON file di Application.persistentDataPath dan disinkronkan dengan PlayerPrefs sebagai fail-safe.
    /// </summary>
    public class CareSaveManager : MonoBehaviour
    {
        public static CareSaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "care_savedata.json";
        private const string PREF_KEY_PROLOGUE = "CARE_PROLOGUE_COMPLETED";
        private const string PREF_KEY_STAGE = "CARE_CURRENT_STAGE";
        private const string PREF_KEY_LEVEL = "CARE_CURRENT_LEVEL";
        private const string PREF_KEY_EXP = "CARE_TOTAL_EXP";
        private const string PREF_KEY_ANIMALS = "CARE_UNLOCKED_ANIMALS";

        [SerializeField] private CareSaveData currentData = new CareSaveData();

        public CareSaveData Data => currentData;
        public bool IsPrologueCompleted => currentData != null && currentData.isPrologueCompleted;
        public int CurrentStage => currentData != null ? currentData.currentStage : 1;
        public int CurrentLevel => currentData != null ? currentData.currentLevel : 1;
        public int TotalExp => currentData != null ? currentData.totalExp : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGame();
        }

        /// <summary>
        /// Memuat progress game dari penyimpanan lokal (JSON file & PlayerPrefs).
        /// </summary>
        public void LoadGame()
        {
            string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            bool loadedFromFile = false;

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        currentData = JsonUtility.FromJson<CareSaveData>(json);
                        if (currentData != null)
                        {
                            loadedFromFile = true;
                            Debug.Log($"[CareSaveManager] Berhasil memuat progress game dari file: {filePath}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CareSaveManager] Gagal membaca save file, menggunakan fallback PlayerPrefs: {e.Message}");
                }
            }

            // Fallback / sync dari PlayerPrefs jika file belum ada atau terjadi masalah
            if (!loadedFromFile)
            {
                currentData = new CareSaveData();
                currentData.isPrologueCompleted = PlayerPrefs.GetInt(PREF_KEY_PROLOGUE, 0) == 1;
                currentData.currentStage = PlayerPrefs.GetInt(PREF_KEY_STAGE, 1);
                currentData.currentLevel = PlayerPrefs.GetInt(PREF_KEY_LEVEL, 1);
                currentData.totalExp = PlayerPrefs.GetInt(PREF_KEY_EXP, 0);

                string animalsStr = PlayerPrefs.GetString(PREF_KEY_ANIMALS, "");
                if (!string.IsNullOrEmpty(animalsStr))
                {
                    string[] split = animalsStr.Split(',');
                    foreach (var s in split)
                    {
                        if (!string.IsNullOrEmpty(s) && !currentData.unlockedAnimals.Contains(s))
                        {
                            currentData.unlockedAnimals.Add(s);
                        }
                    }
                }
                Debug.Log($"[CareSaveManager] Inisialisasi data dari PlayerPrefs. Prologue: {currentData.isPrologueCompleted}");
            }

            // Pastikan data list tidak null
            if (currentData.unlockedAnimals == null) currentData.unlockedAnimals = new List<string>();
            if (currentData.completedLevels == null) currentData.completedLevels = new List<string>();

            // Sinkronkan satwa terbuka ke CareStoryDatabase
            SyncAnimalsWithDatabase();
        }

        /// <summary>
        /// Menyimpan seluruh progress game saat ini ke local storage.
        /// </summary>
        public void SaveGame()
        {
            if (currentData == null) currentData = new CareSaveData();
            currentData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 1. Simpan ke JSON File (Application.persistentDataPath)
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                string json = JsonUtility.ToJson(currentData, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"[CareSaveManager] Progress game berhasil disimpan ke JSON: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareSaveManager] Error menulis save file: {e.Message}");
            }

            // 2. Dual-save ke PlayerPrefs (menjamin persistensi di semua platform)
            PlayerPrefs.SetInt(PREF_KEY_PROLOGUE, currentData.isPrologueCompleted ? 1 : 0);
            PlayerPrefs.SetInt(PREF_KEY_STAGE, currentData.currentStage);
            PlayerPrefs.SetInt(PREF_KEY_LEVEL, currentData.currentLevel);
            PlayerPrefs.SetInt(PREF_KEY_EXP, currentData.totalExp);
            PlayerPrefs.SetString(PREF_KEY_ANIMALS, string.Join(",", currentData.unlockedAnimals));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Menandai apakah narasi awal (prologue) telah selesai.
        /// Saat selesai, tombol Lobby Restorasi di Title Screen akan terbuka secara permanen.
        /// </summary>
        public void SetPrologueCompleted(bool completed)
        {
            if (currentData == null) currentData = new CareSaveData();
            currentData.isPrologueCompleted = completed;
            SaveGame();
            Debug.Log($"[CareSaveManager] Status Prologue diperbarui: {completed}");
        }

        /// <summary>
        /// Membuka satwa baru di Ensiklopedia & Lobby Restorasi serta menyimpannya.
        /// </summary>
        public void UnlockAnimal(string animalKey)
        {
            if (string.IsNullOrEmpty(animalKey)) return;
            if (currentData == null) currentData = new CareSaveData();

            if (!currentData.unlockedAnimals.Contains(animalKey))
            {
                currentData.unlockedAnimals.Add(animalKey);
            }

            // Perbarui data runtime di CareStoryDatabase
            if (CareStoryDatabase.Animals != null)
            {
                var animal = CareStoryDatabase.Animals.Find(a => a.animalKey == animalKey);
                if (animal != null)
                {
                    animal.isUnlocked = true;
                }
            }

            SaveGame();
            Debug.Log($"[CareSaveManager] Satwa '{animalKey}' berhasil dibuka dan disimpan!");
        }

        public bool IsAnimalUnlocked(string animalKey)
        {
            if (currentData == null || currentData.unlockedAnimals == null) return false;
            return currentData.unlockedAnimals.Contains(animalKey);
        }

        /// <summary>
        /// Menambahkan EXP yang diperoleh dari Match-3 RPG Combat dan menyimpannya.
        /// </summary>
        public void AddExp(int expAmount)
        {
            if (expAmount <= 0) return;
            if (currentData == null) currentData = new CareSaveData();
            currentData.totalExp += expAmount;
            SaveGame();
            Debug.Log($"[CareSaveManager] +{expAmount} EXP ditambahkan. Total EXP: {currentData.totalExp}");
        }

        /// <summary>
        /// Memperbarui tahapan tertinggi yang berhasil dicapai pemain.
        /// </summary>
        public void SetStageProgress(int stage, int level)
        {
            if (currentData == null) currentData = new CareSaveData();
            if (stage > currentData.currentStage || (stage == currentData.currentStage && level > currentData.currentLevel))
            {
                currentData.currentStage = stage;
                currentData.currentLevel = level;
            }
            SaveGame();
        }

        public void MarkLevelCompleted(int stage, int level)
        {
            if (currentData == null) currentData = new CareSaveData();
            string key = $"Stage{stage}_Level{level}";
            if (!currentData.completedLevels.Contains(key))
            {
                currentData.completedLevels.Add(key);
            }
            SaveGame();
        }

        private void SyncAnimalsWithDatabase()
        {
            if (CareStoryDatabase.Animals == null) return;
            foreach (var a in CareStoryDatabase.Animals)
            {
                if (currentData.unlockedAnimals.Contains(a.animalKey))
                {
                    a.isUnlocked = true;
                }
            }
        }

        /// <summary>
        /// Menghapus seluruh save data untuk pengujian awal baru (New Game).
        /// </summary>
        [ContextMenu("Reset Save Data")]
        public void ResetSaveData()
        {
            currentData = new CareSaveData();
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch {}

            PlayerPrefs.DeleteKey(PREF_KEY_PROLOGUE);
            PlayerPrefs.DeleteKey(PREF_KEY_STAGE);
            PlayerPrefs.DeleteKey(PREF_KEY_LEVEL);
            PlayerPrefs.DeleteKey(PREF_KEY_EXP);
            PlayerPrefs.DeleteKey(PREF_KEY_ANIMALS);
            PlayerPrefs.Save();

            // Reset status di CareStoryDatabase
            if (CareStoryDatabase.Animals != null)
            {
                foreach (var a in CareStoryDatabase.Animals)
                {
                    a.isUnlocked = false;
                }
            }

            Debug.Log("[CareSaveManager] Save data berhasil di-reset!");
        }
    }
}
