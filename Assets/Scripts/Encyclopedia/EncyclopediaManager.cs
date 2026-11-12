using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CARE.Story;

namespace CARE.UI
{
    public class EncyclopediaManager : MonoBehaviour
    {
        public static EncyclopediaManager Instance { get; private set; }

        [Header("UI Panel")]
        [SerializeField] private GameObject encyclopediaPanel;
        [SerializeField] private Text commonNameText;
        [SerializeField] private Text scientificNameText;
        [SerializeField] private Text openingQuoteText;
        [SerializeField] private Text contentBodyText;
        [SerializeField] private Image animalPreviewImage;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Tab Buttons")]
        [SerializeField] private Button taxonomyTabButton;
        [SerializeField] private Button habitatTabButton;
        [SerializeField] private Button dietTabButton;
        [SerializeField] private Button closeButton;

        [Header("Audio")]
        [SerializeField] private AudioClip tabChangeSFX;
        [SerializeField] private AudioSource audioSource;

        private enum TabType { Taxonomy, Habitat, Diet }
        private TabType currentTab = TabType.Taxonomy;
        private AnimalEncyclopediaData selectedAnimal;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (taxonomyTabButton != null) taxonomyTabButton.onClick.AddListener(() => SwitchTab(TabType.Taxonomy));
            if (habitatTabButton != null) habitatTabButton.onClick.AddListener(() => SwitchTab(TabType.Habitat));
            if (dietTabButton != null) dietTabButton.onClick.AddListener(() => SwitchTab(TabType.Diet));
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);

            if (encyclopediaPanel != null) encyclopediaPanel.SetActive(false);
        }

        public void OpenEncyclopedia(string animalKey)
        {
            selectedAnimal = CareStoryDatabase.Animals.Find(a => a.animalKey == animalKey);
            if (selectedAnimal == null) return;

            if (encyclopediaPanel != null) encyclopediaPanel.SetActive(true);

            if (commonNameText != null) commonNameText.text = selectedAnimal.commonName;
            if (scientificNameText != null) scientificNameText.text = selectedAnimal.scientificName;
            if (openingQuoteText != null) openingQuoteText.text = selectedAnimal.openingQuote;

            // Load Sprite
            string spritePath = selectedAnimal.isUnlocked 
                ? $"TemporaryAssets/Animals/{selectedAnimal.animalKey}"
                : $"TemporaryAssets/Animals/{selectedAnimal.animalKey}_silhouette";

            Sprite animalSprite = Resources.Load<Sprite>(spritePath);
            if (animalPreviewImage != null && animalSprite != null)
            {
                animalPreviewImage.sprite = animalSprite;
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!selectedAnimal.isUnlocked);
            }

            currentTab = TabType.Taxonomy;
            UpdateTabContent();
        }

        private void SwitchTab(TabType newTab)
        {
            currentTab = newTab;
            if (tabChangeSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(tabChangeSFX);
            }
            UpdateTabContent();
        }

        private void UpdateTabContent()
        {
            if (selectedAnimal == null || contentBodyText == null) return;

            switch (currentTab)
            {
                case TabType.Taxonomy:
                    contentBodyText.text = selectedAnimal.taxonomy;
                    break;
                case TabType.Habitat:
                    contentBodyText.text = selectedAnimal.habitat;
                    break;
                case TabType.Diet:
                    contentBodyText.text = selectedAnimal.diet;
                    break;
            }
        }

        public void ClosePanel()
        {
            if (encyclopediaPanel != null) encyclopediaPanel.SetActive(false);
        }
    }
}

