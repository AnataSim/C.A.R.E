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

            if (popupContinueButton != null)
            {
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
                labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

