// ==========================================================================
// C.A.R.E - Main Web Game Controller
// Matches Figma Scenarios and UI
// ==========================================================================

class CareGame {
  constructor() {
    this.currentScreen = "title";
    this.storyIndex = 0;
    this.typewriterTimer = null;
    this.isTyping = false;
    this.currentLevel = null;
    this.unlockedLevels = [1];
    this.dossierAnimal = null;
    this.activeDossierTab = "desc";

    this.gridSize = 7;
    this.board = [];
    this.selectedTile = null;
    this.isProcessingMove = false;
    this.score = 0;
    this.movesLeft = 25;
    this.bossMaxHp = 1000;
    this.bossHp = 1000;
    this.combatPhase = "intro";

    this.tileTypes = [
      { id: "red", icon: "🔥", class: "tile-red", label: "Api" },
      { id: "blue", icon: "💧", class: "tile-blue", label: "Air" },
      { id: "yellow", icon: "☀️", class: "tile-yellow", label: "Surya" },
      { id: "green", icon: "🌿", class: "tile-green", label: "Hutan" },
      { id: "purple", icon: "✨", class: "tile-purple", label: "Benih" },
      { id: "white", icon: "🩹", class: "tile-white", label: "P3K" }
    ];

    this.initDOM();
    this.bindEvents();
    this.showScreen("title");
  }

  initDOM() {
    this.screens = {
      title: document.getElementById("screen-title"),
      story: document.getElementById("screen-story"),
      levels: document.getElementById("screen-levels"),
      dossier: document.getElementById("screen-dossier"),
      gameplay: document.getElementById("screen-gameplay")
    };

    this.storyText = document.getElementById("story-text");
    this.storySpeaker = document.getElementById("story-speaker");
    this.storyPortrait = document.getElementById("story-portrait");
    this.storyPortraitBox = document.getElementById("story-portrait-box");
    this.storyContainer = document.getElementById("story-container");
    this.storyArrow = document.getElementById("story-arrow");

    this.levelCards = document.querySelectorAll(".level-card");

    this.dossierTitle = document.getElementById("dossier-title");
    this.dossierLatin = document.getElementById("dossier-latin");
    this.dossierImg = document.getElementById("dossier-img");
    this.dossierDescContent = document.getElementById("dossier-desc-content");
    this.dossierTaxoContent = document.getElementById("dossier-taxo-content");

    this.stageTitleBadge = document.getElementById("stage-title-badge");
    this.scoreDisplay = document.getElementById("score-display");
    this.movesDisplay = document.getElementById("moves-display");
    this.bossNameLabel = document.getElementById("boss-name-label");
    this.bossHpFill = document.getElementById("boss-hp-fill");
    this.bossHpText = document.getElementById("boss-hp-text");
    this.bossSprite = document.getElementById("boss-sprite");
    this.companionAvatar = document.getElementById("companion-avatar");
    this.companionName = document.getElementById("companion-name");
    this.match3Board = document.getElementById("match3-board");

    this.pauseModal = document.getElementById("pause-modal");
    this.combatDialogModal = document.getElementById("combat-dialog-modal");
    this.combatDialogSpeaker = document.getElementById("combat-dialog-speaker");
    this.combatDialogText = document.getElementById("combat-dialog-text");
    this.combatDialogPortrait = document.getElementById("combat-dialog-portrait");
  }

