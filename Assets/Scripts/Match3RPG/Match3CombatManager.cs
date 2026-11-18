using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CARE.Audio;
using CARE.Story;

namespace CARE.Gameplay
{
    [Serializable]
    public class Match3ItemDef
    {
        public int id;
        public string itemName;
        public string symbol;
        public Sprite sprite;
        public Color themeColor;

        public Match3ItemDef(int id, string itemName, string symbol, Sprite sprite, Color themeColor)
        {
            this.id = id;
            this.itemName = itemName;
            this.symbol = symbol;
            this.sprite = sprite;
            this.themeColor = themeColor;
        }
    }

    /// <summary>
    /// Core Game Engine for Match-3 RPG Combat (Candy Crush + Dungeon Puzzles).
    /// When tiles are combined, they fire projectiles upwards to attack the enemy/target.
    /// Includes full HP bar mechanics, combos, cascades, and victory/defeat states.
    /// </summary>
    public class Match3CombatManager : MonoBehaviour
    {
        public static Match3CombatManager Instance { get; private set; }

        [Header("Board Configuration")]
        public const int COLUMNS = 7;
        public const int ROWS = 6;
        public const float TILE_SIZE = 82f;
        public const float TILE_SPACING = 6f;

        // Runtime State
        public bool CanPlayerMove { get; private set; } = false;
        public int MovesRemaining { get; private set; }
        public int Score { get; private set; }
        public int CurrentCombo { get; private set; } = 1;
        public int MaxComboReached { get; private set; } = 1;

        // Target / Enemy Stats
        public int TargetMaxHP { get; private set; } = 600;
        public int TargetCurrentHP { get; private set; } = 600;

        // UI Panel references
        private GameObject combatPanel;
        private RectTransform boardContainer;
        private RectTransform projectileLayer;
        private RectTransform damageTextLayer;

        // Header UI
        private Text stageTitleText;
        private Text movesCounterText;
        private Text scoreText;
        private Text comboBannerText;
        private Button exitButton;

        // Target / Enemy UI
        private Image enemyImage;
        private RectTransform enemyRect;
        private Image hpBarFill;
        private Text hpText;
        private Text enemyNameText;

        // Result Modals
        private GameObject victoryModal;
        private Text victoryStatsText;
        private Button victoryContinueBtn;

        private GameObject defeatModal;
        private Button defeatRetryBtn;
        private Button defeatExitBtn;

        // Internal Grid
        private Match3Tile[,] grid = new Match3Tile[COLUMNS, ROWS];
        private Match3Tile selectedTile = null;
        private List<Match3ItemDef> currentTileTypes = new List<Match3ItemDef>();

        // Enemy Sprites & Animations
        private Sprite enemyIdleSprite;
        private Sprite enemyEnragedSprite;
        private Vector2 enemyBasePos;
        private bool isEnemyDefeated = false;

        // Callbacks & current mission tracker
        private Action onCombatCompleteCallback;
        private int currentStageNumber = 1;
        private int currentLevelNumber = 1;
        private string currentLocationName = "";

        // Audio
        private AudioClip sfxHit;
        private AudioClip sfxClick;
        private AudioClip sfxBonus;
        private AudioClip sfxBell;
        private AudioClip sfxWhoosh;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadAudioClips();
            LoadEnemySprites();
        }

        private void LoadAudioClips()
        {
            sfxHit = Resources.Load<AudioClip>("Audio/sfx_hit");
            sfxClick = Resources.Load<AudioClip>("Audio/sfx_click");
            sfxBonus = Resources.Load<AudioClip>("Audio/sfx_bonus");
            sfxBell = Resources.Load<AudioClip>("Audio/sfx_bell");
            sfxWhoosh = Resources.Load<AudioClip>("Audio/sfx_whoosh");
        }

        private void LoadEnemySprites()
        {
            enemyIdleSprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man_idle");
            if (enemyIdleSprite == null) enemyIdleSprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man");
            enemyEnragedSprite = Resources.Load<Sprite>("TemporaryAssets/Characters/doctor_sawit_man_enraged");
            if (enemyEnragedSprite == null) enemyEnragedSprite = enemyIdleSprite;
        }

        private void Update()
        {
            // Idle bobbing animation for enemy
            if (enemyRect != null && !isEnemyDefeated && combatPanel != null && combatPanel.activeSelf)
            {
                float bobOffset = Mathf.Sin(Time.time * 3f) * 6f;
                enemyRect.anchoredPosition = new Vector2(enemyBasePos.x, enemyBasePos.y + bobOffset);
            }
        }

        #region Setup UI Wiring

