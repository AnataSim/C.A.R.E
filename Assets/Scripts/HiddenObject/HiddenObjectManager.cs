using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CARE.Story;

namespace CARE.Gameplay
{
    public class HiddenObjectManager : MonoBehaviour
    {
        public static HiddenObjectManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private Text locationTitleText;
        [SerializeField] private Transform itemListContainer;
        [SerializeField] private GameObject itemLabelPrefab;
        [SerializeField] private Transform itemSpawnArea;

        [Header("Item Found Popup")]
        [SerializeField] private GameObject itemPopupPanel;
        [SerializeField] private Text popupItemNameText;
        [SerializeField] private Text popupNoteText;
        [SerializeField] private Image popupItemImage;
        [SerializeField] private Button popupContinueButton;

        [Header("Audio")]
        [SerializeField] private AudioClip itemFoundSFX;
        [SerializeField] private AudioClip wrongClickSFX;
        [SerializeField] private AudioClip levelCompleteSFX;
        [SerializeField] private AudioSource audioSource;

        private List<HiddenItemData> currentItems = new List<HiddenItemData>();
        private HashSet<string> foundItemNames = new HashSet<string>();
        private Action onLevelCompleteCallback;

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

            if (itemFoundSFX == null) itemFoundSFX = Resources.Load<AudioClip>("Audio/sfx_bell");
            if (wrongClickSFX == null) wrongClickSFX = Resources.Load<AudioClip>("Audio/sfx_click");
            if (levelCompleteSFX == null) levelCompleteSFX = Resources.Load<AudioClip>("Audio/sfx_bonus");

            if (popupContinueButton != null)
            {
                popupContinueButton.onClick.AddListener(ClosePopup);
            }

            if (itemPopupPanel != null) itemPopupPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
        }

        public void SetupUI(GameObject gamePanel, Text title, Transform listContainer, Transform spawnArea, GameObject popupPanel, Text popupName, Text popupNote, Image popupImg, Button popupBtn)
        {
            gameplayPanel = gamePanel;
            locationTitleText = title;
            itemListContainer = listContainer;
            itemSpawnArea = spawnArea;
            itemPopupPanel = popupPanel;
            popupItemNameText = popupName;
            popupNoteText = popupNote;
            popupItemImage = popupImg;
            popupContinueButton = popupBtn;

            if (popupContinueButton != null)
            {
                popupContinueButton.onClick.RemoveAllListeners();
                popupContinueButton.onClick.AddListener(ClosePopup);
            }

            if (itemPopupPanel != null) itemPopupPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
        }

        public void StartLevel(string locationName, List<HiddenItemData> items, Action onLevelComplete)
        {
            currentItems = items;
            foundItemNames.Clear();
            onLevelCompleteCallback = onLevelComplete;

            if (gameplayPanel != null) gameplayPanel.SetActive(true);
            if (locationTitleText != null) locationTitleText.text = "AREA INVESTIGASI: " + locationName.ToUpper();

            RefreshItemListUI();
            SpawnClickableItems();
        }

        public void SetItemSpawnArea(Transform area)
        {
            itemSpawnArea = area;
        }

        private Sprite LoadItemSprite(HiddenItemData item)
        {
            for (int s = 1; s <= 3; s++)
            {
                for (int l = 1; l <= 2; l++)
                {
                    Sprite sp = Resources.Load<Sprite>($"TemporaryAssets/HiddenObjects/Stage{s}_Level{l}/{item.spriteName}");
                    if (sp != null) return sp;
                }
            }
            Sprite direct = Resources.Load<Sprite>($"TemporaryAssets/HiddenObjects/{item.spriteName}");
            if (direct != null) return direct;

            return Resources.Load<Sprite>("TemporaryAssets/UI/icon_check");
        }