  bindEvents() {
    document.getElementById("btn-audio-toggle").addEventListener("click", () => {
      const isMuted = audioMgr.toggleMute();
      document.getElementById("btn-audio-toggle").textContent = isMuted ? "🔇" : "🔊";
      audioMgr.playSFX("click");
    });

    document.getElementById("title-content").addEventListener("click", () => {
      audioMgr.playSFX("click");
      audioMgr.playBGM("title");
      this.startStory();
    });

    this.storyContainer.addEventListener("click", () => {
      this.advanceStory();
    });

    document.getElementById("btn-story-skip").addEventListener("click", (e) => {
      e.stopPropagation();
      audioMgr.playSFX("skip");
      this.showLevels();
    });

    document.getElementById("btn-levels-back").addEventListener("click", () => {
      audioMgr.playSFX("click");
      this.showScreen("title");
      audioMgr.playBGM("title");
    });

    this.levelCards.forEach((card) => {
      card.addEventListener("click", () => {
        const levelId = parseInt(card.dataset.level);
        if (this.unlockedLevels.includes(levelId)) {
          audioMgr.playSFX("click");
          this.selectLevel(levelId);
        } else {
          audioMgr.playSFX("hit");
          this.shakeElement(card);
        }
      });
    });

    document.getElementById("tab-desc-btn").addEventListener("click", () => {
      this.setDossierTab("desc");
    });
    document.getElementById("tab-taxo-btn").addEventListener("click", () => {
      this.setDossierTab("taxo");
    });

    document.getElementById("btn-dossier-close").addEventListener("click", () => {
      audioMgr.playSFX("click");
      this.showLevels();
    });

    document.getElementById("btn-mulai-game").addEventListener("click", () => {
      audioMgr.playSFX("whoosh");
      this.startLevelGameplay();
    });

    document.getElementById("btn-game-pause").addEventListener("click", () => {
      audioMgr.playSFX("click");
      this.pauseModal.style.display = "flex";
    });

    document.getElementById("btn-pause-resume").addEventListener("click", () => {
      audioMgr.playSFX("click");
      this.pauseModal.style.display = "none";
    });

    document.getElementById("btn-pause-mute").addEventListener("click", () => {
      const isMuted = audioMgr.toggleMute();
      document.getElementById("btn-pause-mute").textContent = isMuted ? "Suara: Mati (OFF)" : "Suara: Nyala (ON)";
      document.getElementById("btn-audio-toggle").textContent = isMuted ? "🔇" : "🔊";
      audioMgr.playSFX("click");
    });

    document.getElementById("btn-pause-quit").addEventListener("click", () => {
      audioMgr.playSFX("click");
      this.pauseModal.style.display = "none";
      this.showLevels();
    });

    this.combatDialogModal.addEventListener("click", () => {
      this.advanceCombatDialog();
    });
  }

  showScreen(screenName) {
    Object.values(this.screens).forEach((s) => s.classList.remove("active"));
    if (this.screens[screenName]) {
      this.screens[screenName].classList.add("active");
      this.currentScreen = screenName;
    }
  }

  startStory() {
    this.storyIndex = 0;
    this.showScreen("story");
    audioMgr.playBGM("story");
    this.renderStoryStep();
  }

  renderStoryStep() {
    if (this.storyIndex >= STORY_PROLOGUE.length) {
      this.showLevels();
      return;
    }

    const item = STORY_PROLOGUE[this.storyIndex];

    if (item.cardPos === "center") {
      this.storyContainer.classList.add("center-layout");
    } else {
      this.storyContainer.classList.remove("center-layout");
    }

    if (item.portrait) {
      this.storyPortraitBox.style.display = "flex";
      this.storyPortrait.src = item.portrait;
      this.storySpeaker.textContent = item.speaker;
      this.storySpeaker.style.display = "block";
    } else {
      this.storyPortraitBox.style.display = "none";
      if (item.speaker && item.speaker !== "NARATOR") {
        this.storySpeaker.textContent = item.speaker;
        this.storySpeaker.style.display = "block";
      } else {
        this.storySpeaker.style.display = "none";
      }
    }

    if (item.isFinalIntro) {
      this.storyText.innerHTML = `<span class="story-center-banner">${item.text}</span>`;
      this.isTyping = false;
      this.storyArrow.style.display = "flex";
      return;
    }

    this.typewriteText(item.text);
  }

  typewriteText(fullText) {
    if (this.typewriterTimer) {
      clearInterval(this.typewriterTimer);
    }

    this.storyText.textContent = "";
    this.isTyping = true;
    this.storyArrow.style.display = "none";

    let i = 0;
    const speed = 25;

    this.typewriterTimer = setInterval(() => {
      if (i < fullText.length) {
        this.storyText.textContent += fullText.charAt(i);
        if (i % 2 === 0) {
          audioMgr.playTypewriterBlip(480 + (i % 5) * 20);
        }
        i++;
      } else {
        clearInterval(this.typewriterTimer);
        this.isTyping = false;
        this.storyArrow.style.display = "flex";
      }
    }, speed);
  }

  advanceStory() {
    const currentItem = STORY_PROLOGUE[this.storyIndex];
    if (this.isTyping) {
      clearInterval(this.typewriterTimer);
      this.storyText.textContent = currentItem.text;
      this.isTyping = false;
      this.storyArrow.style.display = "flex";
      audioMgr.playSFX("page");
    } else {
      audioMgr.playSFX("click");
      this.storyIndex++;
      this.renderStoryStep();
    }
  }