        public void SetupUI(
            GameObject panel,
            RectTransform boardTrans,
            RectTransform projLayer,
            RectTransform dmgLayer,
            Text titleTxt,
            Text movesTxt,
            Text scoreTxt,
            Text comboTxt,
            Button exitBtn,
            Image enemyImg,
            Image hpFill,
            Text hpTxt,
            Text enemyNameTxt,
            GameObject winModal,
            Text winStatsTxt,
            Button winBtn,
            GameObject loseModal,
            Button loseRetryBtn,
            Button loseExitBtn)
        {
            combatPanel = panel;
            boardContainer = boardTrans;
            projectileLayer = projLayer;
            damageTextLayer = dmgLayer;

            stageTitleText = titleTxt;
            movesCounterText = movesTxt;
            scoreText = scoreTxt;
            comboBannerText = comboTxt;
            exitButton = exitBtn;

            enemyImage = enemyImg;
            if (enemyImage != null)
            {
                enemyRect = enemyImage.GetComponent<RectTransform>();
                if (enemyRect != null) enemyBasePos = enemyRect.anchoredPosition;
            }
            hpBarFill = hpFill;
            hpText = hpTxt;
            enemyNameText = enemyNameTxt;

            victoryModal = winModal;
            victoryStatsText = winStatsTxt;
            victoryContinueBtn = winBtn;

            defeatModal = loseModal;
            defeatRetryBtn = loseRetryBtn;
            defeatExitBtn = loseExitBtn;

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() =>
                {
                    if (CARE.Core.GameFlowManager.Instance != null)
                    {
                        CARE.Core.GameFlowManager.Instance.OpenStageMap();
                    }
                });
            }

            if (victoryContinueBtn != null)
            {
                victoryContinueBtn.onClick.RemoveAllListeners();
                victoryContinueBtn.onClick.AddListener(OnVictoryContinueClicked);
            }

            if (defeatRetryBtn != null)
            {
                defeatRetryBtn.onClick.RemoveAllListeners();
                defeatRetryBtn.onClick.AddListener(OnRetryClicked);
            }

            if (defeatExitBtn != null)
            {
                defeatExitBtn.onClick.RemoveAllListeners();
                defeatExitBtn.onClick.AddListener(() =>
                {
                    if (CARE.Core.GameFlowManager.Instance != null)
                    {
                        CARE.Core.GameFlowManager.Instance.OpenStageMap();
                    }
                });
            }

            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);
        }

        #endregion

        #region Start Combat

        /// <summary>
        /// Starts a Match-3 RPG Combat level against the enemy target.
        /// Reuses the existing Criminal Case item assets as the puzzle combine tiles!
        /// </summary>
        public void StartCombat(int stage, int level, string locationName, Action onComplete)
        {
            currentStageNumber = stage;
            currentLevelNumber = level;
            currentLocationName = locationName;
            onCombatCompleteCallback = onComplete;

            if (combatPanel != null) combatPanel.SetActive(true);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);

            isEnemyDefeated = false;
            Score = 0;
            CurrentCombo = 1;
            MaxComboReached = 1;

            // Configure Target HP & Moves based on stage & level
            SetupLevelBalancing(stage, level);

            // Setup Criminal Case items for this stage
            SetupTileItemDefinitions(stage, level);

            // Update UI
            UpdateHeaderUI();
            UpdateHPBarUI(true);

            // Play Combat BGM
            if (CareAudioManager.Instance != null)
            {
                if (level == 2 || stage == 3)
                    CareAudioManager.Instance.PlayBossBGM();
                else
                    CareAudioManager.Instance.PlayInvestigationBGM();
            }

            // Build Match-3 Grid
            StartCoroutine(InitializeBoardRoutine());
        }

        private void SetupLevelBalancing(int stage, int level)
        {
            if (stage == 1 && level == 1)
            {
                TargetMaxHP = 500;
                MovesRemaining = 24;
            }
            else if (stage == 1 && level == 2)
            {
                TargetMaxHP = 750;
                MovesRemaining = 22;
            }
            else if (stage == 2 && level == 1)
            {
                TargetMaxHP = 900;
                MovesRemaining = 22;
            }
            else if (stage == 2 && level == 2)
            {
                TargetMaxHP = 1200;
                MovesRemaining = 20;
            }
            else if (stage == 3 && level == 1)
            {
                TargetMaxHP = 1500;
                MovesRemaining = 20;
            }
            else
            {
                TargetMaxHP = 2200;
                MovesRemaining = 25;
            }

            TargetCurrentHP = TargetMaxHP;

            if (enemyNameText != null)
            {
                string rank = (stage == 3) ? "KAPITALIS PEMBALAKAN" : (level == 2 ? "MANDOR HUTAN" : "INVESTIGASI LAWAN");
                enemyNameText.text = $"DOCTOR SAWIT MAN ({rank})";
            }

            if (enemyImage != null)
            {
                enemyImage.sprite = enemyIdleSprite;
                enemyImage.color = Color.white;
                enemyImage.transform.localScale = Vector3.one;
            }
        }

        private void SetupTileItemDefinitions(int stage, int level)
        {
            currentTileTypes.Clear();

            // 5 Candy / Gem Placeholder Element Types with high-contrast, juicy Candy Crush colors:
            // 0: Red Ruby (Candy Heart/Apple) - Symbol: "🍎"
            // 1: Blue Sapphire (Water Drop) - Symbol: "💧"
            // 2: Green Emerald (Leaf) - Symbol: "🌿"
            // 3: Yellow Topaz (Star) - Symbol: "⭐"
            // 4: Purple Amethyst (Diamond) - Symbol: "💎"
            string[] candySymbols = new string[] { "🍎", "💧", "🌿", "⭐", "💎" };
            Color[] candyColors = new Color[]
            {
                new Color(0.96f, 0.22f, 0.32f, 1f), // Red Ruby
                new Color(0.12f, 0.65f, 1f, 1f),    // Blue Sapphire
                new Color(0.14f, 0.88f, 0.42f, 1f), // Green Emerald
                new Color(1f, 0.84f, 0.12f, 1f),    // Yellow Topaz
                new Color(0.78f, 0.32f, 0.96f, 1f)  // Purple Amethyst
            };

            // Grab item data from CareStoryDatabase for this stage if available
            List<HiddenItemData> stageItems = null;
            if (stage == 1 && level == 1) stageItems = CareStoryDatabase.Stage1_Level1_Items;
            else if (stage == 1 && level == 2) stageItems = CareStoryDatabase.Stage1_Level2_Items;
            else if (stage == 2 && level == 1) stageItems = CareStoryDatabase.Stage2_Level1_Items;
            else if (stage == 2 && level == 2) stageItems = CareStoryDatabase.Stage2_Level2_Items;
            else if (stage == 3 && level == 1) stageItems = CareStoryDatabase.Stage3_Level1_Items;
            else if (stage == 3 && level == 2) stageItems = CareStoryDatabase.Stage3_Level2_Items;

            if (stageItems != null && stageItems.Count >= 5)
            {
                string folder = $"TemporaryAssets/HiddenObjects/Stage{stage}_Level{level}/";
                for (int i = 0; i < 5; i++)
                {
                    Sprite s = Resources.Load<Sprite>(folder + stageItems[i].spriteName);
                    if (s == null)
                    {
                        // Fallback to Stage 1 Level 1 items
                        s = Resources.Load<Sprite>("TemporaryAssets/HiddenObjects/Stage1_Level1/" + stageItems[i].spriteName);
                    }
                    currentTileTypes.Add(new Match3ItemDef(i, stageItems[i].itemName, candySymbols[i], s, candyColors[i]));
                }
            }
            else
            {
                // Fallback default set from Stage 1 Level 1
                string[] defaultSprites = new string[] { "kotak_p3k", "botol_air", "kain_bersih", "peta_kawasan", "jejak_kaki_satwa" };
                string[] defaultNames = new string[] { "Kotak P3K", "Botol Air", "Kain Bersih", "Peta Kawasan", "Jejak Satwa" };

                for (int i = 0; i < 5; i++)
                {
                    Sprite s = Resources.Load<Sprite>("TemporaryAssets/HiddenObjects/Stage1_Level1/" + defaultSprites[i]);
                    currentTileTypes.Add(new Match3ItemDef(i, defaultNames[i], candySymbols[i], s, candyColors[i]));
                }
            }
        }

        #endregion

        #region Board Generation

        private IEnumerator InitializeBoardRoutine()
        {
            CanPlayerMove = false;
            selectedTile = null;

            // Clear old tiles
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    if (grid[x, y] != null)
                    {
                        Destroy(grid[x, y].gameObject);
                        grid[x, y] = null;
                    }
                }
            }

            yield return null;

            // Fill grid with random types, guaranteeing no initial 3-matches
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    List<int> validTypes = new List<int>();
                    for (int t = 0; t < currentTileTypes.Count; t++)
                    {
                        // Check horizontal match-3
                        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null)
                        {
                            if (grid[x - 1, y].TypeId == t && grid[x - 2, y].TypeId == t)
                                continue;
                        }

                        // Check vertical match-3
                        if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null)
                        {
                            if (grid[x, y - 1].TypeId == t && grid[x, y - 2].TypeId == t)
                                continue;
                        }

                        validTypes.Add(t);
                    }

                    int chosenType = validTypes[UnityEngine.Random.Range(0, validTypes.Count)];
                    SpawnTileAt(x, y, chosenType, false);
                }
            }

            PlaySound(sfxWhoosh, 0.7f);
            yield return new WaitForSeconds(0.2f);

            CanPlayerMove = true;
            Debug.Log($"[Match3CombatManager] Board initialized with {COLUMNS}x{ROWS} candy tiles. Ready for player input!");
        }

        private Match3Tile SpawnTileAt(int x, int y, int typeId, bool animateDrop = true)
        {
            Vector2 targetPos = CalculateTilePosition(x, y);
            Vector2 startPos = animateDrop ? new Vector2(targetPos.x, targetPos.y + 400f) : targetPos;

            Match3ItemDef def = currentTileTypes[typeId];
            Match3Tile tile = Match3Tile.Spawn(boardContainer, x, y, typeId, def.sprite, def.itemName, def.symbol, def.themeColor, startPos, new Vector2(TILE_SIZE, TILE_SIZE));

            grid[x, y] = tile;

            if (animateDrop)
            {
                tile.DropTo(targetPos, 0.28f);
            }

            return tile;
        }

        public Vector2 CalculateTilePosition(int x, int y)
        {
            // Center grid around (0, 0) of boardContainer
            float totalWidth = COLUMNS * TILE_SIZE + (COLUMNS - 1) * TILE_SPACING;
            float totalHeight = ROWS * TILE_SIZE + (ROWS - 1) * TILE_SPACING;

            float startX = -totalWidth * 0.5f + TILE_SIZE * 0.5f;
            float startY = -totalHeight * 0.5f + TILE_SIZE * 0.5f;

            float posX = startX + x * (TILE_SIZE + TILE_SPACING);
            float posY = startY + y * (TILE_SIZE + TILE_SPACING);

            return new Vector2(posX, posY);
        }

        #endregion

        #region Player Interaction & Tile Swapping

        public void OnTileClicked(Match3Tile tile)
        {
            if (!CanPlayerMove || tile == null) return;

            if (selectedTile == null)
            {
                selectedTile = tile;
                selectedTile.SetSelected(true);
                PlaySound(sfxClick, 0.8f);
            }
            else if (selectedTile == tile)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
            else
            {
                // Check if adjacent
                int dx = Mathf.Abs(selectedTile.GridX - tile.GridX);
                int dy = Mathf.Abs(selectedTile.GridY - tile.GridY);

                if (dx + dy == 1)
                {
                    Match3Tile first = selectedTile;
                    selectedTile.SetSelected(false);
                    selectedTile = null;

                    TrySwapTiles(first.GridX, first.GridY, tile.GridX, tile.GridY);
                }
                else
                {
                    selectedTile.SetSelected(false);
                    selectedTile = tile;
                    selectedTile.SetSelected(true);
                    PlaySound(sfxClick, 0.8f);
                }
            }
        }

        public void TrySwapTiles(int x1, int y1, int x2, int y2)
        {
            if (!CanPlayerMove) return;

            // Boundary validation
            if (x1 < 0 || x1 >= COLUMNS || y1 < 0 || y1 >= ROWS) return;
            if (x2 < 0 || x2 >= COLUMNS || y2 < 0 || y2 >= ROWS) return;

            if (selectedTile != null)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
            }

            StartCoroutine(SwapAndProcessRoutine(x1, y1, x2, y2));
        }

        private IEnumerator SwapAndProcessRoutine(int x1, int y1, int x2, int y2)
        {
            CanPlayerMove = false;

            Match3Tile tileA = grid[x1, y1];
            Match3Tile tileB = grid[x2, y2];

            if (tileA == null || tileB == null)
            {
                CanPlayerMove = true;
                yield break;
            }

            PlaySound(sfxWhoosh, 0.6f);

            // Swap in grid array
            grid[x1, y1] = tileB;
            grid[x2, y2] = tileA;

            tileA.SetGridPosition(x2, y2);
            tileB.SetGridPosition(x1, y1);

            Vector2 posA = CalculateTilePosition(x1, y1);
            Vector2 posB = CalculateTilePosition(x2, y2);

            // Animate swap
            tileA.MoveTo(posB, 0.16f);
            tileB.MoveTo(posA, 0.16f);

            yield return new WaitForSeconds(0.18f);

            // Check if matches are formed
            List<Match3Tile> matches = FindAllMatches();

            if (matches.Count > 0)
            {
                // Valid move!
                MovesRemaining--;
                CurrentCombo = 1;
                UpdateHeaderUI();

                // Process matches, attack enemy, and handle cascades
                yield return ProcessMatchesAndCascades(matches);

                // Check victory or defeat conditions
                if (TargetCurrentHP <= 0)
                {
                    yield return HandleVictorySequence();
                }
                else if (MovesRemaining <= 0)
                {
                    yield return HandleDefeatSequence();
                }
                else
                {
                    CanPlayerMove = true;
                }
            }
            else
            {
                // Invalid move: swap back
                PlaySound(sfxClick, 0.7f);

                grid[x1, y1] = tileA;
                grid[x2, y2] = tileB;

                tileA.SetGridPosition(x1, y1);
                tileB.SetGridPosition(x2, y2);

                tileA.MoveTo(posA, 0.16f);
                tileB.MoveTo(posB, 0.16f);

                yield return new WaitForSeconds(0.18f);
                CanPlayerMove = true;
            }
        }

        #endregion

        #region Match Finding & Attacks

        private List<Match3Tile> FindAllMatches()
        {
            HashSet<Match3Tile> matchedSet = new HashSet<Match3Tile>();

            // 1. Horizontal Check
            for (int y = 0; y < ROWS; y++)
            {
                int matchCount = 1;
                for (int x = 0; x < COLUMNS; x++)
                {
                    if (x < COLUMNS - 1 && grid[x, y] != null && grid[x + 1, y] != null && grid[x, y].TypeId == grid[x + 1, y].TypeId)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int k = 0; k < matchCount; k++)
                            {
                                matchedSet.Add(grid[x - k, y]);
                            }
                        }
                        matchCount = 1;
                    }
                }
            }

            // 2. Vertical Check
            for (int x = 0; x < COLUMNS; x++)
            {
                int matchCount = 1;
                for (int y = 0; y < ROWS; y++)
                {
                    if (y < ROWS - 1 && grid[x, y] != null && grid[x, y + 1] != null && grid[x, y].TypeId == grid[x, y + 1].TypeId)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int k = 0; k < matchCount; k++)
                            {
                                matchedSet.Add(grid[x, y - k]);
                            }
                        }
                        matchCount = 1;
                    }
                }
            }

            return new List<Match3Tile>(matchedSet);
        }

        private IEnumerator ProcessMatchesAndCascades(List<Match3Tile> matchedTiles)
        {
            while (matchedTiles != null && matchedTiles.Count > 0)
            {
                // Play match chime
                if (CurrentCombo > 1)
                {
                    PlaySound(sfxBonus, 0.85f);
                    ShowComboBanner($"COMBO x{CurrentCombo}!");
                }
                else
                {
                    PlaySound(sfxBell, 0.75f);
                    HideComboBanner();
                }

                if (CurrentCombo > MaxComboReached) MaxComboReached = CurrentCombo;

                // Base damage: 25 per tile, multiplied by combo
                int baseTileDamage = 25;
                float comboMultiplier = 1f + (CurrentCombo - 1) * 0.5f;
                int damagePerTile = Mathf.RoundToInt(baseTileDamage * comboMultiplier);

                // Boosted EXP Calculation
                int baseTileExp = 250;
                int matchBonus = 0;
                if (matchedTiles.Count == 4) matchBonus = 500;
                else if (matchedTiles.Count >= 5) matchBonus = 1500;

                int earnedExp = (matchedTiles.Count * baseTileExp + matchBonus) * CurrentCombo;
                Score += earnedExp;
                UpdateHeaderUI();

                // Spawn floating EXP popup above board
                SpawnFloatingExp($"+{earnedExp:N0} EXP!", new Vector2(0f, -80f));

                Transform projParent = projectileLayer != null ? projectileLayer : combatPanel.transform;

                // Target center in Canvas coordinate space
                Vector2 targetEnemyPos = (enemyImage != null) ?
                    (Vector2)projParent.InverseTransformPoint(enemyImage.transform.position) :
                    new Vector2(0, 240);

                // Launch Attack Projectiles from combined items straight to the enemy above!
                foreach (var tile in matchedTiles)
                {
                    if (tile == null) continue;

                    Vector2 tileWorldPos = (Vector2)projParent.InverseTransformPoint(tile.transform.position);

                    Match3Projectile.Spawn(
                        projParent,
                        tileWorldPos,
                        targetEnemyPos,
                        tile.ItemSprite,
                        tile.Symbol,
                        tile.ThemeColor,
                        damagePerTile,
                        CurrentCombo,
                        OnEnemyHit
                    );

                    // Clear from grid array
                    grid[tile.GridX, tile.GridY] = null;
                    tile.AnimateMatchAndDestroy(0.24f);
                }

                yield return new WaitForSeconds(0.28f);

                // Gravity: drop tiles down
                yield return ApplyGravityRoutine();

                // Check for cascading matches
                yield return new WaitForSeconds(0.15f);
                matchedTiles = FindAllMatches();
                if (matchedTiles.Count > 0)
                {
                    CurrentCombo++;
                }
            }

            HideComboBanner();
        }

        private IEnumerator ApplyGravityRoutine()
        {
            float dropDuration = 0.22f;

            for (int x = 0; x < COLUMNS; x++)
            {
                int emptyCount = 0;
                for (int y = 0; y < ROWS; y++)
                {
                    if (grid[x, y] == null)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        // Move down by emptyCount
                        int newY = y - emptyCount;
                        Match3Tile tile = grid[x, y];
                        grid[x, y] = null;
                        grid[x, newY] = tile;
                        tile.SetGridPosition(x, newY);

                        Vector2 targetPos = CalculateTilePosition(x, newY);
                        tile.DropTo(targetPos, dropDuration);
                    }
                }

                // Refill top vacated spots with new random tiles
                for (int i = 0; i < emptyCount; i++)
                {
                    int spawnY = ROWS - emptyCount + i;
                    int randomType = UnityEngine.Random.Range(0, currentTileTypes.Count);
                    SpawnTileAt(x, spawnY, randomType, true);
                }
            }

            yield return new WaitForSeconds(dropDuration + 0.05f);
        }

        #endregion

        #region Enemy Damage & Reaction

        public void OnEnemyHit(int damage, int combo, Vector2 hitPos, Color hitColor)
        {
            if (isEnemyDefeated) return;

            PlaySound(sfxHit, 0.9f);

            // Deduct HP
            TargetCurrentHP = Mathf.Max(0, TargetCurrentHP - damage);
            UpdateHPBarUI(false);

            // Spawn Impact Explosion Particles & Shockwave
            SpawnImpactExplosion(hitPos, hitColor);

            // Floating Damage Number
            SpawnFloatingDamage(damage, combo, hitPos);

            // Punchy Enemy Shake & Red Flash
            StartCoroutine(EnemyHitReactionRoutine());

            // Switch to enraged sprite if low HP
            if (TargetCurrentHP <= TargetMaxHP * 0.35f && enemyImage != null && enemyEnragedSprite != null)
            {
                enemyImage.sprite = enemyEnragedSprite;
            }
        }

        private void SpawnImpactExplosion(Vector2 pos, Color color)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;

            // 1. Expanding Shockwave Ring
            GameObject ringObj = new GameObject("ImpactShockwave", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ringObj.transform.SetParent(layer, false);
            RectTransform ringRT = ringObj.GetComponent<RectTransform>();
            ringRT.sizeDelta = new Vector2(40f, 40f);
            ringRT.anchoredPosition = pos;
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = GetWhiteSprite();
            ringImg.color = new Color(color.r, color.g, color.b, 0.9f);
            ringImg.raycastTarget = false;
            StartCoroutine(AnimateShockwave(ringRT, ringImg));

            // 2. Radial Spark Particles (8 bursts radiating outward)
            int particleCount = 8;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = i * (360f / particleCount) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float speed = UnityEngine.Random.Range(120f, 210f);

                GameObject sparkObj = new GameObject($"Spark_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                sparkObj.transform.SetParent(layer, false);
                RectTransform sparkRT = sparkObj.GetComponent<RectTransform>();
                sparkRT.sizeDelta = new Vector2(16f, 16f);
                sparkRT.anchoredPosition = pos;
                Image sparkImg = sparkObj.GetComponent<Image>();
                sparkImg.sprite = GetWhiteSprite();
                sparkImg.color = Color.Lerp(color, Color.white, 0.5f);
                sparkImg.raycastTarget = false;

                StartCoroutine(AnimateSpark(sparkRT, sparkImg, dir * speed));
            }
        }

        private IEnumerator AnimateShockwave(RectTransform rt, Image img)
        {
            float duration = 0.28f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.4f;
            Vector3 endScale = Vector3.one * 2.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.localScale = Vector3.Lerp(startScale, endScale, t);
                Color c = img.color;
                c.a = Mathf.Lerp(0.9f, 0f, t * t);
                img.color = c;
                yield return null;
            }
            Destroy(rt.gameObject);
        }

        private IEnumerator AnimateSpark(RectTransform rt, Image img, Vector2 velocity)
        {
            float duration = 0.35f;
            float elapsed = 0f;
            Vector2 currentPos = rt.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                currentPos += velocity * Time.deltaTime;
                velocity *= 0.92f;
                rt.anchoredPosition = currentPos;

                float s = Mathf.Lerp(1.2f, 0f, t);
                rt.localScale = new Vector3(s, s, 1f);

                Color c = img.color;
                c.a = 1f - t;
                img.color = c;

                yield return null;
            }
            Destroy(rt.gameObject);
        }

        private void SpawnFloatingExp(string expText, Vector2 pos)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;

            GameObject expObj = new GameObject("ExpPopup", typeof(RectTransform));
            expObj.transform.SetParent(layer, false);
            RectTransform rt = expObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, 60);
            rt.anchoredPosition = pos;

            Text txt = expObj.AddComponent<Text>();
            txt.font = GetGameFont();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            txt.text = $"🌟 {expText}";
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(1f, 0.92f, 0.25f, 1f);

            StartCoroutine(AnimateFloatingExpText(rt, txt));
        }

        private IEnumerator AnimateFloatingExpText(RectTransform rt, Text txt)
        {
            float duration = 0.85f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, 80f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                float scale = t < 0.25f ? Mathf.Lerp(0.7f, 1.35f, t / 0.25f) : Mathf.Lerp(1.35f, 1f, (t - 0.25f) / 0.75f);
                rt.localScale = new Vector3(scale, scale, 1f);

                Color c = txt.color;
                c.a = 1f - (t * t);
                txt.color = c;

                yield return null;
            }

            Destroy(rt.gameObject);
        }

        private IEnumerator EnemyHitReactionRoutine()
        {
            if (enemyImage == null) yield break;

            enemyImage.color = new Color(1f, 0.35f, 0.35f, 1f);

            float elapsed = 0f;
            float duration = 0.16f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float rx = UnityEngine.Random.Range(-16f, 16f);
                float ry = UnityEngine.Random.Range(-12f, 12f);
                if (enemyRect != null)
                {
                    enemyRect.anchoredPosition = enemyBasePos + new Vector2(rx, ry);
                }
                yield return null;
            }

            if (enemyRect != null) enemyRect.anchoredPosition = enemyBasePos;
            enemyImage.color = Color.white;
        }

        private void SpawnFloatingDamage(int damage, int combo, Vector2 pos)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;

            GameObject dmgObj = new GameObject("DmgText", typeof(RectTransform));
            dmgObj.transform.SetParent(layer, false);
            RectTransform rt = dmgObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(320, 60);

            // Position slightly jittered above enemy
            float rx = UnityEngine.Random.Range(-40f, 40f);
            float ry = UnityEngine.Random.Range(-15f, 25f);
            rt.anchoredPosition = new Vector2(pos.x + rx, pos.y + 40f + ry);

            Text txt = dmgObj.AddComponent<Text>();
            txt.font = GetGameFont();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            if (combo > 1)
            {
                txt.text = $"⚡ KRITIKAL! -{damage}";
                txt.fontSize = 32;
                txt.fontStyle = FontStyle.Bold;
                txt.color = new Color(1f, 0.92f, 0.25f, 1f); // Vibrant gold
            }
            else
            {
                txt.text = $"💥 -{damage}";
                txt.fontSize = 28;
                txt.fontStyle = FontStyle.Bold;
                txt.color = new Color(1f, 0.35f, 0.35f, 1f); // Bright red
            }

            StartCoroutine(AnimateFloatingText(rt, txt));
        }

        private IEnumerator AnimateFloatingText(RectTransform rt, Text txt)
        {
            float duration = 0.75f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, 65f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                float scale = t < 0.2f ? Mathf.Lerp(0.8f, 1.25f, t / 0.2f) : Mathf.Lerp(1.25f, 1f, (t - 0.2f) / 0.8f);
                rt.localScale = new Vector3(scale, scale, 1f);

                Color c = txt.color;
                c.a = 1f - (t * t);
                txt.color = c;
                yield return null;
            }

            Destroy(rt.gameObject);
        }

        #endregion

        #region Victory & Defeat Sequences

        private IEnumerator HandleVictorySequence()
        {
            isEnemyDefeated = true;
            CanPlayerMove = false;

            PlaySound(sfxBonus, 1f);

            // Enemy Defeat Animation (shrink & fade)
            if (enemyImage != null)
            {
                float elapsed = 0f;
                float duration = 0.8f;
                Vector3 baseScale = enemyImage.transform.localScale;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    enemyImage.transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t);
                    enemyImage.transform.localEulerAngles = new Vector3(0, 0, t * 180f);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.4f);

            if (victoryModal != null)
            {
                victoryModal.SetActive(true);
                if (victoryStatsText != null)
                {
                    victoryStatsText.text = $"Target Berhasil Ditumbangkan!\n\n" +
                                            $"• Sisa Langkah: {MovesRemaining}\n" +
                                            $"• Total EXP Diperoleh: 🌟 +{Score:N0} EXP\n" +
                                            $"• Combo Maksimal: x{MaxComboReached}\n\n" +
                                            $"Kombinasi barang bukti forensik berhasil memukul mundur Doctor Sawit Man!";
                }
            }
        }

        private IEnumerator HandleDefeatSequence()
        {
            CanPlayerMove = false;
            yield return new WaitForSeconds(0.4f);

            if (defeatModal != null)
            {
                defeatModal.SetActive(true);
            }
        }

        private void OnVictoryContinueClicked()
        {
            if (victoryModal != null) victoryModal.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);

            onCombatCompleteCallback?.Invoke();
        }

        private void OnRetryClicked()
        {
            if (defeatModal != null) defeatModal.SetActive(false);
            StartCombat(currentStageNumber, currentLevelNumber, currentLocationName, onCombatCompleteCallback);
        }

        #endregion

        #region UI Updates & Helpers

        private void UpdateHeaderUI()
        {
            if (stageTitleText != null)
            {
                stageTitleText.text = $"⚔️ TAHAP {currentStageNumber} - MISI {currentLevelNumber}: {currentLocationName.ToUpper()}";
            }

            if (movesCounterText != null)
            {
                movesCounterText.text = $"🎯 LANGKAH: {MovesRemaining}";
                movesCounterText.color = MovesRemaining <= 5 ? new Color(1f, 0.35f, 0.35f) : Color.white;
            }

            if (scoreText != null)
            {
                scoreText.text = $"🌟 EXP: {Score:N0}";
            }
        }

        private static Sprite whiteSprite;
        public static Sprite GetWhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            return whiteSprite;
        }

        private Coroutine hpCoroutine;
        private void UpdateHPBarUI(bool instant = false)
        {
            float fillRatio = Mathf.Clamp01((float)TargetCurrentHP / TargetMaxHP);

            if (hpBarFill != null)
            {
                if (hpCoroutine != null) StopCoroutine(hpCoroutine);

                if (instant)
                {
                    ApplyHPBarRatio(fillRatio);
                }
                else
                {
                    hpCoroutine = StartCoroutine(SmoothHPBarRoutine(fillRatio));
                }
            }

            if (hpText != null)
            {
                hpText.text = $"HP: {TargetCurrentHP} / {TargetMaxHP} ({Mathf.RoundToInt(fillRatio * 100f)}%)";
            }
        }

        private void ApplyHPBarRatio(float ratio)
        {
            if (hpBarFill == null) return;

            // Ensure sprite is assigned for Image.Type.Filled
            if (hpBarFill.sprite == null)
            {
                hpBarFill.sprite = GetWhiteSprite();
            }

            // 1. Image.Type.Filled fillAmount
            hpBarFill.fillAmount = ratio;

            // 2. Double-guarantee: physically shrink RectTransform anchorMax.x from 1.0 down to ratio
            RectTransform rt = hpBarFill.rectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(ratio, 1f);
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
            }

            // 3. Dynamic color shift: Green -> Yellow -> Red
            Color barColor;
            if (ratio > 0.5f)
            {
                float t = (ratio - 0.5f) / 0.5f;
                barColor = Color.Lerp(new Color(0.95f, 0.8f, 0.15f), new Color(0.2f, 0.88f, 0.38f), t);
            }
            else
            {
                float t = ratio / 0.5f;
                barColor = Color.Lerp(new Color(0.92f, 0.22f, 0.22f), new Color(0.95f, 0.8f, 0.15f), t);
            }
            hpBarFill.color = barColor;
        }

        private IEnumerator SmoothHPBarRoutine(float targetFill)
        {
            float startFill = hpBarFill.fillAmount;
            if (hpBarFill.rectTransform != null && hpBarFill.rectTransform.anchorMax.x > 0f)
            {
                startFill = hpBarFill.rectTransform.anchorMax.x;
            }

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float current = Mathf.Lerp(startFill, targetFill, elapsed / duration);
                ApplyHPBarRatio(current);
                yield return null;
            }

            ApplyHPBarRatio(targetFill);
            hpCoroutine = null;
        }

        private static Font cachedFont;
        public static Font GetGameFont()
        {
            if (cachedFont != null) return cachedFont;
            try { cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
            if (cachedFont == null) { try { cachedFont = Resources.Load<Font>("LegacyRuntime.ttf"); } catch {} }
            if (cachedFont == null) { try { cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 16); } catch {} }
            return cachedFont;
        }

        private void ShowComboBanner(string text)
        {
            if (comboBannerText != null)
            {
                comboBannerText.gameObject.SetActive(true);
                comboBannerText.text = text;
            }
        }

        private void HideComboBanner()
        {
            if (comboBannerText != null)
            {
                comboBannerText.gameObject.SetActive(false);
            }
        }

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip != null && CareAudioManager.Instance != null)
            {
                CareAudioManager.Instance.PlaySFX(clip, volume);
            }
        }

        #endregion
    }
}
