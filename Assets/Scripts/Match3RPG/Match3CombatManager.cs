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
    /// Grid format: 15x6 (15 Columns x 6 Rows).
    /// Features:
    /// - Total Damage Accumulated (Total DMG: N)
    /// - Turns Counter (Turns: remaining/max)
    /// - Real-time Countdown Timer (Time: MM:SS)
    /// - Difficulty Scaling: higher stages have fewer turns, less time, faster boss patrol, and rigged spawn chances
    /// - Fair combo validation: guaranteed valid moves always exist (automatic reshuffle if no moves)
    /// - Moving Boss with real-time Hitbox and vertical Rocket attack collision
    /// </summary>
    public class Match3CombatManager : MonoBehaviour
    {
        public static Match3CombatManager Instance { get; private set; }

        [Header("Board Configuration (15x6 Grid)")]
        public const int COLUMNS = 15;
        public const int ROWS = 6;
        public const float TILE_SIZE = 76f;
        public const float TILE_SPACING = 6f;

        [Header("Boss Patrol & Hitbox Settings")]
        public float bossPatrolSpeed = 220f;
        public float BossHitboxWidth = 200f;
        public float BossHitboxHeight = 220f;
        public float BossHitboxTop => bossBaseY + BossHitboxHeight * 0.5f;
        public float BossHitboxBottom => bossBaseY - BossHitboxHeight * 0.5f;

        private float bossMinX = -574f;
        private float bossMaxX = 574f;
        private int bossMoveDirection = -1; // -1: right-to-left, 1: left-to-right
        private float bossCurrentX = 574f;
        private float bossBaseY = 220f;
        private bool isBossMoving = true;
        private GameObject hitboxIndicatorObj;

        // Runtime Stats
        public bool CanPlayerMove { get; private set; } = false;
        public int MaxTurns { get; private set; } = 24;
        public int MovesRemaining { get; private set; } = 24;
        public int TotalDamageAccumulated { get; private set; } = 0;
        public float TimeRemaining { get; private set; } = 45f;
        public float MaxLevelTime { get; private set; } = 45f;
        public bool IsTimerRunning { get; private set; } = false;

        public int Score { get; private set; }
        public int CurrentCombo { get; private set; } = 1;
        public int MaxComboReached { get; private set; } = 1;

        // Difficulty Tuning
        private int activeTileTypeCount = 5;
        private float riggedSpawnProbability = 0f;

        // Target / Enemy Stats
        public int TargetMaxHP { get; private set; } = 600;
        public int TargetCurrentHP { get; private set; } = 600;

        // UI Panel references
        private GameObject combatPanel;
        private RectTransform boardContainer;
        private RectTransform projectileLayer;
        private RectTransform damageTextLayer;

        // Header & Live Combat HUD UI
        private Text stageTitleText;
        private Text totalDmgText;
        private Text turnsText;
        private Text timerText;
        private Text patrolSpeedText;
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
        private Text defeatTitleText;
        private Text defeatDescText;
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
            // 1. Timer Countdown
            if (IsTimerRunning && !isEnemyDefeated && combatPanel != null && combatPanel.activeSelf)
            {
                TimeRemaining -= Time.deltaTime;
                if (TimeRemaining <= 0f)
                {
                    TimeRemaining = 0f;
                    IsTimerRunning = false;
                    StartCoroutine(HandleTimeOutDefeat());
                }
                UpdateTimerUI();
            }

            // 2. Boss Patrol Horizontal Movement
            if (enemyRect != null && !isEnemyDefeated && combatPanel != null && combatPanel.activeSelf)
            {
                if (isBossMoving)
                {
                    bossCurrentX += bossMoveDirection * bossPatrolSpeed * Time.deltaTime;

                    if (bossCurrentX <= bossMinX)
                    {
                        bossCurrentX = bossMinX;
                        bossMoveDirection = 1; // Turn right
                    }
                    else if (bossCurrentX >= bossMaxX)
                    {
                        bossCurrentX = bossMaxX;
                        bossMoveDirection = -1; // Turn left
                    }
                }

                float walkBob = Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 8f;
                enemyRect.anchoredPosition = new Vector2(bossCurrentX, bossBaseY + walkBob);

                float targetFacing = bossMoveDirection > 0 ? -1f : 1f;
                Vector3 currentScale = enemyRect.localScale;
                currentScale.x = targetFacing;
                currentScale.y = 1f;
                currentScale.z = 1f;
                enemyRect.localScale = currentScale;
                enemyRect.localEulerAngles = Vector3.zero;
            }

            // 3. Live Combat HUD real-time updates
            if (patrolSpeedText != null && combatPanel != null && combatPanel.activeSelf)
            {
                patrolSpeedText.text = $"{bossPatrolSpeed:F0} px/s";
            }
        }

        #region Hitbox & Collision Detection

        public bool CheckEnemyHit(Vector2 projPos, float radius = 28f)
        {
            if (isEnemyDefeated || enemyRect == null || combatPanel == null || !combatPanel.activeSelf)
                return false;

            float bossX = enemyRect.anchoredPosition.x;
            float bossY = enemyRect.anchoredPosition.y;

            float halfW = (BossHitboxWidth * 0.5f) + radius;
            float halfH = (BossHitboxHeight * 0.5f) + radius;

            bool xOverlap = Mathf.Abs(projPos.x - bossX) <= halfW;
            bool yOverlap = Mathf.Abs(projPos.y - bossY) <= halfH;

            return xOverlap && yOverlap;
        }

        private void CreateHitboxIndicator()
        {
            if (enemyRect == null) return;

            Transform existing = enemyRect.transform.Find("HitboxTargetFrame");
            if (existing != null) Destroy(existing.gameObject);

            hitboxIndicatorObj = new GameObject("HitboxTargetFrame", typeof(RectTransform));
            hitboxIndicatorObj.transform.SetParent(enemyRect.transform, false);
            RectTransform frt = hitboxIndicatorObj.GetComponent<RectTransform>();
            frt.sizeDelta = new Vector2(BossHitboxWidth, BossHitboxHeight);
            frt.anchoredPosition = Vector2.zero;
            frt.localEulerAngles = Vector3.zero;
            frt.localScale = Vector3.one;

            Sprite white = GetWhiteSprite();

            // Left Target Bracket [
            GameObject leftBracket = new GameObject("LeftBracket", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            leftBracket.transform.SetParent(hitboxIndicatorObj.transform, false);
            RectTransform lrt = leftBracket.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(6f, BossHitboxHeight * 0.7f);
            lrt.anchoredPosition = new Vector2(-BossHitboxWidth * 0.5f, 0);
            Image lImg = leftBracket.GetComponent<Image>();
            lImg.sprite = white;
            lImg.color = new Color(1f, 0.3f, 0.3f, 0.7f);
            lImg.raycastTarget = false;

            // Right Target Bracket ]
            GameObject rightBracket = new GameObject("RightBracket", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rightBracket.transform.SetParent(hitboxIndicatorObj.transform, false);
            RectTransform rrt = rightBracket.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(6f, BossHitboxHeight * 0.7f);
            rrt.anchoredPosition = new Vector2(BossHitboxWidth * 0.5f, 0);
            Image rImg = rightBracket.GetComponent<Image>();
            rImg.sprite = white;
            rImg.color = new Color(1f, 0.3f, 0.3f, 0.7f);
            rImg.raycastTarget = false;

            // Ground Target Zone Ring at feet
            GameObject ringObj = new GameObject("FootTargetZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ringObj.transform.SetParent(hitboxIndicatorObj.transform, false);
            RectTransform ringRT = ringObj.GetComponent<RectTransform>();
            ringRT.sizeDelta = new Vector2(BossHitboxWidth, 34f);
            ringRT.anchoredPosition = new Vector2(0, -BossHitboxHeight * 0.5f + 16f);
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = white;
            ringImg.color = new Color(1f, 0.22f, 0.22f, 0.42f);
            ringImg.raycastTarget = false;

            // Target Label
            GameObject txtObj = new GameObject("TargetLabel", typeof(RectTransform));
            txtObj.transform.SetParent(hitboxIndicatorObj.transform, false);
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(BossHitboxWidth, 24f);
            trt.anchoredPosition = new Vector2(0, -BossHitboxHeight * 0.5f + 16f);
            Text lbl = txtObj.AddComponent<Text>();
            lbl.font = GetGameFont();
            lbl.text = "🎯 TARGET";
            lbl.fontSize = 14;
            lbl.fontStyle = FontStyle.Bold;
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = new Color(1f, 0.95f, 0.5f, 0.85f);
            lbl.raycastTarget = false;
        }

        #endregion

        #region Setup UI Wiring

        public void SetupUI(
            GameObject panel,
            RectTransform boardTrans,
            RectTransform projLayer,
            RectTransform dmgLayer,
            Text titleTxt,
            Text totalDmgTxt,
            Text turnsTxt,
            Text timerTxt,
            Text patrolSpeedTxt,
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
            Text loseTitleTxt,
            Text loseDescTxt,
            Button loseRetryBtn,
            Button loseExitBtn)
        {
            combatPanel = panel;
            boardContainer = boardTrans;
            projectileLayer = projLayer;
            damageTextLayer = dmgLayer;

            stageTitleText = titleTxt;
            totalDmgText = totalDmgTxt;
            turnsText = turnsTxt;
            timerText = timerTxt;
            patrolSpeedText = patrolSpeedTxt;
            scoreText = scoreTxt;
            comboBannerText = comboTxt;
            exitButton = exitBtn;

            enemyImage = enemyImg;
            if (enemyImage != null)
            {
                enemyRect = enemyImage.GetComponent<RectTransform>();
                if (enemyRect != null)
                {
                    enemyBasePos = enemyRect.anchoredPosition;
                    bossBaseY = enemyBasePos.y;
                    enemyRect.localScale = Vector3.one;
                    enemyRect.localEulerAngles = Vector3.zero;
                }
                enemyImage.transform.localScale = Vector3.one;
                enemyImage.transform.localEulerAngles = Vector3.zero;
            }
            hpBarFill = hpFill;
            hpText = hpTxt;
            enemyNameText = enemyNameTxt;

            victoryModal = winModal;
            victoryStatsText = winStatsTxt;
            victoryContinueBtn = winBtn;

            defeatModal = loseModal;
            defeatTitleText = loseTitleTxt;
            defeatDescText = loseDescTxt;
            defeatRetryBtn = loseRetryBtn;
            defeatExitBtn = loseExitBtn;

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() =>
                {
                    IsTimerRunning = false;
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
                    IsTimerRunning = false;
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
        /// Starts Match-3 RPG Combat with difficulty scaling:
        /// - Turns decrease as difficulty rises
        /// - Timer decreases as difficulty rises
        /// - Boss movement speed accelerates
        /// - Rigged chances make accidental matches rarer, but legal combos are always guaranteed!
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
            isBossMoving = true;
            Score = 0;
            TotalDamageAccumulated = 0;
            CurrentCombo = 1;
            MaxComboReached = 1;

            // Configure Boss Patrol bounds based on 15x6 grid width
            bossMinX = CalculateTilePosition(0, 0).x;
            bossMaxX = CalculateTilePosition(COLUMNS - 1, 0).x;
            bossCurrentX = bossMaxX; // Starts on the right side
            bossMoveDirection = -1;  // Moves right-to-left first

            // Create target indicator
            CreateHitboxIndicator();

            // Configure Target HP, Turns, Timer, Boss Speed, and Rigging
            SetupLevelBalancing(stage, level);

            // Setup Criminal Case items for this stage
            SetupTileItemDefinitions(stage, level);

            // Update UI
            UpdateHeaderUI();
            UpdateHPBarUI(true);

            // Start Timer Countdown
            IsTimerRunning = true;

            // Play Combat BGM
            if (CareAudioManager.Instance != null)
            {
                if (level == 2 || stage == 3)
                    CareAudioManager.Instance.PlayBossBGM();
                else
                    CareAudioManager.Instance.PlayInvestigationBGM();
            }

            // Build Match-3 Grid (15x6)
            StartCoroutine(InitializeBoardRoutine());
        }

        private void SetupLevelBalancing(int stage, int level)
        {
            // Difficulty Scaling:
            // S1 L1: Easiest (24 turns, 60s, speed 190, 4 types, 0% rigged)
            // S1 L2: Easy-Medium (20 turns, 50s, speed 240, 5 types, 25% rigged)
            // S2 L1: Medium (18 turns, 42s, speed 300, 5 types, 45% rigged)
            // S2 L2: Medium-Hard (16 turns, 35s, speed 360, 5 types, 65% rigged)
            // S3 L1: Hard (14 turns, 30s, speed 440, 6 types, 80% rigged)
            // S3 L2: Boss Final / Extreme (12 turns, 25s, speed 520, 6 types, 90% rigged)

            if (stage == 1 && level == 1)
            {
                TargetMaxHP = 400;
                MaxTurns = 24;
                MaxLevelTime = 60f;
                bossPatrolSpeed = 190f;
                activeTileTypeCount = 4;
                riggedSpawnProbability = 0.0f;
            }
            else if (stage == 1 && level == 2)
            {
                TargetMaxHP = 550;
                MaxTurns = 20;
                MaxLevelTime = 50f;
                bossPatrolSpeed = 240f;
                activeTileTypeCount = 5;
                riggedSpawnProbability = 0.25f;
            }
            else if (stage == 2 && level == 1)
            {
                TargetMaxHP = 750;
                MaxTurns = 18;
                MaxLevelTime = 42f;
                bossPatrolSpeed = 300f;
                activeTileTypeCount = 5;
                riggedSpawnProbability = 0.45f;
            }
            else if (stage == 2 && level == 2)
            {
                TargetMaxHP = 950;
                MaxTurns = 16;
                MaxLevelTime = 35f;
                bossPatrolSpeed = 360f;
                activeTileTypeCount = 5;
                riggedSpawnProbability = 0.65f;
            }
            else if (stage == 3 && level == 1)
            {
                TargetMaxHP = 1200;
                MaxTurns = 14;
                MaxLevelTime = 30f;
                bossPatrolSpeed = 440f;
                activeTileTypeCount = 6;
                riggedSpawnProbability = 0.80f;
            }
            else // Stage 3 Level 2 (Final Boss)
            {
                TargetMaxHP = 1600;
                MaxTurns = 12;
                MaxLevelTime = 25f;
                bossPatrolSpeed = 520f;
                activeTileTypeCount = 6;
                riggedSpawnProbability = 0.90f;
            }

            MovesRemaining = MaxTurns;
            TimeRemaining = MaxLevelTime;
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
                enemyImage.transform.localEulerAngles = Vector3.zero;
            }

            if (enemyRect != null)
            {
                enemyRect.localScale = Vector3.one;
                enemyRect.localEulerAngles = Vector3.zero;
            }
        }

        private void SetupTileItemDefinitions(int stage, int level)
        {
            currentTileTypes.Clear();

            // Up to 6 Candy / Gem Element Types with high-contrast, juicy colors:
            string[] candySymbols = new string[] { "🍎", "💧", "🌿", "⭐", "💎", "🔥" };
            Color[] candyColors = new Color[]
            {
                new Color(0.96f, 0.22f, 0.32f, 1f), // Red Ruby
                new Color(0.12f, 0.65f, 1f, 1f),    // Blue Sapphire
                new Color(0.14f, 0.88f, 0.42f, 1f), // Green Emerald
                new Color(1f, 0.84f, 0.12f, 1f),    // Yellow Topaz
                new Color(0.78f, 0.32f, 0.96f, 1f), // Purple Amethyst
                new Color(1f, 0.48f, 0.12f, 1f)     // Fire Orange Amber
            };

            // Grab item data from CareStoryDatabase for this stage if available
            List<HiddenItemData> stageItems = null;
            if (stage == 1 && level == 1) stageItems = CareStoryDatabase.Stage1_Level1_Items;
            else if (stage == 1 && level == 2) stageItems = CareStoryDatabase.Stage1_Level2_Items;
            else if (stage == 2 && level == 1) stageItems = CareStoryDatabase.Stage2_Level1_Items;
            else if (stage == 2 && level == 2) stageItems = CareStoryDatabase.Stage2_Level2_Items;
            else if (stage == 3 && level == 1) stageItems = CareStoryDatabase.Stage3_Level1_Items;
            else if (stage == 3 && level == 2) stageItems = CareStoryDatabase.Stage3_Level2_Items;

            int countToLoad = Mathf.Min(activeTileTypeCount, 6);

            for (int i = 0; i < countToLoad; i++)
            {
                Sprite s = null;
                string itemName = $"Item {i + 1}";

                if (stageItems != null && i < stageItems.Count)
                {
                    itemName = stageItems[i].itemName;
                    string folder = $"TemporaryAssets/HiddenObjects/Stage{stage}_Level{level}/";
                    s = Resources.Load<Sprite>(folder + stageItems[i].spriteName);
                    if (s == null)
                    {
                        s = Resources.Load<Sprite>("TemporaryAssets/HiddenObjects/Stage1_Level1/" + stageItems[i].spriteName);
                    }
                }
                else
                {
                    string[] defaultSprites = new string[] { "kotak_p3k", "botol_air", "kain_bersih", "peta_kawasan", "jejak_kaki_satwa", "kotak_p3k" };
                    string[] defaultNames = new string[] { "Kotak P3K", "Botol Air", "Kain Bersih", "Peta Kawasan", "Jejak Satwa", "Minyak Sawit" };
                    s = Resources.Load<Sprite>("TemporaryAssets/HiddenObjects/Stage1_Level1/" + defaultSprites[i]);
                    itemName = defaultNames[i];
                }

                currentTileTypes.Add(new Match3ItemDef(i, itemName, candySymbols[i], s, candyColors[i]));
            }
        }

        #endregion

        #region Board Generation & Rigged Spawning

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

            // Generate initial board with no 3-matches
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    List<int> validTypes = new List<int>();
                    for (int t = 0; t < currentTileTypes.Count; t++)
                    {
                        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null)
                        {
                            if (grid[x - 1, y].TypeId == t && grid[x - 2, y].TypeId == t)
                                continue;
                        }

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

            // Guarantee that at least one legal move exists!
            if (!HasPossibleValidMoves())
            {
                EnsureAtLeastOneMoveExists();
            }

            PlaySound(sfxWhoosh, 0.7f);
            yield return new WaitForSeconds(0.2f);

            CanPlayerMove = true;
            Debug.Log($"[Match3CombatManager] Board initialized with {COLUMNS}x{ROWS} candy tiles. Ready!");
        }

        private Match3Tile SpawnTileAt(int x, int y, int typeId, bool animateDrop = true)
        {
            Vector2 targetPos = CalculateTilePosition(x, y);
            Vector2 startPos = animateDrop ? new Vector2(targetPos.x, targetPos.y + 420f) : targetPos;

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
            float totalWidth = COLUMNS * TILE_SIZE + (COLUMNS - 1) * TILE_SPACING;
            float totalHeight = ROWS * TILE_SIZE + (ROWS - 1) * TILE_SPACING;

            float startX = -totalWidth * 0.5f + TILE_SIZE * 0.5f;
            float startY = -totalHeight * 0.5f + TILE_SIZE * 0.5f;

            float posX = startX + x * (TILE_SIZE + TILE_SPACING);
            float posY = startY + y * (TILE_SIZE + TILE_SPACING);

            return new Vector2(posX, posY);
        }

        /// <summary>
        /// Rigged tile type selector:
        /// On higher difficulties, suppresses colors that would form instant accidental cascades,
        /// forcing the player to manually search and execute swaps!
        /// </summary>
        private int ChooseTileTypeWithRigging(int x, int y)
        {
            int totalTypes = currentTileTypes.Count;
            if (UnityEngine.Random.value < riggedSpawnProbability)
            {
                // Find types that would complete a match-3
                List<int> badTypes = new List<int>();

                // Check vertical below
                if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null)
                {
                    if (grid[x, y - 1].TypeId == grid[x, y - 2].TypeId)
                        badTypes.Add(grid[x, y - 1].TypeId);
                }

                // Check horizontal adjacent
                if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null)
                {
                    if (grid[x - 1, y].TypeId == grid[x - 2, y].TypeId)
                        badTypes.Add(grid[x - 1, y].TypeId);
                }

                List<int> goodTypes = new List<int>();
                for (int t = 0; t < totalTypes; t++)
                {
                    if (!badTypes.Contains(t)) goodTypes.Add(t);
                }

                if (goodTypes.Count > 0)
                {
                    return goodTypes[UnityEngine.Random.Range(0, goodTypes.Count)];
                }
            }

            return UnityEngine.Random.Range(0, totalTypes);
        }

        #endregion

        #region Move Validation & Guarantee Combos

        /// <summary>
        /// Validates whether any legal match-3 move exists on the current board.
        /// </summary>
        public bool HasPossibleValidMoves()
        {
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    if (grid[x, y] == null) continue;

                    // Try right swap
                    if (x < COLUMNS - 1 && grid[x + 1, y] != null)
                    {
                        if (WouldSwapFormMatch(x, y, x + 1, y)) return true;
                    }

                    // Try up swap
                    if (y < ROWS - 1 && grid[x, y + 1] != null)
                    {
                        if (WouldSwapFormMatch(x, y, x, y + 1)) return true;
                    }
                }
            }
            return false;
        }

        private bool WouldSwapFormMatch(int x1, int y1, int x2, int y2)
        {
            int typeA = grid[x1, y1].TypeId;
            int typeB = grid[x2, y2].TypeId;
            if (typeA == typeB) return false;

            return CheckMatchAtVirtual(x2, y2, typeA, x1, y1) || CheckMatchAtVirtual(x1, y1, typeB, x2, y2);
        }

        private bool CheckMatchAtVirtual(int x, int y, int testType, int ignoreX, int ignoreY)
        {
            // Horizontal count
            int horiz = 1;
            // Scan left
            for (int i = x - 1; i >= 0; i--)
            {
                if (i == ignoreX && y == ignoreY) break;
                if (grid[i, y] != null && grid[i, y].TypeId == testType) horiz++;
                else break;
            }
            // Scan right
            for (int i = x + 1; i < COLUMNS; i++)
            {
                if (i == ignoreX && y == ignoreY) break;
                if (grid[i, y] != null && grid[i, y].TypeId == testType) horiz++;
                else break;
            }
            if (horiz >= 3) return true;

            // Vertical count
            int vert = 1;
            // Scan down
            for (int j = y - 1; j >= 0; j--)
            {
                if (x == ignoreX && j == ignoreY) break;
                if (grid[x, j] != null && grid[x, j].TypeId == testType) vert++;
                else break;
            }
            // Scan up
            for (int j = y + 1; j < ROWS; j++)
            {
                if (x == ignoreX && j == ignoreY) break;
                if (grid[x, j] != null && grid[x, j].TypeId == testType) vert++;
                else break;
            }
            if (vert >= 3) return true;

            return false;
        }

        private void EnsureAtLeastOneMoveExists()
        {
            // Inject a guaranteed match-pair in center
            int cx = COLUMNS / 2;
            int cy = ROWS / 2;
            int targetType = grid[cx, cy].TypeId;

            // Create a setup: [T, T, Other, T]
            if (cx < COLUMNS - 3 && grid[cx + 1, cy] != null && grid[cx + 3, cy] != null)
            {
                SetTileType(cx + 1, cy, targetType);
                SetTileType(cx + 3, cy, targetType);
            }
        }

        private void SetTileType(int x, int y, int newType)
        {
            if (grid[x, y] == null) return;
            Destroy(grid[x, y].gameObject);
            SpawnTileAt(x, y, newType, false);
        }

        private IEnumerator ReshuffleBoardRoutine()
        {
            ShowComboBanner("MENGACAK PAPAN...");
            PlaySound(sfxWhoosh, 0.8f);

            yield return new WaitForSeconds(0.3f);

            // Reshuffle existing tiles
            List<int> allTypes = new List<int>();
            for (int x = 0; x < COLUMNS; x++)
            {
                for (int y = 0; y < ROWS; y++)
                {
                    if (grid[x, y] != null) allTypes.Add(grid[x, y].TypeId);
                }
            }

            int attempts = 0;
            do
            {
                attempts++;
                // Fisher-Yates shuffle
                for (int i = allTypes.Count - 1; i > 0; i--)
                {
                    int k = UnityEngine.Random.Range(0, i + 1);
                    int temp = allTypes[i];
                    allTypes[i] = allTypes[k];
                    allTypes[k] = temp;
                }

                int idx = 0;
                for (int x = 0; x < COLUMNS; x++)
                {
                    for (int y = 0; y < ROWS; y++)
                    {
                        SetTileType(x, y, allTypes[idx++]);
                    }
                }
            }
            while (!HasPossibleValidMoves() && attempts < 10);

            if (!HasPossibleValidMoves()) EnsureAtLeastOneMoveExists();

            yield return new WaitForSeconds(0.2f);
            HideComboBanner();
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
                MovesRemaining--;
                CurrentCombo = 1;
                UpdateHeaderUI();

                // Process matches, attack enemy, and handle cascades
                yield return ProcessMatchesAndCascades(matches);

                // Verify valid moves remain on board
                if (!HasPossibleValidMoves() && TargetCurrentHP > 0 && MovesRemaining > 0)
                {
                    yield return ReshuffleBoardRoutine();
                }

                // Check victory or defeat conditions
                if (TargetCurrentHP <= 0)
                {
                    yield return HandleVictorySequence();
                }
                else if (MovesRemaining <= 0)
                {
                    yield return HandleDefeatSequence("⚠️ LANGKAH HABIS!");
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

            // 1. Horizontal Check (across 15 columns)
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

            // 2. Vertical Check (across 6 rows)
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

                // Base damage per rocket on hit: 40, multiplied by combo
                int baseTileDamage = 40;
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

                // Launch Attack Rockets straight upwards from the matched tiles!
                foreach (var tile in matchedTiles)
                {
                    if (tile == null) continue;

                    Vector2 tilePosInProjLayer = (Vector2)projParent.InverseTransformPoint(tile.transform.position);

                    PlaySound(sfxWhoosh, 0.65f);

                    Match3Projectile.SpawnRocket(
                        projParent,
                        tilePosInProjLayer,
                        tile.ItemSprite,
                        tile.Symbol,
                        tile.ThemeColor,
                        damagePerTile,
                        CurrentCombo,
                        OnEnemyHit,
                        OnEnemyMiss
                    );

                    grid[tile.GridX, tile.GridY] = null;
                    tile.AnimateMatchAndDestroy(0.24f);
                }

                yield return new WaitForSeconds(0.26f);

                // Gravity: drop tiles down with rigged spawner
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
                        int newY = y - emptyCount;
                        Match3Tile tile = grid[x, y];
                        grid[x, y] = null;
                        grid[x, newY] = tile;
                        tile.SetGridPosition(x, newY);

                        Vector2 targetPos = CalculateTilePosition(x, newY);
                        tile.DropTo(targetPos, dropDuration);
                    }
                }

                for (int i = 0; i < emptyCount; i++)
                {
                    int spawnY = ROWS - emptyCount + i;
                    int chosenType = ChooseTileTypeWithRigging(x, spawnY);
                    SpawnTileAt(x, spawnY, chosenType, true);
                }
            }

            yield return new WaitForSeconds(dropDuration + 0.05f);
        }

        #endregion

        #region Enemy Damage, Hit & Miss Reactions

        public void OnEnemyHit(int damage, int combo, Vector2 hitPos, Color hitColor)
        {
            if (isEnemyDefeated) return;

            PlaySound(sfxHit, 0.95f);

            // Accumulate Total Damage
            TotalDamageAccumulated += damage;

            // Deduct HP
            TargetCurrentHP = Mathf.Max(0, TargetCurrentHP - damage);
            UpdateHPBarUI(false);
            UpdateHeaderUI();

            // Spawn Impact Explosion Particles & Shockwave
            SpawnImpactExplosion(hitPos, hitColor);

            // Floating Damage Number
            SpawnFloatingDamage(damage, combo, hitPos);

            // Enemy Shake & Red Flash
            StartCoroutine(EnemyHitReactionRoutine());

            // Switch to enraged sprite if low HP
            if (TargetCurrentHP <= TargetMaxHP * 0.35f && enemyImage != null && enemyEnragedSprite != null)
            {
                enemyImage.sprite = enemyEnragedSprite;
            }

            // Victory Check
            if (TargetCurrentHP <= 0 && !isEnemyDefeated)
            {
                StartCoroutine(HandleVictorySequence());
            }
        }

        public void OnEnemyMiss(Vector2 missPos)
        {
            if (isEnemyDefeated) return;

            SpawnFloatingMiss(missPos);
            SpawnSmokePuff(missPos);
        }

        private void SpawnImpactExplosion(Vector2 pos, Color color)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;
            Sprite white = GetWhiteSprite();

            // Expanding Shockwave Ring
            GameObject ringObj = new GameObject("ImpactShockwave", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ringObj.transform.SetParent(layer, false);
            RectTransform ringRT = ringObj.GetComponent<RectTransform>();
            ringRT.sizeDelta = new Vector2(40f, 40f);
            ringRT.anchoredPosition = pos;
            Image ringImg = ringObj.GetComponent<Image>();
            ringImg.sprite = white;
            ringImg.color = new Color(color.r, color.g, color.b, 0.9f);
            ringImg.raycastTarget = false;
            StartCoroutine(AnimateShockwave(ringRT, ringImg));

            // Radial Spark Particles
            int particleCount = 8;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = i * (360f / particleCount) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float speed = UnityEngine.Random.Range(130f, 220f);

                GameObject sparkObj = new GameObject($"Spark_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                sparkObj.transform.SetParent(layer, false);
                RectTransform sparkRT = sparkObj.GetComponent<RectTransform>();
                sparkRT.sizeDelta = new Vector2(16f, 16f);
                sparkRT.anchoredPosition = pos;
                Image sparkImg = sparkObj.GetComponent<Image>();
                sparkImg.sprite = white;
                sparkImg.color = Color.Lerp(color, Color.white, 0.5f);
                sparkImg.raycastTarget = false;

                StartCoroutine(AnimateSpark(sparkRT, sparkImg, dir * speed));
            }
        }

        private void SpawnSmokePuff(Vector2 pos)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;
            Sprite white = GetWhiteSprite();

            for (int i = 0; i < 4; i++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float speed = UnityEngine.Random.Range(40f, 90f);

                GameObject smokeObj = new GameObject("SmokePuff", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                smokeObj.transform.SetParent(layer, false);
                RectTransform smokeRT = smokeObj.GetComponent<RectTransform>();
                smokeRT.sizeDelta = new Vector2(18f, 18f);
                smokeRT.anchoredPosition = pos;
                Image smokeImg = smokeObj.GetComponent<Image>();
                smokeImg.sprite = white;
                smokeImg.color = new Color(0.85f, 0.85f, 0.85f, 0.65f);
                smokeImg.raycastTarget = false;

                StartCoroutine(AnimateSpark(smokeRT, smokeImg, dir * speed));
            }
        }

        private IEnumerator AnimateShockwave(RectTransform rt, Image img)
        {
            float duration = 0.28f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.4f;
            Vector3 endScale = Vector3.one * 2.6f;

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
            float duration = 0.32f;
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
                float rx = UnityEngine.Random.Range(-14f, 14f);
                float ry = UnityEngine.Random.Range(-10f, 10f);
                if (enemyRect != null)
                {
                    enemyRect.anchoredPosition = new Vector2(bossCurrentX + rx, bossBaseY + ry);
                }
                yield return null;
            }

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

            float rx = UnityEngine.Random.Range(-35f, 35f);
            float ry = UnityEngine.Random.Range(-10f, 20f);
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
                txt.color = new Color(1f, 0.92f, 0.25f, 1f);
            }
            else
            {
                txt.text = $"💥 -{damage}";
                txt.fontSize = 28;
                txt.fontStyle = FontStyle.Bold;
                txt.color = new Color(1f, 0.35f, 0.35f, 1f);
            }

            StartCoroutine(AnimateFloatingText(rt, txt));
        }

        private void SpawnFloatingMiss(Vector2 pos)
        {
            Transform layer = damageTextLayer != null ? damageTextLayer : combatPanel.transform;

            GameObject missObj = new GameObject("MissText", typeof(RectTransform));
            missObj.transform.SetParent(layer, false);
            RectTransform rt = missObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(240, 50);
            rt.anchoredPosition = new Vector2(pos.x, pos.y + 20f);

            Text txt = missObj.AddComponent<Text>();
            txt.font = GetGameFont();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            txt.text = "MELESET!";
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);

            StartCoroutine(AnimateFloatingText(rt, txt));
        }

        private IEnumerator AnimateFloatingText(RectTransform rt, Text txt)
        {
            float duration = 0.7f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, 60f);

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
            isBossMoving = false;
            IsTimerRunning = false;
            CanPlayerMove = false;

            PlaySound(sfxBonus, 1f);

            if (hitboxIndicatorObj != null) hitboxIndicatorObj.SetActive(false);

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
                    int totalSec = Mathf.CeilToInt(TimeRemaining);
                    int mins = totalSec / 60;
                    int secs = totalSec % 60;

                    victoryStatsText.text = $"Target Berhasil Ditumbangkan!\n\n" +
                                            $"• Total Kerusakan: 💥 {TotalDamageAccumulated:N0} DMG\n" +
                                            $"• Sisa Langkah: 🎯 {MovesRemaining}/{MaxTurns}\n" +
                                            $"• Sisa Waktu: ⏱️ {mins:D2}:{secs:D2}\n" +
                                            $"• Total EXP Diperoleh: 🌟 +{Score:N0} EXP\n" +
                                            $"• Combo Maksimal: x{MaxComboReached}\n\n" +
                                            $"Tembakan roket forensik berhasil memukul mundur Doctor Sawit Man!";
                }
            }
        }

        private IEnumerator HandleTimeOutDefeat()
        {
            yield return HandleDefeatSequence("⏱️ WAKTU HABIS!", "Waktu penyelidikan telah habis! Doctor Sawit Man berhasil melarikan diri dari sergapan tim C.A.R.E.");
        }

        private IEnumerator HandleDefeatSequence(string title = "⚠️ LANGKAH HABIS!", string desc = null)
        {
            CanPlayerMove = false;
            IsTimerRunning = false;
            yield return new WaitForSeconds(0.4f);

            if (defeatModal != null)
            {
                defeatModal.SetActive(true);
                if (defeatTitleText != null) defeatTitleText.text = title;
                if (defeatDescText != null && !string.IsNullOrEmpty(desc)) defeatDescText.text = desc;
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
                stageTitleText.text = $"⚔️ TAHAP {currentStageNumber} - MISI {currentLevelNumber}";
            }

            if (totalDmgText != null)
            {
                totalDmgText.text = $"{TotalDamageAccumulated:N0} DMG";
            }

            if (turnsText != null)
            {
                turnsText.text = $"{MovesRemaining} / {MaxTurns}";
                turnsText.color = MovesRemaining <= 4 ? new Color(1f, 0.35f, 0.35f) : Color.white;
            }

            if (patrolSpeedText != null)
            {
                patrolSpeedText.text = $"{bossPatrolSpeed:F0} px/s";
            }

            if (scoreText != null)
            {
                scoreText.text = $"+{Score:N0} EXP";
            }

            UpdateTimerUI();
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                int totalSec = Mathf.CeilToInt(TimeRemaining);
                int mins = totalSec / 60;
                int secs = totalSec % 60;
                timerText.text = $"{mins:D2}:{secs:D2}";

                if (TimeRemaining <= 10f)
                {
                    float pulse = Mathf.PingPong(Time.time * 6f, 0.45f);
                    timerText.color = new Color(1f, pulse, pulse);
                }
                else
                {
                    timerText.color = new Color(0.4f, 0.95f, 0.95f);
                }
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

            if (hpBarFill.sprite == null)
            {
                hpBarFill.sprite = GetWhiteSprite();
            }

            hpBarFill.fillAmount = ratio;

            RectTransform rt = hpBarFill.rectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(ratio, 1f);
                rt.offsetMin = new Vector2(2f, 2f);
                rt.offsetMax = new Vector2(-2f, -2f);
            }

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