  showLevels() {
    this.showScreen("levels");
    audioMgr.playBGM("hub");

    this.levelCards.forEach((card) => {
      const levelId = parseInt(card.dataset.level);
      const isUnlocked = this.unlockedLevels.includes(levelId);
      const lockOverlay = card.querySelector(".level-card-lock-overlay");

      if (isUnlocked) {
        card.classList.remove("locked");
        if (lockOverlay) lockOverlay.style.display = "none";
      } else {
        card.classList.add("locked");
        if (lockOverlay) lockOverlay.style.display = "flex";
      }
    });
  }

  selectLevel(levelId) {
    const levelData = LEVELS.find((l) => l.id === levelId);
    if (!levelData) return;

    this.currentLevel = levelData;
    this.showAnimalDossier(levelData.targetAnimal);
  }

  showAnimalDossier(animalKey) {
    const dossier = ANIMAL_DOSSIERS[animalKey];
    if (!dossier) return;

    this.dossierAnimal = dossier;
    this.dossierTitle.textContent = dossier.name;
    this.dossierLatin.textContent = dossier.latin;
    this.dossierImg.src = dossier.image;

    this.dossierDescContent.innerHTML = `
      <h4>Dampak Kebakaran Hutan & Lahan</h4>
      <p>${dossier.desc}</p>
    `;

    this.dossierTaxoContent.innerHTML = `
      <h4>Taksonomi</h4>
      <p>Famili: ${dossier.taxonomy.famili}<br>
         Genus: ${dossier.taxonomy.genus}<br>
         Spesies: ${dossier.taxonomy.spesies}</p>
      <h4>Habitat Alami</h4>
      <p>${dossier.habitat}</p>
      <h4>Sumber Pakan</h4>
      <p>${dossier.makanan}</p>
    `;

    this.setDossierTab("desc");
    this.showScreen("dossier");
  }

  setDossierTab(tab) {
    this.activeDossierTab = tab;
    audioMgr.playSFX("page");

    const descBtn = document.getElementById("tab-desc-btn");
    const taxoBtn = document.getElementById("tab-taxo-btn");

    if (tab === "desc") {
      descBtn.classList.add("active");
      taxoBtn.classList.remove("active");
      this.dossierDescContent.style.display = "block";
      this.dossierTaxoContent.style.display = "none";
    } else {
      descBtn.classList.remove("active");
      taxoBtn.classList.add("active");
      this.dossierDescContent.style.display = "none";
      this.dossierTaxoContent.style.display = "block";
    }
  }

  startLevelGameplay() {
    this.showScreen("gameplay");
    const lvl = this.currentLevel;

    const bgmKey = lvl.id === 1 ? "combat1" : lvl.id === 2 ? "combat2" : "combat3";
    audioMgr.playBGM(bgmKey);

    this.score = 0;
    this.movesLeft = lvl.id === 1 ? 25 : lvl.id === 2 ? 22 : 20;
    this.bossMaxHp = lvl.bossHp;
    this.bossHp = lvl.bossHp;

    this.stageTitleBadge.textContent = `Stage ${lvl.id}: ${lvl.title}`;
    this.scoreDisplay.textContent = `Skor: 0 / ${lvl.targetScore}`;
    this.movesDisplay.textContent = `Langkah: ${this.movesLeft}`;
    this.bossNameLabel.textContent = "Dr. Sawit Man";
    this.updateBossHpUI();

    this.bossSprite.src = lvl.bossCombatPose;

    const dossier = ANIMAL_DOSSIERS[lvl.targetAnimal];
    this.companionAvatar.src = dossier.image;
    this.companionName.textContent = dossier.name;

    this.initBoard();

    this.combatPhase = "intro";
    this.combatDialogIndex = 0;
    this.showCombatDialogue(lvl.introDialog);
  }

  showCombatDialogue(dialogList) {
    this.activeDialogList = dialogList;
    this.combatDialogIndex = 0;
    this.combatDialogModal.style.display = "flex";
    this.renderCombatDialogueStep();
  }