        private void SpawnClickableItems()
        {
            if (itemSpawnArea == null || currentItems == null) return;

            // Clear previous items
            foreach (Transform child in itemSpawnArea)
            {
                Destroy(child.gameObject);
            }

            // Balanced distributed coordinates across standard 1920x1080 canvas
            Vector2[] defaultPositions = new Vector2[]
            {
                new Vector2(-420, 80),
                new Vector2(-200, -140),
                new Vector2(30, 120),
                new Vector2(240, -80),
                new Vector2(440, 140)
            };

            for (int i = 0; i < currentItems.Count; i++)
            {
                var item = currentItems[i];
                Sprite itemSprite = LoadItemSprite(item);

                GameObject itemObj = new GameObject("HiddenItem_" + item.itemName);
                itemObj.transform.SetParent(itemSpawnArea, false);

                RectTransform rt = itemObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(90, 90);
                Vector2 pos = i < defaultPositions.Length ? defaultPositions[i] : new Vector2(UnityEngine.Random.Range(-400, 400), UnityEngine.Random.Range(-150, 150));
                rt.anchoredPosition = pos;

                Image img = itemObj.AddComponent<Image>();
                img.sprite = itemSprite;
                img.preserveAspect = true;

                Button btn = itemObj.AddComponent<Button>();
                string capturedName = item.itemName;
                Sprite capturedSprite = itemSprite;
                GameObject capturedObj = itemObj;

                btn.onClick.AddListener(() =>
                {
                    OnItemClicked(capturedName, capturedSprite);
                    if (capturedObj != null) capturedObj.SetActive(false);
                });
            }
        }

        private void RefreshItemListUI()
        {
            if (itemListContainer == null) return;

            // Clear previous labels
            foreach (Transform child in itemListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new labels
            foreach (var item in currentItems)
            {
                GameObject labelObj = new GameObject("Item_" + item.itemName);
                labelObj.transform.SetParent(itemListContainer, false);
                Text labelText = labelObj.AddComponent<Text>();
                Font labelFont = null;
                try { labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
                if (labelFont == null) { try { labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch {} }
                if (labelFont == null) labelFont = Font.CreateDynamicFontFromOSFont("Arial", 20);

                labelText.font = labelFont;
                labelText.fontSize = 20;

                bool isFound = foundItemNames.Contains(item.itemName);
                labelText.text = (isFound ? "✔ " : "○ ") + item.itemName;
                labelText.color = isFound ? Color.green : Color.white;
            }
        }

        /// <summary>
        /// Dipanggil saat pemain mengetuk objek di layar.
        /// </summary>
        public void OnItemClicked(string itemName, Sprite itemSprite = null)
        {
            if (foundItemNames.Contains(itemName)) return;

            HiddenItemData itemData = currentItems.Find(i => i.itemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (itemData != null)
            {
                // Item Benar
                foundItemNames.Add(itemData.itemName);

                if (itemFoundSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(itemFoundSFX);
                }

                ShowItemPopup(itemData, itemSprite);
                RefreshItemListUI();

                // Cek apakah semua sudah ditemukan
                if (foundItemNames.Count >= currentItems.Count)
                {
                    OnAllItemsFound();
                }
            }
            else
            {
                // Objek Salah
                if (wrongClickSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(wrongClickSFX);
                }
            }
        }

        private void ShowItemPopup(HiddenItemData data, Sprite sprite)
        {
            if (itemPopupPanel == null) return;

            itemPopupPanel.SetActive(true);
            if (popupItemNameText != null) popupItemNameText.text = "OBJEK DITEMUKAN: " + data.itemName;
            if (popupNoteText != null) popupNoteText.text = data.popupNote;
            if (popupItemImage != null && sprite != null)
            {
                popupItemImage.sprite = sprite;
            }
        }

        public void ClosePopup()
        {
            if (itemPopupPanel != null) itemPopupPanel.SetActive(false);
        }

        private void OnAllItemsFound()
        {
            if (levelCompleteSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(levelCompleteSFX);
            }

            Debug.Log("[HiddenObject] Seluruh objek berhasil ditemukan! Level selesai.");
            onLevelCompleteCallback?.Invoke();
        }
    }
}