  renderCombatDialogueStep() {
    if (this.combatDialogIndex >= this.activeDialogList.length) {
      this.combatDialogModal.style.display = "none";
      return;
    }

    const item = this.activeDialogList[this.combatDialogIndex];
    this.combatDialogSpeaker.textContent = item.speaker;
    this.combatDialogText.textContent = item.text;

    if (item.portrait) {
      this.combatDialogPortrait.style.display = "block";
      this.combatDialogPortrait.src = item.portrait;
    } else {
      this.combatDialogPortrait.style.display = "none";
    }

    audioMgr.playTypewriterBlip(520);
  }

  advanceCombatDialog() {
    audioMgr.playSFX("click");
    this.combatDialogIndex++;
    if (this.combatDialogIndex >= this.activeDialogList.length) {
      this.combatDialogModal.style.display = "none";
      if (this.combatPhase === "defeat") {
        this.completeLevel();
      }
    } else {
      this.renderCombatDialogueStep();
    }
  }

  updateBossHpUI() {
    const pct = Math.max(0, Math.min(100, (this.bossHp / this.bossMaxHp) * 100));
    this.bossHpFill.style.width = `${pct}%`;
    this.bossHpText.textContent = `${this.bossHp} / ${this.bossMaxHp} HP`;
  }

  initBoard() {
    this.match3Board.innerHTML = "";
    this.board = [];

    for (let r = 0; r < this.gridSize; r++) {
      this.board[r] = [];
      for (let c = 0; c < this.gridSize; c++) {
        let tileType;
        do {
          tileType = this.getRandomTileType();
        } while (
          (r >= 2 && this.board[r - 1][c].type.id === tileType.id && this.board[r - 2][c].type.id === tileType.id) ||
          (c >= 2 && this.board[r][c - 1].type.id === tileType.id && this.board[r][c - 2].type.id === tileType.id)
        );

        const tileEl = document.createElement("div");
        tileEl.className = `tile ${tileType.class}`;
        tileEl.textContent = tileType.icon;
        tileEl.dataset.row = r;
        tileEl.dataset.col = c;

        tileEl.addEventListener("click", () => this.onTileClick(r, c));

        this.match3Board.appendChild(tileEl);
        this.board[r][c] = { type: tileType, el: tileEl };
      }
    }
  }

  getRandomTileType() {
    const idx = Math.floor(Math.random() * this.tileTypes.length);
    return this.tileTypes[idx];
  }

  onTileClick(row, col) {
    if (this.isProcessingMove || this.bossHp <= 0 || this.movesLeft <= 0) return;

    audioMgr.playSFX("click");
    const current = this.board[row][col];

    if (!this.selectedTile) {
      this.selectedTile = { row, col };
      current.el.classList.add("selected");
    } else {
      const prevRow = this.selectedTile.row;
      const prevCol = this.selectedTile.col;

      this.board[prevRow][prevCol].el.classList.remove("selected");

      const isAdjacent =
        (Math.abs(row - prevRow) === 1 && col === prevCol) ||
        (Math.abs(col - prevCol) === 1 && row === prevRow);

      if (isAdjacent) {
        this.swapTiles(prevRow, prevCol, row, col);
      } else {
        this.selectedTile = { row, col };
        current.el.classList.add("selected");
      }
    }
  }

  async swapTiles(r1, c1, r2, c2) {
    this.isProcessingMove = true;
    this.selectedTile = null;

    const tempType = this.board[r1][c1].type;
    this.board[r1][c1].type = this.board[r2][c2].type;
    this.board[r2][c2].type = tempType;

    this.renderBoardDOM();
    audioMgr.playSFX("whoosh");

    await this.delay(200);

    const matches = this.findMatches();
    if (matches.length > 0) {
      this.movesLeft--;
      this.movesDisplay.textContent = `Langkah: ${this.movesLeft}`;
      await this.resolveMatches(matches);
    } else {
      this.board[r2][c2].type = this.board[r1][c1].type;
      this.board[r1][c1].type = tempType;
      this.renderBoardDOM();
      audioMgr.playSFX("hit");
    }

    this.isProcessingMove = false;
  }

  renderBoardDOM() {
    for (let r = 0; r < this.gridSize; r++) {
      for (let c = 0; c < this.gridSize; c++) {
        const tileData = this.board[r][c];
        tileData.el.className = `tile ${tileData.type.class}`;
        tileData.el.textContent = tileData.type.icon;
      }
    }
  }

  findMatches() {
    const matchedCoords = new Set();

    for (let r = 0; r < this.gridSize; r++) {
      for (let c = 0; c < this.gridSize - 2; c++) {
        const typeId = this.board[r][c].type.id;
        if (
          this.board[r][c + 1].type.id === typeId &&
          this.board[r][c + 2].type.id === typeId
        ) {
          matchedCoords.add(`${r},${c}`);
          matchedCoords.add(`${r},${c + 1}`);
          matchedCoords.add(`${r},${c + 2}`);
        }
      }
    }

    for (let c = 0; c < this.gridSize; c++) {
      for (let r = 0; r < this.gridSize - 2; r++) {
        const typeId = this.board[r][c].type.id;
        if (
          this.board[r + 1][c].type.id === typeId &&
          this.board[r + 2][c].type.id === typeId
        ) {
          matchedCoords.add(`${r},${c}`);
          matchedCoords.add(`${r + 1},${c}`);
          matchedCoords.add(`${r + 2},${c}`);
        }
      }
    }

    return Array.from(matchedCoords).map((str) => {
      const [r, c] = str.split(",").map(Number);
      return { r, c, type: this.board[r][c].type };
    });
  }

  async resolveMatches(matches) {
    let combo = 1;

    while (matches.length > 0) {
      audioMgr.playSFX("bonus");

      const matchScore = matches.length * 50 * combo;
      const damage = matches.length * 40 * combo;

      this.score += matchScore;
      this.bossHp = Math.max(0, this.bossHp - damage);

      this.scoreDisplay.textContent = `Skor: ${this.score} / ${this.currentLevel.targetScore}`;
      this.updateBossHpUI();

      this.showDamagePopup(`-${damage} HP!`);

      matches.forEach(({ r, c }) => {
        this.board[r][c].type = null;
      });

      this.renderBoardDOM();
      await this.delay(180);

      for (let c = 0; c < this.gridSize; c++) {
        let emptyRow = this.gridSize - 1;
        for (let r = this.gridSize - 1; r >= 0; r--) {
          if (this.board[r][c].type !== null) {
            if (emptyRow !== r) {
              this.board[emptyRow][c].type = this.board[r][c].type;
              this.board[r][c].type = null;
            }
            emptyRow--;
          }
        }
        for (let r = emptyRow; r >= 0; r--) {
          this.board[r][c].type = this.getRandomTileType();
        }
      }

      this.renderBoardDOM();
      await this.delay(220);

      matches = this.findMatches();
      combo++;

      if (this.bossHp <= 0) {
        this.handleBossDefeat();
        return;
      }
    }

    if (this.movesLeft <= 0 && this.bossHp > 0) {
      audioMgr.playSFX("hit");
      alert("Langkah habis! Coba lagi dan gunakan kombo air & pemadam kebakaran!");
      this.startLevelGameplay();
    }
  }

  showDamagePopup(text) {
    const floater = document.createElement("div");
    floater.className = "floating-text";
    floater.textContent = text;
    floater.style.left = "45%";
    floater.style.top = "30%";
    document.getElementById("screen-gameplay").appendChild(floater);

    setTimeout(() => {
      floater.remove();
    }, 850);
  }

  handleBossDefeat() {
    this.combatPhase = "defeat";
    const lvl = this.currentLevel;

    this.bossSprite.src = lvl.bossDefeatPose;
    audioMgr.playSFX("badge");

    setTimeout(() => {
      this.showCombatDialogue(lvl.defeatDialog);
    }, 600);
  }

  completeLevel() {
    const nextLevelId = this.currentLevel.id + 1;
    if (nextLevelId <= 3 && !this.unlockedLevels.includes(nextLevelId)) {
      this.unlockedLevels.push(nextLevelId);
    }

    audioMgr.playSFX("badge");
    alert(`Selamat! Kamu telah menyelesaikan ${this.currentLevel.title}!`);
    this.showLevels();
  }

  shakeElement(el) {
    el.style.transform = "translateX(-8px)";
    setTimeout(() => (el.style.transform = "translateX(8px)"), 80);
    setTimeout(() => (el.style.transform = "translateX(-6px)"), 160);
    setTimeout(() => (el.style.transform = "translateX(6px)"), 240);
    setTimeout(() => (el.style.transform = ""), 320);
  }

  delay(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}

window.addEventListener("DOMContentLoaded", () => {
  window.careGame = new CareGame();
});
